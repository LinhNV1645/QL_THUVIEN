using LibraryManagement.Application.DTOs.Sach;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Web.Controllers;

[AllowAnonymous]
public class TimKiemController : Controller
{
    private readonly ISachRepository _sachRepo;
    private readonly AppDbContext _db;

    public TimKiemController(ISachRepository sachRepo, AppDbContext db)
    {
        _sachRepo = sachRepo;
        _db = db;
    }

    // GET /TimKiem
    public async Task<IActionResult> Index(SachFilterDto? filter)
    {
        await LoadTheLoaiDropdownAsync();

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
        var result = await _sachRepo.GetAllAsync(filter);
        return View(result);
    }

    private async Task LoadTheLoaiDropdownAsync()
    {
        var theLoais = await _db.TheLoais
            .OrderBy(t => t.TenTheLoai)
            .Select(t => new { t.MaTheLoai, t.TenTheLoai })
            .ToListAsync();

        ViewBag.TheLoais = new SelectList(theLoais, "MaTheLoai", "TenTheLoai");
    }
}
