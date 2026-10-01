using LibraryManagement.Application.DTOs.Sach;
using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagement.Web.Controllers;

[AllowAnonymous]
public class TimKiemController(ISachRepository sachRepo, IDanhMucRepository danhMucRepo) : Controller
{
    // GET /TimKiem
    public async Task<IActionResult> Index(SachFilterDto? filter)
    {
        ViewBag.TheLoais = new SelectList(await danhMucRepo.GetTheLoaisAsync(), "Id", "Ten");

        // Chưa có từ khoá gì thì không tìm, trả về null model
        bool hasQuery = !string.IsNullOrWhiteSpace(filter?.TuKhoa)
                        || filter?.MaTheLoai != null
                        || filter?.MaTacGia != null
                        || filter?.MaNXB != null
                        || (filter?.ChiConTon ?? false);

        ViewBag.Filter = filter ?? new SachFilterDto();

        if (!hasQuery)
            return View((object?)null);

        filter!.PageSize = 20;
        var result = await sachRepo.GetAllAsync(filter);
        return View(result);
    }
}
