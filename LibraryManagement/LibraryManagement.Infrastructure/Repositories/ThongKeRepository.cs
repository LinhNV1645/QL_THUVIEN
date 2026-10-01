using System.Data;
using System.Data.Common;
using System.Globalization;
using LibraryManagement.Application.DTOs.MuonSach;
using LibraryManagement.Application.DTOs.ThongKe;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class ThongKeRepository(AppDbContext db) : IThongKeRepository
{
    private static readonly byte[] TrangThaiChuaTra = [1, 3, 4];

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var now   = DateTime.Today;
        var today = DateOnly.FromDateTime(now);
        var dauThang = new DateOnly(now.Year, now.Month, 1);

        return new DashboardDto
        {
            TongDauSach     = await db.Sachs.CountAsync(s => s.TrangThai == 1),
            TongSachTon     = await db.Sachs.Where(s => s.TrangThai == 1).SumAsync(s => s.SoLuongTon),
            TongDocGia      = await db.DocGias.CountAsync(d => d.TrangThai == 1),
            DangMuon        = await db.PhieuMuons.CountAsync(pm => TrangThaiChuaTra.Contains(pm.TrangThai)),
            QuaHan          = await db.PhieuMuons.CountAsync(pm => TrangThaiChuaTra.Contains(pm.TrangThai) && pm.NgayHanTra < today),
            TienPhatChuaThu = await db.PhieuTras.Where(pt => !pt.DaThuPhat).SumAsync(pt => (decimal?)pt.TienPhat) ?? 0,
            MuonTrongThang  = await db.PhieuMuons.CountAsync(pm => pm.NgayMuon >= dauThang && pm.NgayMuon <= today)
        };
    }

    public async Task<IEnumerable<PhieuMuonQuaHanDto>> GetTopQuaHanAsync(int top)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var giaTri = await db.CauHinhHeThongs
            .Where(c => c.TenCauHinh == "MucPhatNgayTreHan")
            .Select(c => c.GiaTri)
            .FirstOrDefaultAsync();
        var mucPhat = decimal.TryParse(giaTri, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0;

        var rows = await db.PhieuMuons
            .AsNoTracking()
            .Where(pm => TrangThaiChuaTra.Contains(pm.TrangThai) && pm.NgayHanTra < today)
            .OrderBy(pm => pm.NgayHanTra)
            .Take(top)
            .Select(pm => new { pm.MaPhieuMuon, pm.DocGia.HoTen, pm.DocGia.Lop, pm.DocGia.Email, pm.NgayHanTra })
            .ToListAsync();

        return rows.Select(r =>
        {
            var soNgayTre = today.DayNumber - r.NgayHanTra.DayNumber;
            return new PhieuMuonQuaHanDto
            {
                MaPhieuMuon     = r.MaPhieuMuon,
                TenDocGia       = r.HoTen,
                Lop             = r.Lop,
                Email           = r.Email,
                NgayHanTra      = r.NgayHanTra,
                SoNgayTre       = soNgayTre,
                TienPhatUocTinh = soNgayTre * mucPhat
            };
        }).ToList();
    }

    public Task<IEnumerable<SachMuonNhieuDto>> GetSachMuonNhieuAsync(int thang, int nam, int topN) =>
        GoiThongKeAsync("sp_ThongKeSachMuonNhieu", thang, nam, topN, r => new SachMuonNhieuDto
        {
            MaSach     = r.GetInt32(r.GetOrdinal("MaSach")),
            TenSach    = r.GetString(r.GetOrdinal("TenSach")),
            TenTacGia  = r["TenTacGia"] as string ?? "",
            TenTheLoai = r["TenTheLoai"] as string ?? "",
            SoLuotMuon = r.GetInt32(r.GetOrdinal("SoLuotMuon"))
        });

    public Task<IEnumerable<DocGiaMuonNhieuDto>> GetDocGiaMuonNhieuAsync(int thang, int nam, int topN) =>
        GoiThongKeAsync("sp_ThongKeDocGiaMuonNhieu", thang, nam, topN, r => new DocGiaMuonNhieuDto
        {
            MaDocGia     = r.GetInt32(r.GetOrdinal("MaDocGia")),
            HoTen        = r.GetString(r.GetOrdinal("HoTen")),
            Lop          = r["Lop"] as string,
            SoLanMuon    = r.GetInt32(r.GetOrdinal("SoLanMuon")),
            TongSachMuon = r.GetInt32(r.GetOrdinal("TongSachMuon"))
        });

    public async Task<(IReadOnlyList<PhieuMuonSapDenHanDto> Items, int TongSo)> GetSapDenHanAsync(int soNgay, int topN)
    {
        var tongSo = 0;
        var items = await GoiSpAsync("sp_Dashboard_SapDenHan",
            [new SqlParameter("@SoNgay", soNgay), new SqlParameter("@TopN", topN)],
            r =>
            {
                tongSo = Convert.ToInt32(r["TongSo"]);
                return new PhieuMuonSapDenHanDto
                {
                    MaPhieuMuon  = Convert.ToInt32(r["MaPhieuMuon"]),
                    NgayMuon     = DocNgay(r["NgayMuon"]) ?? default,
                    NgayHanTra   = DocNgay(r["NgayHanTra"]) ?? default,
                    DaGiaHan     = Convert.ToByte(r["TrangThai"]) == 4,
                    MaDocGia     = Convert.ToInt32(r["MaDocGia"]),
                    HoTen        = r["HoTen"] as string ?? "",
                    Lop          = r["Lop"] as string,
                    SoDienThoai  = r["SoDienThoai"] as string,
                    SoNgayConLai = Convert.ToInt32(r["SoNgayConLai"]),
                    SoCuon       = Convert.ToInt32(r["SoCuon"]),
                    DanhSachSach = r["DanhSachSach"] as string ?? ""
                };
            });
        return (items, tongSo);
    }

    public Task<IReadOnlyList<SachMuonNhieuDto>> GetTopSachAsync(DateOnly? tuNgay, DateOnly? denNgay, int topN) =>
        GoiSpAsync("sp_Dashboard_TopSach", ThamSoKhoangNgay(tuNgay, denNgay, topN), r => new SachMuonNhieuDto
        {
            MaSach       = Convert.ToInt32(r["MaSach"]),
            TenSach      = r["TenSach"] as string ?? "",
            TenTacGia    = r["TenTacGia"] as string ?? "",
            TenTheLoai   = r["TenTheLoai"] as string ?? "",
            SoLuotMuon   = Convert.ToInt32(r["SoLuotMuon"]),
            TongCuonMuon = Convert.ToInt32(r["TongCuonMuon"]),
            SoLuongNhap  = Convert.ToInt32(r["SoLuongNhap"]),
            SoLuongTon   = Convert.ToInt32(r["SoLuongTon"])
        });

    public Task<IReadOnlyList<DocGiaMuonNhieuDto>> GetTopDocGiaAsync(DateOnly? tuNgay, DateOnly? denNgay, int topN) =>
        GoiSpAsync("sp_Dashboard_TopDocGia", ThamSoKhoangNgay(tuNgay, denNgay, topN), r => new DocGiaMuonNhieuDto
        {
            MaDocGia       = Convert.ToInt32(r["MaDocGia"]),
            HoTen          = r["HoTen"] as string ?? "",
            Lop            = r["Lop"] as string,
            SoLanMuon      = Convert.ToInt32(r["SoLanMuon"]),
            TongSachMuon   = Convert.ToInt32(r["TongSachMuon"]),
            SoPhieuChuaTra = Convert.ToInt32(r["SoPhieuChuaTra"]),
            LanMuonGanNhat = DocNgay(r["LanMuonGanNhat"])
        });

    private static SqlParameter[] ThamSoKhoangNgay(DateOnly? tuNgay, DateOnly? denNgay, int topN) =>
    [
        new SqlParameter("@TopN", topN),
        new SqlParameter("@TuNgay",  SqlDbType.Date) { Value = (object?)tuNgay?.ToDateTime(TimeOnly.MinValue)  ?? DBNull.Value },
        new SqlParameter("@DenNgay", SqlDbType.Date) { Value = (object?)denNgay?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value }
    ];

    private static DateOnly? DocNgay(object giaTri) => giaTri switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => null
    };

    private async Task<IEnumerable<T>> GoiThongKeAsync<T>(
        string spName, int thang, int nam, int topN, Func<DbDataReader, T> map) =>
        await GoiSpAsync(spName,
            [new SqlParameter("@Thang", thang), new SqlParameter("@Nam", nam), new SqlParameter("@TopN", topN)],
            map);

    private async Task<IReadOnlyList<T>> GoiSpAsync<T>(
        string spName, SqlParameter[] thamSo, Func<DbDataReader, T> map)
    {
        var list = new List<T>();
        await db.Database.OpenConnectionAsync();
        try
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(thamSo);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(map(reader));
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
        return list;
    }
}
