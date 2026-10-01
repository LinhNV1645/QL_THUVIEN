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
        // Các stored procedure CAST giá trị cấu hình sang INT nên chỉ chấp nhận số nguyên không âm
        var khongHopLe = values
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Key)
                      && !(int.TryParse(kv.Value?.Trim(), out var n) && n >= 0))
            .Select(kv => kv.Key)
            .ToList();
        if (khongHopLe.Count > 0)
        {
            TempData["Error"] = "Giá trị phải là số nguyên không âm: " + string.Join(", ", khongHopLe);
            return RedirectToAction(nameof(Index));
        }

        try
        {
            foreach (var kv in values)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key))
                    await _cauHinhRepo.UpdateAsync(kv.Key, kv.Value.Trim());
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
