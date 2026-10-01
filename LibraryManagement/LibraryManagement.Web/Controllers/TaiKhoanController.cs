using LibraryManagement.Application.DTOs.TaiKhoan;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class TaiKhoanController : Controller
{
    private readonly ITaiKhoanRepository _taiKhoanRepo;
    private readonly AppDbContext _db;

    public TaiKhoanController(ITaiKhoanRepository taiKhoanRepo, AppDbContext db)
    {
        _taiKhoanRepo = taiKhoanRepo;
        _db = db;
    }

    // GET /TaiKhoan
    public async Task<IActionResult> Index()
    {
        var list = await _taiKhoanRepo.GetAllAsync();
        return View(list);
    }

    // GET /TaiKhoan/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadVaiTroViewBagAsync();
        return View();
    }

    // POST /TaiKhoan/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTaiKhoanDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TenDangNhap))
            ModelState.AddModelError("TenDangNhap", "Tên đăng nhập không được trống.");
        if (dto.MatKhau.Length < 6)
            ModelState.AddModelError("MatKhau", "Mật khẩu phải có ít nhất 6 ký tự.");

        if (!ModelState.IsValid)
        {
            await LoadVaiTroViewBagAsync();
            return View(dto);
        }

        try
        {
            var maTK = await _taiKhoanRepo.CreateAsync(dto);
            TempData["Success"] = $"Tạo tài khoản #{maTK} thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            await LoadVaiTroViewBagAsync();
            return View(dto);
        }
    }

    // GET /TaiKhoan/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var tk = await _taiKhoanRepo.GetByUsernameAsync("") ?? null;
        var all = await _taiKhoanRepo.GetAllAsync();
        var found = all.FirstOrDefault(t => t.MaTaiKhoan == id);
        if (found == null) return NotFound();

        var dto = new UpdateTaiKhoanDto
        {
            MaTaiKhoan = found.MaTaiKhoan,
            HoTen = found.HoTen,
            Email = found.Email,
            MaVaiTro = found.MaVaiTro
        };

        await LoadVaiTroViewBagAsync();
        ViewBag.TenDangNhap = found.TenDangNhap;
        return View(dto);
    }

    // POST /TaiKhoan/Edit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTaiKhoanDto dto)
    {
        dto.MaTaiKhoan = id;
        if (string.IsNullOrWhiteSpace(dto.HoTen))
            ModelState.AddModelError("HoTen", "Họ tên không được trống.");

        if (!ModelState.IsValid)
        {
            await LoadVaiTroViewBagAsync();
            return View(dto);
        }

        try
        {
            await _taiKhoanRepo.UpdateAsync(dto);
            TempData["Success"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            await LoadVaiTroViewBagAsync();
            return View(dto);
        }
    }

    // POST /TaiKhoan/DoiMatKhau/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiMatKhau(int id, string matKhauMoi)
    {
        if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Length < 6)
        {
            TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _taiKhoanRepo.ChangePasswordAsync(id, matKhauMoi);
            TempData["Success"] = "Đổi mật khẩu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // POST /TaiKhoan/ToggleTrangThai/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTrangThai(int id, byte trangThaiHienTai)
    {
        try
        {
            byte trangThaiMoi = trangThaiHienTai == 1 ? (byte)0 : (byte)1;
            await _taiKhoanRepo.SetTrangThaiAsync(id, trangThaiMoi);
            TempData["Success"] = trangThaiMoi == 1 ? "Đã kích hoạt tài khoản." : "Đã vô hiệu hóa tài khoản.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadVaiTroViewBagAsync()
    {
        var vaiTros = await _db.VaiTros.ToListAsync();
        ViewBag.VaiTros = new SelectList(vaiTros, "MaVaiTro", "TenVaiTro");
    }
}
