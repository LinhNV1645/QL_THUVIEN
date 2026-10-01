using LibraryManagement.Application.DTOs.Sach;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class SachController : Controller
{
    private readonly ISachRepository _sachRepo;
    private readonly IQrService _qrService;
    private readonly AppDbContext _db;

    public SachController(ISachRepository sachRepo, IQrService qrService, AppDbContext db)
    {
        _sachRepo = sachRepo;
        _qrService = qrService;
        _db = db;
    }

    public async Task<IActionResult> Index(SachFilterDto filter)
    {
        var result = await _sachRepo.GetAllAsync(filter);
        await LoadDropdownsAsync();
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();
        return View(new CreateSachDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSachDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(dto);
        }
        try
        {
            await _sachRepo.CreateAsync(dto);
            TempData["Success"] = "Thêm sách thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            await LoadDropdownsAsync();
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sach = await _sachRepo.GetByIdAsync(id);
        if (sach == null) return NotFound();
        await LoadDropdownsAsync();
        var dto = new UpdateSachDto
        {
            MaSach = sach.MaSach,
            MaTheLoai = sach.MaTheLoai,
            MaTacGia = sach.MaTacGia,
            MaNXB = sach.MaNXB,
            TenSach = sach.TenSach,
            NamXuatBan = sach.NamXuatBan,
            SoLuongNhap = sach.SoLuongNhap,
            ViTri = sach.ViTri,
            MoTa = sach.MoTa
        };
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateSachDto dto)
    {
        if (id != dto.MaSach) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(dto);
        }
        try
        {
            await _sachRepo.UpdateAsync(dto);
            TempData["Success"] = "Cập nhật sách thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            await LoadDropdownsAsync();
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var sach = await _sachRepo.GetByIdAsync(id);
        if (sach == null) return NotFound();
        return View(sach);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _sachRepo.SoftDeleteAsync(id);
            TempData["Success"] = "Đã ẩn sách khỏi hệ thống.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult QrImage(string maQR)
    {
        if (string.IsNullOrWhiteSpace(maQR)) return BadRequest();
        var bytes = _qrService.GenerateQrBytes(maQR);
        return File(bytes, "image/png");
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.TheLoais = new SelectList(
            await _db.TheLoais.OrderBy(t => t.TenTheLoai).ToListAsync(),
            "MaTheLoai", "TenTheLoai");
        ViewBag.TacGias = new SelectList(
            await _db.TacGias.OrderBy(t => t.TenTacGia).ToListAsync(),
            "MaTacGia", "TenTacGia");
        ViewBag.NhaXuatBans = new SelectList(
            await _db.NhaXuatBans.OrderBy(n => n.TenNXB).ToListAsync(),
            "MaNXB", "TenNXB");
    }
}
