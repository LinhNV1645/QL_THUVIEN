using LibraryManagement.Application.DTOs.MuonSach;
using LibraryManagement.Application.DTOs.Sach;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class MuonSachController(
    IPhieuMuonRepository phieuMuonRepo,
    ISachRepository sachRepo,
    IDocGiaRepository docGiaRepo,
    IEmailService emailService,
    AppDbContext db) : Controller
{
    // GET /MuonSach
    public async Task<IActionResult> Index(PhieuMuonFilterDto filter)
    {
        var result = await phieuMuonRepo.GetAllAsync(filter);
        ViewBag.Filter = filter;
        return View(result);
    }

    // GET /MuonSach/LapPhieu
    [HttpGet]
    public IActionResult LapPhieu()
    {
        return View();
    }

    // POST /MuonSach/LapPhieu
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> LapPhieu(
        int maDocGia,
        string sachJson,
        string? ghiChu,
        string? ngayHanTra)
    {
        try
        {
            var danhSachSach = JsonSerializer.Deserialize<List<SachMuonItem>>(
                string.IsNullOrWhiteSpace(sachJson) ? "[]" : sachJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? [];

            if (maDocGia <= 0)
            {
                TempData["Error"] = "Vui lòng chọn độc giả.";
                return View();
            }
            if (danhSachSach.Count == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một cuốn sách.";
                return View();
            }

            DateOnly? hanTra = null;
            if (!string.IsNullOrEmpty(ngayHanTra) &&
                DateOnly.TryParseExact(ngayHanTra, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var parsed))
                hanTra = parsed;

            var maNhanVien = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var dto = new LapPhieuMuonDto
            {
                MaDocGia      = maDocGia,
                DanhSachSach  = danhSachSach,
                NhanVienLap   = maNhanVien,
                GhiChu        = ghiChu,
                NgayHanTra    = hanTra
            };

            var maPhieu = await phieuMuonRepo.LapPhieuMuonAsync(dto);
            TempData["Success"] = $"Lập phiếu mượn #{maPhieu} thành công.";
            return RedirectToAction(nameof(Detail), new { id = maPhieu });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            return View();
        }
    }

    // GET /MuonSach/Detail/{id}
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var phieu = await phieuMuonRepo.GetByIdAsync(id);
        if (phieu == null) return NotFound();
        return View(phieu);
    }

    // GET /MuonSach/QuaHan
    [HttpGet]
    public async Task<IActionResult> QuaHan()
    {
        var list = await phieuMuonRepo.GetQuaHanAsync();
        return View(list);
    }

    // GET /MuonSach/TimDocGia?keyword=...  (AJAX)
    [HttpGet]
    public async Task<IActionResult> TimDocGia(string? keyword)
    {
        var result = await docGiaRepo.GetAllAsync(keyword, null, 1, 10);
        var json = result.Items
            .Where(d => d.TrangThai == 1)
            .Select(d => new
            {
                id = d.MaDocGia,
                hoTen = d.HoTen,
                lop = d.Lop ?? ""
            });
        return Json(json);
    }

    // GET /MuonSach/TimSach?keyword=...  (AJAX)
    [HttpGet]
    public async Task<IActionResult> TimSach(string? keyword)
    {
        var filter = new SachFilterDto { TuKhoa = keyword, ChiConTon = true, Page = 1, PageSize = 10 };
        var result = await sachRepo.GetAllAsync(filter);
        var json = result.Items.Select(s => new
        {
            id = s.MaSach,
            tenSach = s.TenSach,
            soLuongTon = s.SoLuongTon
        });
        return Json(json);
    }

    // GET /MuonSach/InfoDocGia?maDocGia=...  (AJAX)
    [HttpGet]
    public async Task<IActionResult> InfoDocGia(int maDocGia)
    {
        var docGia = await docGiaRepo.GetByIdAsync(maDocGia);
        if (docGia == null) return Json(null);
        var dangMuon = await phieuMuonRepo.GetDangMuonByDocGiaAsync(maDocGia);
        return Json(new
        {
            hoTen = docGia.HoTen,
            lop = docGia.Lop ?? "",
            soSachDangMuon = dangMuon.Sum(p => p.SoSachMuon),
            coPhieuQuaHan = dangMuon.Any(p => p.SoNgayTreHan > 0)
        });
    }

    // POST /MuonSach/GửiMailQuaHan — gửi email nhắc 1 phiếu quá hạn
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiMailQuaHan(int maPhieuMuon)
    {
        var phieu = await phieuMuonRepo.GetByIdAsync(maPhieuMuon);
        if (phieu == null)
        {
            TempData["Error"] = $"Không tìm thấy phiếu mượn #{maPhieuMuon}.";
            return RedirectToAction(nameof(QuaHan));
        }
        if (string.IsNullOrWhiteSpace(phieu.Email))
        {
            TempData["Error"] = $"Độc giả {phieu.TenDocGia} không có địa chỉ email.";
            return RedirectToAction(nameof(QuaHan));
        }

        var danhSachSach = string.Join("\n", phieu.DanhSachSach.Select(s => $"  • {s.TenSach}"));
        try
        {
            await emailService.SendNhacNhoQuaHanAsync(
                toEmail     : phieu.Email,
                tenDocGia   : phieu.TenDocGia,
                danhSachSach: danhSachSach,
                ngayHanTra  : phieu.NgayHanTra,
                soNgayTre   : phieu.SoNgayTreHan,
                tienPhat    : 0);
            TempData["Success"] = $"Đã gửi email nhắc nhở tới {phieu.Email}.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Gửi email thất bại: {ex.Message}";
        }
        return RedirectToAction(nameof(QuaHan));
    }

    // POST /MuonSach/GuiMailTatCaQuaHan — gửi email cho tất cả phiếu quá hạn có email
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiMailTatCaQuaHan()
    {
        var list = await phieuMuonRepo.GetQuaHanAsync();
        int sent = 0, noEmail = 0, failed = 0;

        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Email)) { noEmail++; continue; }

            var phieu = await phieuMuonRepo.GetByIdAsync(item.MaPhieuMuon);
            if (phieu == null) { failed++; continue; }

            var danhSachSach = string.Join("\n", phieu.DanhSachSach.Select(s => $"  • {s.TenSach}"));
            try
            {
                await emailService.SendNhacNhoQuaHanAsync(
                    toEmail     : item.Email,
                    tenDocGia   : item.TenDocGia,
                    danhSachSach: danhSachSach,
                    ngayHanTra  : item.NgayHanTra,
                    soNgayTre   : item.SoNgayTre,
                    tienPhat    : item.TienPhatUocTinh);
                sent++;
            }
            catch { failed++; }
        }

        TempData["Success"] = $"Đã gửi: {sent} email. Không có email: {noEmail}. Thất bại: {failed}.";
        return RedirectToAction(nameof(QuaHan));
    }

    // GET /MuonSach/LichSuDocGia?maDocGia=...  (AJAX)
    [HttpGet]
    public async Task<IActionResult> LichSuDocGia(int maDocGia)
    {
        // Evaluate to memory first — EF Core không dịch được DateOnly.ToString và string.Join
        var raw = await db.PhieuMuons
            .Include(p => p.CTPhieuMuons).ThenInclude(ct => ct.Sach)
            .Where(p => p.MaDocGia == maDocGia)
            .OrderByDescending(p => p.NgayMuon)
            .Take(10)
            .ToListAsync();

        var list = raw.Select(p => new
        {
            maPhieuMuon = p.MaPhieuMuon,
            ngayMuon    = p.NgayMuon.ToString("dd/MM/yyyy"),
            ngayHanTra  = p.NgayHanTra.ToString("dd/MM/yyyy"),
            trangThai   = p.TrangThai switch
            {
                0 => "Đã trả", 1 => "Đang mượn",
                2 => "Quá hạn", 3 => "Đã xử lý", _ => "Gia hạn"
            },
            soSach    = p.CTPhieuMuons.Count,
            sachNames = string.Join(", ", p.CTPhieuMuons.Select(ct => ct.Sach?.TenSach ?? ""))
        });

        return Json(list);
    }
}
