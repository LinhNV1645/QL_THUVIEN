using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class CauHinhController : Controller
{
    private readonly ICauHinhRepository _cauHinhRepo;

    public CauHinhController(ICauHinhRepository cauHinhRepo)
    {
        _cauHinhRepo = cauHinhRepo;
    }

    // GET /CauHinh
    public async Task<IActionResult> Index()
    {
        var list = await _cauHinhRepo.GetAllAsync();
        return View(list);
    }

    // POST /CauHinh/Update
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Dictionary<string, string> values)
    {
        try
        {
            foreach (var kv in values)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key))
                    await _cauHinhRepo.UpdateAsync(kv.Key, kv.Value ?? "");
            }
            TempData["Success"] = "Đã lưu cấu hình thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
