using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Web.Models;

namespace LibraryManagement.Web.Controllers;

[Authorize]
public class HomeController(IThongKeRepository thongKeRepo) : Controller
{
    private static readonly int[] SoNgayHopLe = [3, 7, 14];

    // GET /?ky=thang|nam|tatca&soNgay=3|7|14
    public async Task<IActionResult> Index(string? ky, int soNgay = 7)
    {
        var homNay = DateOnly.FromDateTime(DateTime.Today);
        ky = ky is "thang" or "nam" or "tatca" ? ky : "nam";
        soNgay = SoNgayHopLe.Contains(soNgay) ? soNgay : 7;

        (DateOnly? TuNgay, string Ten) khoang = ky switch
        {
            "thang" => (new DateOnly(homNay.Year, homNay.Month, 1), $"Tháng {homNay.Month}/{homNay.Year}"),
            "nam"   => (new DateOnly(homNay.Year, 1, 1), $"Năm {homNay.Year}"),
            _       => (null, "Toàn thời gian")
        };
        var tuNgay = khoang.TuNgay;
        var tenKy = khoang.Ten;
        DateOnly? denNgay = tuNgay is null ? null : homNay;

        var laNhanVien = User.IsInRole("Admin") || User.IsInRole("NhanVien");
        var tongQuan = await thongKeRepo.GetDashboardAsync();

        var vm = new DashboardViewModel
        {
            TongQuan        = tongQuan,
            SoNgaySapDenHan = soNgay,
            KyThongKe       = ky,
            TenKyThongKe    = tenKy,
            LaNhanVien      = laNhanVien,
            TopSach         = await thongKeRepo.GetTopSachAsync(tuNgay, denNgay, 10)
        };

        if (laNhanVien)
        {
            if (tongQuan.QuaHan > 0)
                vm.QuaHan = (await thongKeRepo.GetTopQuaHanAsync(5)).ToList();

            var (sapDenHan, tongSo) = await thongKeRepo.GetSapDenHanAsync(soNgay, 10);
            vm.SapDenHan       = sapDenHan;
            vm.TongSoSapDenHan = tongSo;
            vm.TopDocGia       = await thongKeRepo.GetTopDocGiaAsync(tuNgay, denNgay, 10);
        }

        return View(vm);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
