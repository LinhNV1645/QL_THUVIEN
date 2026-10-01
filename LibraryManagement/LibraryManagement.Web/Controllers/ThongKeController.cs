using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class ThongKeController(IThongKeRepository thongKeRepo, IExportService exportService) : Controller
{
    private const string ExcelMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    // GET /ThongKe
    public async Task<IActionResult> Index(int? thang, int? nam)
    {
        var (t, n) = ChuanHoaThangNam(thang, nam);

        ViewBag.Thang = t;
        ViewBag.Nam = n;
        ViewBag.SachMuonNhieu = (await thongKeRepo.GetSachMuonNhieuAsync(t, n, 10)).ToList();
        ViewBag.DocGiaMuonNhieu = (await thongKeRepo.GetDocGiaMuonNhieuAsync(t, n, 10)).ToList();

        return View();
    }

    // GET /ThongKe/ExportSach
    [HttpGet]
    public async Task<IActionResult> ExportSach()
    {
        try
        {
            var bytes = await exportService.ExportSachToExcelAsync();
            return File(bytes, ExcelMime, $"DanhSachSach_{DateTime.Today:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Xuất Excel thất bại: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET /ThongKe/ExportThongKe?thang=&nam=
    [HttpGet]
    public async Task<IActionResult> ExportThongKe(int? thang, int? nam)
    {
        var (t, n) = ChuanHoaThangNam(thang, nam);
        try
        {
            var bytes = await exportService.ExportThongKeToExcelAsync(t, n);
            return File(bytes, ExcelMime, $"ThongKe_{n}_{t:D2}.xlsx");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Xuất Excel thất bại: " + ex.Message;
            return RedirectToAction(nameof(Index), new { thang = t, nam = n });
        }
    }

    // GET /ThongKe/ExportLichSuMuon?thang=&nam=
    [HttpGet]
    public async Task<IActionResult> ExportLichSuMuon(int? thang, int? nam)
    {
        var (t, n) = ChuanHoaThangNam(thang, nam);
        try
        {
            var bytes = await exportService.ExportLichSuMuonToExcelAsync(null, t, n);
            return File(bytes, ExcelMime, $"LichSuMuon_{n}_{t:D2}.xlsx");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Xuất Excel thất bại: " + ex.Message;
            return RedirectToAction(nameof(Index), new { thang = t, nam = n });
        }
    }

    private static (int Thang, int Nam) ChuanHoaThangNam(int? thang, int? nam)
    {
        var t = thang is >= 1 and <= 12 ? thang.Value : DateTime.Today.Month;
        var n = nam is >= 2000 and <= 2100 ? nam.Value : DateTime.Today.Year;
        return (t, n);
    }
}
