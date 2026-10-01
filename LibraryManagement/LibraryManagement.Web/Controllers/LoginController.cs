using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BCrypt.Net;

namespace LibraryManagement.Web.Controllers;

public class LoginController(AppDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Index(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string tenDangNhap, string matKhau, string? returnUrl = null)
    {
        var tk = await db.TaiKhoans
            .Include(t => t.VaiTro)
            .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap && t.TrangThai == 1);

        if (tk == null || !BCrypt.Net.BCrypt.Verify(matKhau, tk.MatKhau))
        {
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View();
        }

        // Cập nhật lần đăng nhập cuối
        tk.LanDangNhapCuoi = DateTime.Now;
        await db.SaveChangesAsync();

        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, tk.MaTaiKhoan.ToString()),
            new(ClaimTypes.Name,           tk.HoTen),
            new(ClaimTypes.Role,           tk.VaiTro.TenVaiTro),
            new("TenDangNhap",             tk.TenDangNhap)
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index");
    }
}
