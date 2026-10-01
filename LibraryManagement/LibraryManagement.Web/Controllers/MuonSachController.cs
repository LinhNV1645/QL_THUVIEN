using LibraryManagement.Application.DTOs.MuonSach;
using LibraryManagement.Application.DTOs.Sach;
using LibraryManagement.Application.DTOs.DocGia;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class MuonSachController : Controller
{
    private readonly IPhieuMuonRepository _phieuMuonRepo;
    private readonly ISachRepository _sachRepo;
    private readonly IDocGiaRepository _docGiaRepo;
    private readonly AppDbContext _db;

    public MuonSachController(
        IPhieuMuonRepository phieuMuonRepo,
        ISachRepository sachRepo,
        IDocGiaRepository docGiaRepo,
        AppDbContext db)
    {
        _phieuMuonRepo = phieuMuonRepo;
        _sachRepo = sachRepo;
        _docGiaRepo = docGiaRepo;
        _db = db;
    }

    // GET /MuonSach
    public async Task<IActionResult> Index(PhieuMuonFilterDto filter)
    {
        var result = await _phieuMuonRepo.GetAllAsync(filter);
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
        string? ghiChu)
    {
        try
        {
            var danhSachSach = JsonSerializer.Deserialize<List<SachMuonItem>>(
                sachJson ?? "[]",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? [];

            if (danhSachSach.Count == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một cuốn sách.";
                return View();
            }

            var maNhanVien = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var dto = new LapPhieuMuonDto
            {
                MaDocGia = maDocGia,
                DanhSachSach = danhSachSach,
                NhanVienLap = maNhanVien,
                GhiChu = ghiChu
            };

            var maPhieu = await _phieuMuonRepo.LapPhieuMuonAsync(dto);
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
        var phieu = await _phieuMuonRepo.GetByIdAsync(id);
        if (phieu == null) return NotFound();
        return View(phieu);
    }

    // GET /MuonSach/QuaHan
    [HttpGet]
    public async Task<IActionResult> QuaHan()
    {
        var list = await _phieuMuonRepo.GetQuaHanAsync();
        return View(list);
    }

    // GET /MuonSach/TimDocGia?keyword=...  (AJAX)
    [HttpGet]
    public async Task<IActionResult> TimDocGia(string? keyword)
    {
        var result = await _docGiaRepo.GetAllAsync(keyword, null, 1, 10);
        var json = result.Items.Select(d => new
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
        var result = await _sachRepo.GetAllAsync(filter);
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
        var dangMuon = await _phieuMuonRepo.GetDangMuonByDocGiaAsync(maDocGia);
        var docGiaResult = await _docGiaRepo.GetAllAsync(null, null, 1, 1000);
        var docGia = docGiaResult.Items.FirstOrDefault(d => d.MaDocGia == maDocGia);
        if (docGia == null) return Json(null);
        return Json(new
        {
            hoTen = docGia.HoTen,
            lop = docGia.Lop ?? "",
            soSachDangMuon = dangMuon.Count()
        });
    }
}
