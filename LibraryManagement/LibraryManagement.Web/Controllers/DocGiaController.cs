using LibraryManagement.Application.DTOs.DocGia;
using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class DocGiaController : Controller
{
    private readonly IDocGiaRepository _docGiaRepo;

    public DocGiaController(IDocGiaRepository docGiaRepo)
    {
        _docGiaRepo = docGiaRepo;
    }

    public async Task<IActionResult> Index(string? keyword, string? lop, int page = 1)
    {
        var result = await _docGiaRepo.GetAllAsync(keyword, lop, page, 20);
        ViewBag.Keyword = keyword;
        ViewBag.Lop = lop;
        return View(result);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateDocGiaDto());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDocGiaDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _docGiaRepo.CreateAsync(dto);
            TempData["Success"] = "Thêm độc giả thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dg = await _docGiaRepo.GetByIdAsync(id);
        if (dg == null) return NotFound();
        var dto = new UpdateDocGiaDto
        {
            MaDocGia = dg.MaDocGia,
            HoTen = dg.HoTen,
            Lop = dg.Lop,
            NgaySinh = dg.NgaySinh,
            GioiTinh = dg.GioiTinh,
            DiaChi = dg.DiaChi,
            Email = dg.Email,
            SoDienThoai = dg.SoDienThoai
        };
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateDocGiaDto dto)
    {
        if (id != dto.MaDocGia) return BadRequest();
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _docGiaRepo.UpdateAsync(dto);
            TempData["Success"] = "Cập nhật độc giả thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var dg = await _docGiaRepo.GetByIdAsync(id);
        if (dg == null) return NotFound();
        var lichSu = await _docGiaRepo.GetLichSuAsync(id);
        ViewBag.LichSu = lichSu;
        return View(dg);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTrangThai(int id)
    {
        try
        {
            var dg = await _docGiaRepo.GetByIdAsync(id);
            if (dg == null) return NotFound();
            byte newStatus = dg.TrangThai == 1 ? (byte)0 : (byte)1;
            await _docGiaRepo.SetTrangThaiAsync(id, newStatus);
            TempData["Success"] = newStatus == 1 ? "Đã kích hoạt độc giả." : "Đã khóa độc giả.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
