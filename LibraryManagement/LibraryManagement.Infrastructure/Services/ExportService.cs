using ClosedXML.Excel;
using LibraryManagement.Application.DTOs.Sach;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Services;

public class ExportService(AppDbContext db, ISachRepository sachRepo) : IExportService
{
    // ── Danh sách sách → Excel ───────────────────────────────────────────────
    public async Task<byte[]> ExportSachToExcelAsync(SachFilterDto? filter)
    {
        var f = filter ?? new SachFilterDto { PageSize = int.MaxValue };
        f.PageSize = int.MaxValue; f.Page = 1;
        var result = await sachRepo.GetAllAsync(f);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Danh sách sách");

        // Header
        string[] headers = ["STT", "Mã sách", "Tên sách", "Tác giả", "Thể loại", "NXB", "Năm XB", "Tồn kho"];
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        int row = 2;
        int stt = 1;
        foreach (var s in result.Items)
        {
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = s.MaSach;
            ws.Cell(row, 3).Value = s.TenSach;
            ws.Cell(row, 4).Value = s.TenTacGia;
            ws.Cell(row, 5).Value = s.TenTheLoai;
            ws.Cell(row, 6).Value = s.TenNXB;
            ws.Cell(row, 7).Value = s.NamXuatBan?.ToString() ?? "";
            ws.Cell(row, 8).Value = s.SoLuongTon;
            row++;
        }

        ws.Columns().AdjustToContents();
        return ToBytes(wb);
    }

    // ── Lịch sử mượn → Excel ────────────────────────────────────────────────
    public async Task<byte[]> ExportLichSuMuonToExcelAsync(int? maDocGia, int thang, int nam)
    {
        var q = db.PhieuMuons
            .Include(p => p.DocGia)
            .Include(p => p.CTPhieuMuons).ThenInclude(ct => ct.Sach)
            .Include(p => p.PhieuTra)
            .Where(p => p.NgayMuon.Month == thang && p.NgayMuon.Year == nam);

        if (maDocGia.HasValue) q = q.Where(p => p.MaDocGia == maDocGia.Value);
        var phieus = await q.ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Lịch sử mượn");

        string[] headers = ["STT", "Mã phiếu", "Độc giả", "Lớp", "Tên sách", "Ngày mượn", "Hạn trả", "Ngày trả", "Tiền phạt"];
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        int row = 2; int stt = 1;
        foreach (var p in phieus)
        {
            foreach (var ct in p.CTPhieuMuons)
            {
                ws.Cell(row, 1).Value = stt++;
                ws.Cell(row, 2).Value = p.MaPhieuMuon;
                ws.Cell(row, 3).Value = p.DocGia.HoTen;
                ws.Cell(row, 4).Value = p.DocGia.Lop ?? "";
                ws.Cell(row, 5).Value = ct.Sach.TenSach;
                ws.Cell(row, 6).Value = p.NgayMuon.ToString("dd/MM/yyyy");
                ws.Cell(row, 7).Value = p.NgayHanTra.ToString("dd/MM/yyyy");
                ws.Cell(row, 8).Value = p.PhieuTra?.NgayTra.ToString("dd/MM/yyyy") ?? "";
                ws.Cell(row, 9).Value = (double)(p.PhieuTra?.TienPhat ?? 0);
                row++;
            }
        }

        ws.Columns().AdjustToContents();
        return ToBytes(wb);
    }

    // ── Thống kê → Excel (2 sheets) ─────────────────────────────────────────
    public async Task<byte[]> ExportThongKeToExcelAsync(int thang, int nam)
    {
        using var wb = new XLWorkbook();
        await BuildSheetSachMuonNhieu(wb, thang, nam);
        await BuildSheetDocGiaMuonNhieu(wb, thang, nam);
        return ToBytes(wb);
    }

