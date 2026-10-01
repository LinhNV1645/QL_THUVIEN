using LibraryManagement.Application.DTOs.TraSach;
using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class TraSachController(
    IPhieuTraRepository phieuTraRepo,
    IPhieuMuonRepository phieuMuonRepo,
    ICauHinhRepository cauHinhRepo) : Controller
{
    // GET /TraSach
    public async Task<IActionResult> Index()
    {
        var all = await phieuTraRepo.GetAllAsync();
        return View(all);
    }

    // GET /TraSach/LapPhieu?maPhieuMuon={id}
    [HttpGet]
    public async Task<IActionResult> LapPhieu(int? maPhieuMuon)
    {
        if (maPhieuMuon.HasValue)
        {
            var phieuMuon = await phieuMuonRepo.GetByIdAsync(maPhieuMuon.Value);
            ViewBag.PhieuMuon = phieuMuon;
            ViewBag.MucPhat = phieuMuon == null ? 0 : await LayMucPhatAsync();
        }
        return View();
    }

    // POST /TraSach/LapPhieu
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> LapPhieu(int maPhieuMuon, byte trangThaiSach, string? ghiChu)
    {
        try
        {
            var maNhanVien = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var dto = new TraSachDto
            {
                MaPhieuMuon = maPhieuMuon,
                TrangThaiSach = trangThaiSach,
                NhanVienThu = maNhanVien,
                GhiChu = ghiChu
            };

            var result = await phieuTraRepo.TraSachAsync(dto);
            TempData["TraKetQua"] = JsonSerializer.Serialize(result);
            TempData["Success"] = $"Trả sách thành công! Phiếu trả #{result.MaPhieuTra}";
            return RedirectToAction(nameof(Detail), new { id = result.MaPhieuTra });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            var phieuMuon = await phieuMuonRepo.GetByIdAsync(maPhieuMuon);
            ViewBag.PhieuMuon = phieuMuon;
            ViewBag.MucPhat = phieuMuon == null ? 0 : await LayMucPhatAsync();
            return View();
        }
    }

    // GET /TraSach/Detail/{id}
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var phieuTra = await phieuTraRepo.GetByIdAsync(id);
        if (phieuTra == null) return NotFound();

        if (TempData["TraKetQua"] is string json)
        {
            var ketQua = JsonSerializer.Deserialize<TraSachResultDto>(json);
            ViewBag.KetQua = ketQua;
        }
        return View(phieuTra);
    }

    // POST /TraSach/ThuPhat/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ThuPhat(int id)
    {
        try
        {
            await phieuTraRepo.ThuPhatAsync(id);
            TempData["Success"] = "Đã ghi nhận thu phạt thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    private async Task<decimal> LayMucPhatAsync()
    {
        var giaTri = await cauHinhRepo.GetValueAsync("MucPhatNgayTreHan");
        return decimal.TryParse(giaTri, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0;
    }
}
