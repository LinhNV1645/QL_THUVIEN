using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Infrastructure.Persistence;
using LibraryManagement.Application.DTOs.ThongKe;
using LibraryManagement.Web.Models;

namespace LibraryManagement.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Today;
        var today = DateOnly.FromDateTime(now);

        var dto = new DashboardDto
        {
            TongDauSach     = await _db.Sachs.CountAsync(s => s.TrangThai == 1),
            TongSachTon     = await _db.Sachs.Where(s => s.TrangThai == 1).SumAsync(s => s.SoLuongTon),
            TongDocGia      = await _db.DocGias.CountAsync(d => d.TrangThai == 1),
            DangMuon        = await _db.PhieuMuons.CountAsync(pm => pm.TrangThai == 1 || pm.TrangThai == 4),
            QuaHan          = await _db.PhieuMuons.CountAsync(pm => pm.TrangThai == 1 && pm.NgayHanTra < today),
            TienPhatChuaThu = await _db.PhieuTras.Where(pt => pt.DaThuPhat == false).SumAsync(pt => (decimal?)pt.TienPhat) ?? 0,
            MuonTrongThang  = await _db.PhieuMuons.CountAsync(pm => pm.NgayMuon.Month == now.Month && pm.NgayMuon.Year == now.Year)
        };

        var quaHanList = await _db.PhieuMuons
            .Include(pm => pm.DocGia)
            .Where(pm => pm.TrangThai == 1 && pm.NgayHanTra < today)
            .OrderBy(pm => pm.NgayHanTra)
            .Take(5)
            .Select(pm => new { pm.MaPhieuMuon, pm.DocGia.HoTen, pm.NgayHanTra })
            .ToListAsync();

        ViewBag.QuaHanList = quaHanList;
        return View(dto);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
