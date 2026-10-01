using System.Data;
using System.Globalization;
using System.Text.Json;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.DTOs.MuonSach;
using LibraryManagement.Application.DTOs.Sach;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class PhieuMuonRepository(AppDbContext db) : IPhieuMuonRepository
{
    // Trạng thái phiếu còn giữ sách: 1=Đang mượn, 3=Quá hạn, 4=Đã gia hạn
    private static readonly byte[] TrangThaiChuaTra = [1, 3, 4];

    // ── gọi SP với OUTPUT param ──────────────────────────────────────────────
    public async Task<int> LapPhieuMuonAsync(LapPhieuMuonDto dto)
    {
        var danhSach = dto.DanhSachSach
            .Where(x => x.MaSach > 0)
            .GroupBy(x => x.MaSach)
            .Select(g => new { MaSach = g.Key, SoLuong = g.Sum(x => x.SoLuong) });
        var json = JsonSerializer.Serialize(danhSach);

        await db.Database.OpenConnectionAsync();
        try
        {
            using var cmd = (SqlCommand)db.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_LapPhieuMuon";
            cmd.Parameters.AddWithValue("@MaDocGia",     dto.MaDocGia);
            cmd.Parameters.AddWithValue("@DanhSachSach", json);
            cmd.Parameters.AddWithValue("@NhanVienLap",  dto.NhanVienLap);
            cmd.Parameters.AddWithValue("@GhiChu",
                string.IsNullOrWhiteSpace(dto.GhiChu) ? DBNull.Value : dto.GhiChu.Trim());

            var outParam = new SqlParameter("@MaPhieuMuon", SqlDbType.Int)
                { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outParam);

            await cmd.ExecuteNonQueryAsync();
            return (int)outParam.Value;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    public async Task<PhieuMuonDetailDto?> GetByIdAsync(int maPhieu)
    {
        var p = await db.PhieuMuons
            .AsNoTracking()
            .Include(x => x.DocGia)
            .Include(x => x.CTPhieuMuons).ThenInclude(ct => ct.Sach)
                .ThenInclude(s => s.TheLoai)
            .Include(x => x.CTPhieuMuons).ThenInclude(ct => ct.Sach)
                .ThenInclude(s => s.TacGia)
            .Include(x => x.CTPhieuMuons).ThenInclude(ct => ct.Sach)
                .ThenInclude(s => s.NhaXuatBan)
            .Include(x => x.GiaHans)
            .FirstOrDefaultAsync(x => x.MaPhieuMuon == maPhieu);

        if (p is null) return null;

        var today = DateOnly.FromDateTime(DateTime.Today);

        return new PhieuMuonDetailDto
        {
            MaPhieuMuon   = p.MaPhieuMuon,
            MaDocGia      = p.MaDocGia,
            TenDocGia     = p.DocGia.HoTen,
            Lop           = p.DocGia.Lop ?? "",
            Email         = p.DocGia.Email,
            NgayMuon      = p.NgayMuon,
            NgayHanTra    = p.NgayHanTra,
            TrangThai     = p.TrangThai,
            TrangThaiText = MapTrangThai(p.TrangThai, p.NgayHanTra, today),
            SoNgayTreHan  = SoNgayTre(p.TrangThai, p.NgayHanTra, today),
            SoSachMuon    = p.CTPhieuMuons.Sum(ct => ct.SoLuongMuon),
            GhiChu        = p.GhiChu,
            DanhSachSach  = p.CTPhieuMuons.Select(ct => new SachDto
            {
                MaSach     = ct.Sach.MaSach,
                TenSach    = ct.Sach.TenSach,
                TenTheLoai = ct.Sach.TheLoai.TenTheLoai,
                TenTacGia  = ct.Sach.TacGia.TenTacGia,
                TenNXB     = ct.Sach.NhaXuatBan.TenNXB,
                ViTri      = ct.Sach.ViTri,
                MaQR       = ct.Sach.MaQR,
                SoLuongTon = ct.Sach.SoLuongTon,
                TrangThai  = ct.Sach.TrangThai
            }).ToList(),
            LichSuGiaHan = p.GiaHans.OrderBy(g => g.LanGiaHan).Select(g => new GiaHanDto
            {
                MaGiaHan   = g.MaGiaHan, NgayGiaHan = g.NgayGiaHan,
                HanTraCu   = g.HanTraCu, HanTraMoi  = g.HanTraMoi,
                LanGiaHan  = g.LanGiaHan
            }).ToList()
        };
    }

    public async Task<PaginatedResult<PhieuMuonDto>> GetAllAsync(PhieuMuonFilterDto filter)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var page     = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var q = db.PhieuMuons.AsNoTracking().AsQueryable();

        if (filter.MaDocGia.HasValue) q = q.Where(p => p.MaDocGia == filter.MaDocGia.Value);
        if (filter.TrangThai.HasValue)
        {
            q = filter.TrangThai.Value switch
            {
                // "Quá hạn" gồm cả phiếu chưa được job đánh dấu
                3 => q.Where(p => p.TrangThai == 3
                               || ((p.TrangThai == 1 || p.TrangThai == 4) && p.NgayHanTra < today)),
                1 => q.Where(p => p.TrangThai == 1 && p.NgayHanTra >= today),
                4 => q.Where(p => p.TrangThai == 4 && p.NgayHanTra >= today),
                var tt => q.Where(p => p.TrangThai == tt)
            };
        }
        if (filter.TuNgay.HasValue)  q = q.Where(p => p.NgayMuon >= filter.TuNgay.Value);
        if (filter.DenNgay.HasValue) q = q.Where(p => p.NgayMuon <= filter.DenNgay.Value);

        var total = await q.CountAsync();
        var rows  = await q
            .OrderByDescending(p => p.NgayMuon).ThenByDescending(p => p.MaPhieuMuon)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.MaPhieuMuon, p.DocGia.HoTen, p.DocGia.Lop,
                p.NgayMuon, p.NgayHanTra, p.TrangThai,
                SoSach = p.CTPhieuMuons.Sum(ct => ct.SoLuongMuon)
            })
            .ToListAsync();

        return new PaginatedResult<PhieuMuonDto>
        {
            Items = rows.Select(p => new PhieuMuonDto
            {
                MaPhieuMuon   = p.MaPhieuMuon,
                TenDocGia     = p.HoTen,
                Lop           = p.Lop ?? "",
                NgayMuon      = p.NgayMuon,
                NgayHanTra    = p.NgayHanTra,
                TrangThai     = p.TrangThai,
                TrangThaiText = MapTrangThai(p.TrangThai, p.NgayHanTra, today),
                SoNgayTreHan  = SoNgayTre(p.TrangThai, p.NgayHanTra, today),
                SoSachMuon    = p.SoSach
            }).ToList(),
            TotalCount = total,
            Page = page, PageSize = pageSize
        };
    }

    public async Task<IEnumerable<PhieuMuonQuaHanDto>> GetQuaHanAsync()
    {
        var today   = DateOnly.FromDateTime(DateTime.Today);
        var mucPhat = await GetMucPhatAsync();

        var rows = await db.PhieuMuons
            .AsNoTracking()
            .Where(p => TrangThaiChuaTra.Contains(p.TrangThai) && p.NgayHanTra < today)
            .Select(p => new { p.MaPhieuMuon, p.DocGia.HoTen, p.DocGia.Lop, p.DocGia.Email, p.NgayHanTra })
            .ToListAsync();

        return rows.Select(p =>
        {
            var soNgayTre = today.DayNumber - p.NgayHanTra.DayNumber;
            return new PhieuMuonQuaHanDto
            {
                MaPhieuMuon     = p.MaPhieuMuon,
                TenDocGia       = p.HoTen,
                Lop             = p.Lop,
                Email           = p.Email,
                NgayHanTra      = p.NgayHanTra,
                SoNgayTre       = soNgayTre,
                TienPhatUocTinh = soNgayTre * mucPhat
            };
        }).ToList();
    }

    public async Task<IEnumerable<PhieuMuonDto>> GetDangMuonByDocGiaAsync(int maDocGia)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var rows = await db.PhieuMuons
            .AsNoTracking()
            .Where(p => p.MaDocGia == maDocGia && TrangThaiChuaTra.Contains(p.TrangThai))
            .Select(p => new
            {
                p.MaPhieuMuon, p.DocGia.HoTen, p.DocGia.Lop,
                p.NgayMuon, p.NgayHanTra, p.TrangThai,
                SoSach = p.CTPhieuMuons.Where(ct => ct.TrangThaiCT == 1).Sum(ct => ct.SoLuongMuon)
            })
            .ToListAsync();

        return rows.Select(p => new PhieuMuonDto
        {
            MaPhieuMuon   = p.MaPhieuMuon,
            TenDocGia     = p.HoTen,
            Lop           = p.Lop ?? "",
            NgayMuon      = p.NgayMuon,
            NgayHanTra    = p.NgayHanTra,
            TrangThai     = p.TrangThai,
            TrangThaiText = MapTrangThai(p.TrangThai, p.NgayHanTra, today),
            SoNgayTreHan  = SoNgayTre(p.TrangThai, p.NgayHanTra, today),
            SoSachMuon    = p.SoSach
        }).ToList();
    }

    private async Task<decimal> GetMucPhatAsync()
    {
        var giaTri = await db.CauHinhHeThongs
            .Where(c => c.TenCauHinh == "MucPhatNgayTreHan")
            .Select(c => c.GiaTri)
            .FirstOrDefaultAsync();
        return decimal.TryParse(giaTri, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    private static int SoNgayTre(byte tt, DateOnly hanTra, DateOnly today) =>
        tt != 2 && hanTra < today ? today.DayNumber - hanTra.DayNumber : 0;

    private static string MapTrangThai(byte tt, DateOnly hanTra, DateOnly today) => tt switch
    {
        2 => "Đã trả",
        3 => "Quá hạn",
        1 or 4 when hanTra < today => "Quá hạn",
        1 => "Đang mượn",
        4 => "Đã gia hạn",
        _ => "Không xác định"
    };
}
