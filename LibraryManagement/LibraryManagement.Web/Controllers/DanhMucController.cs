using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class DanhMucController(IDanhMucRepository danhMucRepo) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.TheLoais    = (await danhMucRepo.GetTheLoaisAsync()).ToList();
        ViewBag.TacGias     = (await danhMucRepo.GetTacGiasAsync()).ToList();
        ViewBag.NhaXuatBans = (await danhMucRepo.GetNhaXuatBansAsync()).ToList();
        return View();
    }

    // ── Thể loại ──────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AddTheLoai(string tenTheLoai) =>
        ThucHienAsync(() => danhMucRepo.AddTheLoaiAsync(tenTheLoai),
            $"Đã thêm thể loại \"{tenTheLoai?.Trim()}\".");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteTheLoai(int id) =>
        ThucHienAsync(() => danhMucRepo.DeleteTheLoaiAsync(id), "Đã xóa thể loại.");

    // ── Tác giả ───────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AddTacGia(string tenTacGia) =>
        ThucHienAsync(() => danhMucRepo.AddTacGiaAsync(tenTacGia),
            $"Đã thêm tác giả \"{tenTacGia?.Trim()}\".");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteTacGia(int id) =>
        ThucHienAsync(() => danhMucRepo.DeleteTacGiaAsync(id), "Đã xóa tác giả.");

    // ── Nhà xuất bản ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AddNXB(string tenNXB) =>
        ThucHienAsync(() => danhMucRepo.AddNhaXuatBanAsync(tenNXB),
            $"Đã thêm NXB \"{tenNXB?.Trim()}\".");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteNXB(int id) =>
        ThucHienAsync(() => danhMucRepo.DeleteNhaXuatBanAsync(id), "Đã xóa NXB.");

    private async Task<IActionResult> ThucHienAsync(Func<Task> action, string thongBao)
    {
        try
        {
            await action();
            TempData["Success"] = thongBao;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] = "Có lỗi xảy ra, vui lòng thử lại.";
        }
        return RedirectToAction(nameof(Index));
    }
}
