using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryManagement.Web.Controllers;

public class LoginController(ITaiKhoanRepository taiKhoanRepo) : Controller
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
        var tk = await taiKhoanRepo.XacThucAsync(tenDangNhap, matKhau);
        if (tk == null)
        {
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, tk.MaTaiKhoan.ToString()),
            new(ClaimTypes.Name,           tk.HoTen),
            new(ClaimTypes.Role,           tk.TenVaiTro),
            new("TenDangNhap",             tk.TenDangNhap)
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index");
    }
}
