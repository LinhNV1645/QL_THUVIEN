using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class GiaHanController(
    IGiaHanRepository giaHanRepo,
    IPhieuMuonRepository phieuMuonRepo) : Controller
{
    // GET /GiaHan?maPhieuMuon={id}
    [HttpGet]
    public async Task<IActionResult> Index(int maPhieuMuon)
    {
        var phieuMuon = await phieuMuonRepo.GetByIdAsync(maPhieuMuon);
        if (phieuMuon == null) return NotFound();
        return View(phieuMuon);
    }

    // POST /GiaHan?maPhieuMuon={id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(int maPhieuMuon, string? ghiChu)
    {
        try
        {
            var maNhanVien = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var hanTraMoi = await giaHanRepo.GiaHanAsync(maPhieuMuon, maNhanVien, ghiChu);
            TempData["Success"] = $"Gia hạn thành công. Hạn trả mới: {hanTraMoi:dd/MM/yyyy}";
            return RedirectToAction("Detail", "MuonSach", new { id = maPhieuMuon });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            var phieuMuon = await phieuMuonRepo.GetByIdAsync(maPhieuMuon);
            if (phieuMuon == null) return NotFound();
            return View(phieuMuon);
        }
    }
}
