using System.Data;
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
    // ── gọi SP với OUTPUT param ──────────────────────────────────────────────
    public async Task<int> LapPhieuMuonAsync(LapPhieuMuonDto dto)
    {
        var json = JsonSerializer.Serialize(dto.DanhSachSach.Select(x => new { x.MaSach, x.SoLuong }));

        await db.Database.OpenConnectionAsync();
        using var cmd = (SqlCommand)db.Database.GetDbConnection().CreateCommand();
        cmd.CommandType    = CommandType.StoredProcedure;
        cmd.CommandText    = "sp_LapPhieuMuon";
        cmd.Parameters.AddWithValue("@MaDocGia",    dto.MaDocGia);
        cmd.Parameters.AddWithValue("@DanhSachSach", json);
        cmd.Parameters.AddWithValue("@NhanVienLap", dto.NhanVienLap);
        if (dto.GhiChu != null) cmd.Parameters.AddWithValue("@GhiChu", dto.GhiChu);

        var outParam = new SqlParameter("@MaPhieuMuon", SqlDbType.Int)
            { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(outParam);

        await cmd.ExecuteNonQueryAsync();
        db.Database.CloseConnection();
        return (int)outParam.Value;
    }

    public async Task<PhieuMuonDetailDto?> GetByIdAsync(int maPhieu)
    {
        var p = await db.PhieuMuons
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

        var soNgayTre = Math.Max(0,
            DateOnly.FromDateTime(DateTime.Today).DayNumber - p.NgayHanTra.DayNumber);

        return new PhieuMuonDetailDto
        {
            MaPhieuMuon  = p.MaPhieuMuon,
            MaDocGia     = p.MaDocGia,
            TenDocGia    = p.DocGia.HoTen,
            Lop          = p.DocGia.Lop ?? "",
            Email        = p.DocGia.Email,
            NgayMuon     = p.NgayMuon,
            NgayHanTra   = p.NgayHanTra,
            TrangThai    = p.TrangThai,
            TrangThaiText = MapTrangThai(p.TrangThai),
            SoNgayTreHan = soNgayTre,
            SoSachMuon   = p.CTPhieuMuons.Count,
            GhiChu       = p.GhiChu,
            DanhSachSach  = p.CTPhieuMuons.Select(ct => new SachDto
            {
                MaSach     = ct.Sach.MaSach, TenSach   = ct.Sach.TenSach,
                TenTheLoai = ct.Sach.TheLoai.TenTheLoai,
                TenTacGia  = ct.Sach.TacGia.TenTacGia,
                TenNXB     = ct.Sach.NhaXuatBan.TenNXB,
                SoLuongTon = ct.Sach.SoLuongTon, TrangThai = ct.Sach.TrangThai
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
        var q = db.PhieuMuons
            .Include(p => p.DocGia)
            .Include(p => p.CTPhieuMuons)
            .AsQueryable();

        if (filter.MaDocGia.HasValue)  q = q.Where(p => p.MaDocGia   == filter.MaDocGia.Value);
        if (filter.TrangThai.HasValue) q = q.Where(p => p.TrangThai  == filter.TrangThai.Value);
        if (filter.TuNgay.HasValue)    q = q.Where(p => p.NgayMuon   >= filter.TuNgay.Value);
        if (filter.DenNgay.HasValue)   q = q.Where(p => p.NgayMuon   <= filter.DenNgay.Value);

        var today    = DateOnly.FromDateTime(DateTime.Today);
        var total    = await q.CountAsync();
        var items    = await q
            .OrderByDescending(p => p.NgayMuon)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new PhieuMuonDto
            {
                MaPhieuMuon  = p.MaPhieuMuon,
                TenDocGia    = p.DocGia.HoTen,
                Lop          = p.DocGia.Lop ?? "",
                NgayMuon     = p.NgayMuon,
                NgayHanTra   = p.NgayHanTra,
                TrangThai    = p.TrangThai,
                TrangThaiText = MapTrangThai(p.TrangThai),
                SoNgayTreHan = p.NgayHanTra < today
                    ? today.DayNumber - p.NgayHanTra.DayNumber : 0,
                SoSachMuon   = p.CTPhieuMuons.Count
            })
            .ToListAsync();

        return new PaginatedResult<PhieuMuonDto>
        {
            Items = items, TotalCount = total,
            Page = filter.Page, PageSize = filter.PageSize
        };
    }

    public async Task<IEnumerable<PhieuMuonQuaHanDto>> GetQuaHanAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await db.PhieuMuons
            .Include(p => p.DocGia)
            .Where(p => p.TrangThai == 1 && p.NgayHanTra < today)
            .Select(p => new PhieuMuonQuaHanDto
            {
                MaPhieuMuon     = p.MaPhieuMuon,
                TenDocGia       = p.DocGia.HoTen,
                Lop             = p.DocGia.Lop,
                Email           = p.DocGia.Email,
                NgayHanTra      = p.NgayHanTra,
                SoNgayTre       = today.DayNumber - p.NgayHanTra.DayNumber,
                TienPhatUocTinh = (today.DayNumber - p.NgayHanTra.DayNumber) * 2000m
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<PhieuMuonDto>> GetDangMuonByDocGiaAsync(int maDocGia)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await db.PhieuMuons
            .Include(p => p.DocGia)
            .Include(p => p.CTPhieuMuons)
            .Where(p => p.MaDocGia == maDocGia && p.TrangThai == 1)
            .Select(p => new PhieuMuonDto
            {
                MaPhieuMuon  = p.MaPhieuMuon,
                TenDocGia    = p.DocGia.HoTen,
                Lop          = p.DocGia.Lop ?? "",
                NgayMuon     = p.NgayMuon,
                NgayHanTra   = p.NgayHanTra,
                TrangThai    = p.TrangThai,
                TrangThaiText = MapTrangThai(p.TrangThai),
                SoNgayTreHan = p.NgayHanTra < today
                    ? today.DayNumber - p.NgayHanTra.DayNumber : 0,
                SoSachMuon   = p.CTPhieuMuons.Count
            })
            .ToListAsync();
    }

    private static string MapTrangThai(byte tt) => tt switch
    {
        1 => "Đang mượn", 2 => "Đã trả", 3 => "Quá hạn", 4 => "Đã gia hạn", _ => "Không xác định"
    };
}
