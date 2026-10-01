using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class GiaHanController : Controller
{
    private readonly IGiaHanRepository _giaHanRepo;
    private readonly AppDbContext _db;

    public GiaHanController(IGiaHanRepository giaHanRepo, AppDbContext db)
    {
        _giaHanRepo = giaHanRepo;
        _db = db;
    }

    // GET /GiaHan/{maPhieuMuon}
    [HttpGet]
    public async Task<IActionResult> Index(int maPhieuMuon)
    {
        var phieuMuon = await _db.PhieuMuons
            .Include(pm => pm.DocGia)
            .FirstOrDefaultAsync(pm => pm.MaPhieuMuon == maPhieuMuon);

        if (phieuMuon == null) return NotFound();

        var lichSuGiaHan = await _giaHanRepo.GetByPhieuMuonAsync(maPhieuMuon);

        ViewBag.PhieuMuon = phieuMuon;
        ViewBag.LichSuGiaHan = lichSuGiaHan;
        ViewBag.SoLanDaGiaHan = lichSuGiaHan.Count();
        return View();
    }

    // POST /GiaHan/{maPhieuMuon}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(int maPhieuMuon, string? ghiChu)
    {
        try
        {
            var maNhanVien = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var hanTraMoi = await _giaHanRepo.GiaHanAsync(maPhieuMuon, maNhanVien, ghiChu);
            TempData["Success"] = $"Gia hạn thành công. Hạn trả mới: {hanTraMoi:dd/MM/yyyy}";
            return RedirectToAction("Detail", "MuonSach", new { id = maPhieuMuon });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;

            var phieuMuon = await _db.PhieuMuons
                .Include(pm => pm.DocGia)
                .FirstOrDefaultAsync(pm => pm.MaPhieuMuon == maPhieuMuon);
            var lichSuGiaHan = await _giaHanRepo.GetByPhieuMuonAsync(maPhieuMuon);

            ViewBag.PhieuMuon = phieuMuon;
            ViewBag.LichSuGiaHan = lichSuGiaHan;
            ViewBag.SoLanDaGiaHan = lichSuGiaHan.Count();
            return View();
        }
    }
}
