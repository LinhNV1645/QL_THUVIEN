using LibraryManagement.Application.DTOs.ThongKe;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LibraryManagement.Web.Controllers;

[Authorize(Policy = "Staff")]
public class ThongKeController : Controller
{
    private readonly AppDbContext _db;
    private readonly IExportService _exportService;

    public ThongKeController(AppDbContext db, IExportService exportService)
    {
        _db = db;
        _exportService = exportService;
    }

    // GET /ThongKe
    public async Task<IActionResult> Index(int? thang, int? nam)
    {
        int t = thang ?? DateTime.Today.Month;
        int n = nam ?? DateTime.Today.Year;

        ViewBag.Thang = t;
        ViewBag.Nam = n;

        var sachList = await GetSachMuonNhieuAsync(t, n, 10);
        var docGiaList = await GetDocGiaMuonNhieuAsync(t, n, 10);

        ViewBag.SachMuonNhieu = sachList;
        ViewBag.DocGiaMuonNhieu = docGiaList;

        return View();
    }

    // GET /ThongKe/ExportSach
    [HttpGet]
    public async Task<IActionResult> ExportSach()
    {
        try
        {
            var bytes = await _exportService.ExportSachToExcelAsync();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DanhSachSach_{DateTime.Today:yyyyMMdd}.xlsx");
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
        int t = thang ?? DateTime.Today.Month;
        int n = nam ?? DateTime.Today.Year;
        try
        {
            var bytes = await _exportService.ExportThongKeToExcelAsync(t, n);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ThongKe_{n}_{t:D2}.xlsx");
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
        int t = thang ?? DateTime.Today.Month;
        int n = nam ?? DateTime.Today.Year;
        try
        {
            var bytes = await _exportService.ExportLichSuMuonToExcelAsync(null, t, n);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"LichSuMuon_{n}_{t:D2}.xlsx");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Xuất Excel thất bại: " + ex.Message;
            return RedirectToAction(nameof(Index), new { thang = t, nam = n });
        }
    }

    // --- Private helpers gọi stored procedures qua ADO.NET ---

    private async Task<List<SachMuonNhieuDto>> GetSachMuonNhieuAsync(int thang, int nam, int topN)
    {
        var list = new List<SachMuonNhieuDto>();
        await _db.Database.OpenConnectionAsync();
        try
        {
            using var cmd = _db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "sp_ThongKeSachMuonNhieu";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Thang", thang));
            cmd.Parameters.Add(new SqlParameter("@Nam", nam));
            cmd.Parameters.Add(new SqlParameter("@TopN", topN));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new SachMuonNhieuDto
                {
                    MaSach = reader.GetInt32(reader.GetOrdinal("MaSach")),
                    TenSach = reader.GetString(reader.GetOrdinal("TenSach")),
                    TenTacGia = reader.IsDBNull(reader.GetOrdinal("TenTacGia")) ? "" : reader.GetString(reader.GetOrdinal("TenTacGia")),
                    TenTheLoai = reader.IsDBNull(reader.GetOrdinal("TenTheLoai")) ? "" : reader.GetString(reader.GetOrdinal("TenTheLoai")),
                    SoLuotMuon = reader.GetInt32(reader.GetOrdinal("SoLuotMuon"))
                });
            }
        }
        finally
        {
            await _db.Database.CloseConnectionAsync();
        }
        return list;
    }

    private async Task<List<DocGiaMuonNhieuDto>> GetDocGiaMuonNhieuAsync(int thang, int nam, int topN)
    {
        var list = new List<DocGiaMuonNhieuDto>();
        await _db.Database.OpenConnectionAsync();
        try
        {
            using var cmd = _db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "sp_ThongKeDocGiaMuonNhieu";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Thang", thang));
            cmd.Parameters.Add(new SqlParameter("@Nam", nam));
            cmd.Parameters.Add(new SqlParameter("@TopN", topN));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new DocGiaMuonNhieuDto
                {
                    MaDocGia = reader.GetInt32(reader.GetOrdinal("MaDocGia")),
                    HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
                    Lop = reader.IsDBNull(reader.GetOrdinal("Lop")) ? null : reader.GetString(reader.GetOrdinal("Lop")),
                    SoLanMuon = reader.GetInt32(reader.GetOrdinal("SoLanMuon")),
                    TongSachMuon = reader.GetInt32(reader.GetOrdinal("TongSachMuon"))
                });
            }
        }
        finally
        {
            await _db.Database.CloseConnectionAsync();
        }
        return list;
    }
}
