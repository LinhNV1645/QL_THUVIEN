using LibraryManagement.Application.DTOs.Sach;
using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class SachController : Controller
{
    private readonly ISachRepository _sachRepo;
    private readonly IQrService _qrService;
    private readonly IDanhMucRepository _danhMucRepo;

    public SachController(ISachRepository sachRepo, IQrService qrService, IDanhMucRepository danhMucRepo)
    {
        _sachRepo = sachRepo;
        _qrService = qrService;
        _danhMucRepo = danhMucRepo;
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
            SoTrang = sach.SoTrang,
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
        // Encode URL vào QR để camera điện thoại có thể mở thẳng trang sách
        var url = $"{Request.Scheme}://{Request.Host}/Sach/Scan/{maQR}";
        var bytes = _qrService.GenerateQrBytes(url);
        return File(bytes, "image/png");
    }

    // Trang công khai — không cần đăng nhập, hiện khi quét QR bằng điện thoại
    [AllowAnonymous]
    [HttpGet("/Sach/Scan/{maQR}")]
    public async Task<IActionResult> Scan(string maQR)
    {
        var sach = await _sachRepo.GetByQrAsync(maQR);
        return View(sach); // null → view hiện "Không tìm thấy sách"
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.TheLoais    = new SelectList(await _danhMucRepo.GetTheLoaisAsync(), "Id", "Ten");
        ViewBag.TacGias     = new SelectList(await _danhMucRepo.GetTacGiasAsync(), "Id", "Ten");
        ViewBag.NhaXuatBans = new SelectList(await _danhMucRepo.GetNhaXuatBansAsync(), "Id", "Ten");
    }
}
