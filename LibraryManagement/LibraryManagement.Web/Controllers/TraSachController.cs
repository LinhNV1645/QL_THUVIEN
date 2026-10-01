using LibraryManagement.Application.DTOs.TraSach;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class TraSachController : Controller
{
    private readonly IPhieuTraRepository _phieuTraRepo;
    private readonly AppDbContext _db;

    public TraSachController(IPhieuTraRepository phieuTraRepo, AppDbContext db)
    {
        _phieuTraRepo = phieuTraRepo;
        _db = db;
    }

    // GET /TraSach
    public async Task<IActionResult> Index()
    {
        var chuaThuPhat = await _phieuTraRepo.GetChuaThuPhatAsync();
        ViewBag.ChuaThuPhat = chuaThuPhat;
        return View(chuaThuPhat);
    }

    // GET /TraSach/LapPhieu?maPhieuMuon={id}
    [HttpGet]
    public async Task<IActionResult> LapPhieu(int? maPhieuMuon)
    {
        if (maPhieuMuon.HasValue)
        {
            var phieuMuon = await _db.PhieuMuons
                .Include(pm => pm.DocGia)
                .Include(pm => pm.CTPhieuMuons).ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(pm => pm.MaPhieuMuon == maPhieuMuon.Value);
            ViewBag.PhieuMuon = phieuMuon;
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

            var result = await _phieuTraRepo.TraSachAsync(dto);
            TempData["TraKetQua"] = JsonSerializer.Serialize(result);
            TempData["Success"] = $"Trả sách thành công! Phiếu trả #{result.MaPhieuTra}";
            return RedirectToAction(nameof(Detail), new { id = result.MaPhieuTra });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            var phieuMuon = await _db.PhieuMuons
                .Include(pm => pm.DocGia)
                .FirstOrDefaultAsync(pm => pm.MaPhieuMuon == maPhieuMuon);
            ViewBag.PhieuMuon = phieuMuon;
            return View();
        }
    }

    // GET /TraSach/Detail/{id}
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var phieuTra = await _phieuTraRepo.GetByIdAsync(id);
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
            await _phieuTraRepo.ThuPhatAsync(id);
            TempData["Success"] = "Đã ghi nhận thu phạt thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
        }
        return RedirectToAction(nameof(Detail), new { id });
    }
}
