using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class DanhMucController : Controller
{
    private readonly AppDbContext _db;

    public DanhMucController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TheLoais = await _db.TheLoais.OrderBy(t => t.TenTheLoai).ToListAsync();
        ViewBag.TacGias = await _db.TacGias.OrderBy(t => t.TenTacGia).ToListAsync();
        ViewBag.NhaXuatBans = await _db.NhaXuatBans.OrderBy(n => n.TenNXB).ToListAsync();
        return View();
    }

    // ── Thể loại ──────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTheLoai(string tenTheLoai)
    {
        if (string.IsNullOrWhiteSpace(tenTheLoai))
        {
            TempData["Error"] = "Tên thể loại không được để trống.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            _db.TheLoais.Add(new TheLoai { TenTheLoai = tenTheLoai.Trim() });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm thể loại \"{tenTheLoai.Trim()}\".";
        }
        catch (Exception)
        {
            TempData["Error"] = "Tên thể loại đã tồn tại hoặc có lỗi xảy ra.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTheLoai(int id)
    {
        var hasSach = await _db.Sachs.AnyAsync(s => s.MaTheLoai == id);
        if (hasSach)
        {
            TempData["Error"] = "Không thể xóa: đang có sách thuộc thể loại này.";
            return RedirectToAction(nameof(Index));
        }
        var entity = await _db.TheLoais.FindAsync(id);
        if (entity != null)
        {
            _db.TheLoais.Remove(entity);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa thể loại.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ── Tác giả ───────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTacGia(string tenTacGia)
    {
        if (string.IsNullOrWhiteSpace(tenTacGia))
        {
            TempData["Error"] = "Tên tác giả không được để trống.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            _db.TacGias.Add(new TacGia { TenTacGia = tenTacGia.Trim() });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm tác giả \"{tenTacGia.Trim()}\".";
        }
        catch (Exception)
        {
            TempData["Error"] = "Có lỗi xảy ra khi thêm tác giả.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTacGia(int id)
    {
        var hasSach = await _db.Sachs.AnyAsync(s => s.MaTacGia == id);
        if (hasSach)
        {
            TempData["Error"] = "Không thể xóa: đang có sách của tác giả này.";
            return RedirectToAction(nameof(Index));
        }
        var entity = await _db.TacGias.FindAsync(id);
        if (entity != null)
        {
            _db.TacGias.Remove(entity);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa tác giả.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ── Nhà xuất bản ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNXB(string tenNXB)
    {
        if (string.IsNullOrWhiteSpace(tenNXB))
        {
            TempData["Error"] = "Tên NXB không được để trống.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            _db.NhaXuatBans.Add(new NhaXuatBan { TenNXB = tenNXB.Trim() });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm NXB \"{tenNXB.Trim()}\".";
        }
        catch (Exception)
        {
            TempData["Error"] = "Có lỗi xảy ra khi thêm NXB.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNXB(int id)
    {
        var hasSach = await _db.Sachs.AnyAsync(s => s.MaNXB == id);
        if (hasSach)
        {
            TempData["Error"] = "Không thể xóa: đang có sách của NXB này.";
            return RedirectToAction(nameof(Index));
        }
        var entity = await _db.NhaXuatBans.FindAsync(id);
        if (entity != null)
        {
            _db.NhaXuatBans.Remove(entity);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa NXB.";
        }
        return RedirectToAction(nameof(Index));
    }
}
