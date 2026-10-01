using LibraryManagement.Application.DTOs.TaiKhoan;
using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class TaiKhoanController(ITaiKhoanRepository taiKhoanRepo) : Controller
{
    // GET /TaiKhoan
    public async Task<IActionResult> Index()
    {
        var list = await taiKhoanRepo.GetAllAsync();
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
        if (string.IsNullOrWhiteSpace(dto.HoTen))
            ModelState.AddModelError("HoTen", "Họ tên không được trống.");
        if ((dto.MatKhau ?? "").Length < 6)
            ModelState.AddModelError("MatKhau", "Mật khẩu phải có ít nhất 6 ký tự.");

        if (!ModelState.IsValid)
        {
            await LoadVaiTroViewBagAsync();
            return View(dto);
        }

        try
        {
            var maTK = await taiKhoanRepo.CreateAsync(dto);
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
        var found = await taiKhoanRepo.GetByIdAsync(id);
        if (found == null) return NotFound();

        var dto = new UpdateTaiKhoanDto
        {
            MaTaiKhoan = found.MaTaiKhoan,
            HoTen = found.HoTen,
            Email = found.Email,
            SoDienThoai = found.SoDienThoai,
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

        // Không cho admin tự hạ quyền của chính mình
        var current = await taiKhoanRepo.GetByIdAsync(id);
        if (current == null) return NotFound();
        if (id == MaTaiKhoanHienTai() && dto.MaVaiTro != current.MaVaiTro)
            ModelState.AddModelError("MaVaiTro", "Bạn không thể tự thay đổi vai trò của chính mình.");

        if (!ModelState.IsValid)
        {
            await LoadVaiTroViewBagAsync();
            ViewBag.TenDangNhap = current.TenDangNhap;
            return View(dto);
        }

        try
        {
            await taiKhoanRepo.UpdateAsync(dto);
            TempData["Success"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            await LoadVaiTroViewBagAsync();
            ViewBag.TenDangNhap = current.TenDangNhap;
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
            await taiKhoanRepo.ChangePasswordAsync(id, matKhauMoi);
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
    public async Task<IActionResult> ToggleTrangThai(int id)
    {
        if (id == MaTaiKhoanHienTai())
        {
            TempData["Error"] = "Bạn không thể vô hiệu hóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var tk = await taiKhoanRepo.GetByIdAsync(id);
            if (tk == null) return NotFound();
            byte trangThaiMoi = tk.TrangThai == 1 ? (byte)0 : (byte)1;
            await taiKhoanRepo.SetTrangThaiAsync(id, trangThaiMoi);
            TempData["Success"] = trangThaiMoi == 1 ? "Đã kích hoạt tài khoản." : "Đã vô hiệu hóa tài khoản.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private int MaTaiKhoanHienTai() =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    private async Task LoadVaiTroViewBagAsync()
    {
        ViewBag.VaiTros = new SelectList(await taiKhoanRepo.GetVaiTrosAsync(), "Id", "Ten");
    }
}