    // ── Xuất sách dạng HTML (thay PDF) ──────────────────────────────────────
    public async Task<byte[]> ExportSachToPdfAsync(SachFilterDto? filter)
    {
        var f = filter ?? new SachFilterDto { PageSize = int.MaxValue };
        f.PageSize = int.MaxValue; f.Page = 1;
        var result = await sachRepo.GetAllAsync(f);

        var rows = string.Join("\n", result.Items.Select((s, i) =>
            $"<tr><td>{i + 1}</td><td>{s.MaSach}</td><td>{s.TenSach}</td>" +
            $"<td>{s.TenTacGia}</td><td>{s.TenTheLoai}</td><td>{s.TenNXB}</td>" +
            $"<td>{s.NamXuatBan}</td><td>{s.SoLuongTon}</td></tr>"));

        var now = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        var html = "<html><head><meta charset=\"utf-8\"/>" +
            "<style>body{font-family:Arial;font-size:13px}" +
            "table{border-collapse:collapse;width:100%}" +
            "th,td{border:1px solid #ccc;padding:6px 10px}th{background:#f0f0f0}</style></head><body>" +
            $"<h2>Danh sách sách — {now}</h2>" +
            "<table><thead><tr><th>STT</th><th>Mã</th><th>Tên sách</th><th>Tác giả</th>" +
            "<th>Thể loại</th><th>NXB</th><th>Năm XB</th><th>Tồn kho</th></tr></thead>" +
            $"<tbody>{rows}</tbody></table></body></html>";

        return System.Text.Encoding.UTF8.GetBytes(html);
    }

    // ── private helpers ──────────────────────────────────────────────────────
    private async Task BuildSheetSachMuonNhieu(XLWorkbook wb, int thang, int nam)
    {
        var ws = wb.Worksheets.Add("Sách mượn nhiều");
        string[] headers = ["STT", "Mã sách", "Tên sách", "Tác giả", "Thể loại", "Số lượt mượn"];
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        // Fetch trước rồi group trong memory — tránh lỗi EF Core dịch GroupBy với navigation properties
        var raw = await db.CTPhieuMuons
            .Include(ct => ct.Sach).ThenInclude(s => s.TacGia)
            .Include(ct => ct.Sach).ThenInclude(s => s.TheLoai)
            .Include(ct => ct.PhieuMuon)
            .Where(ct => ct.PhieuMuon.NgayMuon.Month == thang && ct.PhieuMuon.NgayMuon.Year == nam)
            .ToListAsync();

        var data = raw
            .GroupBy(ct => new
            {
                ct.MaSach,
                TenSach    = ct.Sach?.TenSach    ?? "",
                TenTacGia  = ct.Sach?.TacGia?.TenTacGia   ?? "",
                TenTheLoai = ct.Sach?.TheLoai?.TenTheLoai  ?? ""
            })
            .Select(g => new { g.Key.MaSach, g.Key.TenSach, g.Key.TenTacGia, g.Key.TenTheLoai, SoLuot = g.Count() })
            .OrderByDescending(x => x.SoLuot)
            .Take(20)
            .ToList();

        int row = 2; int stt = 1;
        foreach (var x in data)
        {
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = x.MaSach;
            ws.Cell(row, 3).Value = x.TenSach;
            ws.Cell(row, 4).Value = x.TenTacGia;
            ws.Cell(row, 5).Value = x.TenTheLoai;
            ws.Cell(row, 6).Value = x.SoLuot;
            row++;
        }
        ws.Columns().AdjustToContents();
    }

    private async Task BuildSheetDocGiaMuonNhieu(XLWorkbook wb, int thang, int nam)
    {
        var ws = wb.Worksheets.Add("Độc giả mượn nhiều");
        string[] headers = ["STT", "Mã độc giả", "Họ tên", "Lớp", "Số lần mượn", "Tổng sách"];
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        // Fetch trước rồi group trong memory — tránh lỗi EF Core dịch Sum(navigation.Count)
        var rawDoc = await db.PhieuMuons
            .Include(p => p.DocGia)
            .Include(p => p.CTPhieuMuons)
            .Where(p => p.NgayMuon.Month == thang && p.NgayMuon.Year == nam)
            .ToListAsync();

        var data = rawDoc
            .GroupBy(p => new { p.MaDocGia, HoTen = p.DocGia?.HoTen ?? "", Lop = p.DocGia?.Lop })
            .Select(g => new
            {
                g.Key.MaDocGia,
                g.Key.HoTen,
                g.Key.Lop,
                SoLanMuon    = g.Count(),
                TongSachMuon = g.Sum(p => p.CTPhieuMuons.Count)
            })
            .OrderByDescending(x => x.SoLanMuon)
            .Take(20)
            .ToList();

        int row = 2; int stt = 1;
        foreach (var x in data)
        {
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = x.MaDocGia;
            ws.Cell(row, 3).Value = x.HoTen;
            ws.Cell(row, 4).Value = x.Lop ?? "";
            ws.Cell(row, 5).Value = x.SoLanMuon;
            ws.Cell(row, 6).Value = x.TongSachMuon;
            row++;
        }
        ws.Columns().AdjustToContents();
    }

    private static byte[] ToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
