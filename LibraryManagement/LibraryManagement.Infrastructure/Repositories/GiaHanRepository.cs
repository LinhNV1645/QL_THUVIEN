using System.Data;
using LibraryManagement.Application.DTOs.MuonSach;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class GiaHanRepository(AppDbContext db) : IGiaHanRepository
{
    // ── gọi SP, lấy result set ngày hạn trả mới ─────────────────────────────
    public async Task<DateOnly> GiaHanAsync(int maPhieuMuon, int nhanVienDuyet, string? ghiChu)
    {
        await db.Database.OpenConnectionAsync();
        try
        {
            using var cmd = (SqlCommand)db.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_GiaHanPhieuMuon";
            cmd.Parameters.AddWithValue("@MaPhieuMuon",   maPhieuMuon);
            cmd.Parameters.AddWithValue("@NhanVienDuyet", nhanVienDuyet);
            cmd.Parameters.AddWithValue("@GhiChu",
                string.IsNullOrWhiteSpace(ghiChu) ? DBNull.Value : ghiChu.Trim());

            DateOnly ngayHanTraMoi = default;
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var raw = reader.GetValue(0);
                    ngayHanTraMoi = raw is DateOnly d ? d
                        : DateOnly.FromDateTime(Convert.ToDateTime(raw));
                }
            }
            return ngayHanTraMoi;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    public async Task<IEnumerable<GiaHanDto>> GetByPhieuMuonAsync(int maPhieuMuon)
    {
        return await db.GiaHans
            .Where(g => g.MaPhieuMuon == maPhieuMuon)
            .OrderBy(g => g.LanGiaHan)
            .Select(g => new GiaHanDto
            {
                MaGiaHan  = g.MaGiaHan,
                NgayGiaHan = g.NgayGiaHan,
                HanTraCu  = g.HanTraCu,
                HanTraMoi = g.HanTraMoi,
                LanGiaHan = g.LanGiaHan
            })
            .ToListAsync();
    }
}
