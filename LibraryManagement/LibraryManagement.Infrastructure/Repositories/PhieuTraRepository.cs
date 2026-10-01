using System.Data;
using LibraryManagement.Application.DTOs.TraSach;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class PhieuTraRepository(AppDbContext db) : IPhieuTraRepository
{
    // ── gọi sp_TraSach với OUTPUT param ─────────────────────────────────────
    public async Task<TraSachResultDto> TraSachAsync(TraSachDto dto)
    {
        await db.Database.OpenConnectionAsync();
        using var cmd = (SqlCommand)db.Database.GetDbConnection().CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "sp_TraSach";
        cmd.Parameters.AddWithValue("@MaPhieuMuon",   dto.MaPhieuMuon);
        cmd.Parameters.AddWithValue("@TrangThaiSach",  dto.TrangThaiSach);
        cmd.Parameters.AddWithValue("@NhanVienThu",   dto.NhanVienThu);
        cmd.Parameters.AddWithValue("@GhiChu",        (object?)dto.GhiChu ?? DBNull.Value);

        var outParam = new SqlParameter("@MaPhieuTra", SqlDbType.Int)
            { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(outParam);

        TraSachResultDto result = new();
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            result.MaPhieuTra   = reader.GetInt32(reader.GetOrdinal("MaPhieuTra"));
            result.TienPhat     = reader.GetDecimal(reader.GetOrdinal("TienPhat"));
            result.SoNgayTreHan = reader.GetInt32(reader.GetOrdinal("SoNgayTreHan"));
        }
        db.Database.CloseConnection();
        return result;
    }

    public async Task<PhieuTraDto?> GetByIdAsync(int maPhieuTra)
    {
        return await db.PhieuTras
            .Include(pt => pt.PhieuMuon).ThenInclude(pm => pm.DocGia)
            .Where(pt => pt.MaPhieuTra == maPhieuTra)
            .Select(pt => ToDto(pt))
            .FirstOrDefaultAsync();
    }

    public async Task<PhieuTraDto?> GetByPhieuMuonAsync(int maPhieuMuon)
    {
        return await db.PhieuTras
            .Include(pt => pt.PhieuMuon).ThenInclude(pm => pm.DocGia)
            .Where(pt => pt.MaPhieuMuon == maPhieuMuon)
            .Select(pt => ToDto(pt))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<PhieuTraDto>> GetChuaThuPhatAsync()
    {
        return await db.PhieuTras
            .Include(pt => pt.PhieuMuon).ThenInclude(pm => pm.DocGia)
            .Where(pt => !pt.DaThuPhat && pt.TienPhat > 0)
            .Select(pt => ToDto(pt))
            .ToListAsync();
    }

    public async Task ThuPhatAsync(int maPhieuTra)
    {
        var pt = await db.PhieuTras.FindAsync(maPhieuTra)
            ?? throw new KeyNotFoundException($"Phiếu trả {maPhieuTra} không tồn tại");
        pt.DaThuPhat = true;
        await db.SaveChangesAsync();
    }

    // ── helper ──────────────────────────────────────────────────────────────
    private static PhieuTraDto ToDto(Domain.Entities.PhieuTra pt) => new()
    {
        MaPhieuTra   = pt.MaPhieuTra,
        MaPhieuMuon  = pt.MaPhieuMuon,
        TenDocGia    = pt.PhieuMuon.DocGia.HoTen,
        NgayTra      = pt.NgayTra,
        SoNgayMuon   = pt.SoNgayMuon,
        SoNgayTreHan = pt.SoNgayTreHan,
        TienPhat     = pt.TienPhat,
        DaThuPhat    = pt.DaThuPhat,
        TrangThaiSach = pt.TrangThaiSach
    };
}
