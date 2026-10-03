using System.Data;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Infrastructure.Jobs;

/// <summary>
/// Hangfire job — chạy định kỳ để gửi email nhắc nhở độc giả sắp đến hạn trả sách.
/// Đăng ký trong Program.cs: RecurringJob.AddOrUpdate&lt;NhacNhoHanTraJob&gt;(
///     "nhac-nho-han-tra", j => j.Execute(), "0 7 * * *").
/// </summary>
public class NhacNhoHanTraJob(
    AppDbContext db,
    IEmailService email,
    ILogger<NhacNhoHanTraJob> logger)
{
    private record NhacNhoRow(
        int    MaPhieuMuon,
        DateOnly NgayHanTra,
        string HoTen,
        string? Email,
        string? Lop,
        int    SoNgayConLai);

    public async Task Execute()
    {
        logger.LogInformation("NhacNhoHanTraJob bắt đầu lúc {Time}", DateTime.Now);

        var rows = await LayDanhSachSapDenHanAsync(soNgayTruoc: 3);
        var sachTheoPhieu = await LaySachTheoPhieuAsync(rows.Select(r => r.MaPhieuMuon).ToList());
        int sent = 0, skipped = 0;

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Email))
            {
                logger.LogWarning("Phiếu {MaPhieu}: độc giả {HoTen} không có email, bỏ qua",
                    row.MaPhieuMuon, row.HoTen);
                skipped++;
                continue;
            }

            try
            {
                await email.SendNhacNhoHanTraAsync(
                    toEmail      : row.Email,
                    tenDocGia    : row.HoTen,
                    danhSachSach : sachTheoPhieu.GetValueOrDefault(row.MaPhieuMuon) ?? [],
                    ngayHanTra   : row.NgayHanTra,
                    soNgayConLai : row.SoNgayConLai);
                sent++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Không gửi được email cho {Email}", row.Email);
            }
        }

        logger.LogInformation(
            "NhacNhoHanTraJob hoàn tất — đã gửi: {Sent}, bỏ qua: {Skipped}", sent, skipped);
    }

    private async Task<Dictionary<int, List<string>>> LaySachTheoPhieuAsync(List<int> maPhieus)
    {
        if (maPhieus.Count == 0) return [];

        var rows = await db.CTPhieuMuons
            .AsNoTracking()
            .Where(ct => maPhieus.Contains(ct.MaPhieuMuon))
            .Select(ct => new { ct.MaPhieuMuon, ct.Sach.TenSach })
            .ToListAsync();

        return rows
            .GroupBy(x => x.MaPhieuMuon)
            .ToDictionary(g => g.Key, g => g.Select(x => x.TenSach).ToList());
    }

    // ── gọi sp_LayPhieuMuonSapDenHan ────────────────────────────────────────
    private async Task<IReadOnlyList<NhacNhoRow>> LayDanhSachSapDenHanAsync(int soNgayTruoc)
    {
        var result = new List<NhacNhoRow>();

        await db.Database.OpenConnectionAsync();
        using var cmd = (SqlCommand)db.Database.GetDbConnection().CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "sp_LayPhieuMuonSapDenHan";
        cmd.Parameters.AddWithValue("@SoNgayTruoc", soNgayTruoc);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var ngayRaw = reader["NgayHanTra"];
            var ngayHanTra = ngayRaw is DateOnly d ? d
                : DateOnly.FromDateTime(Convert.ToDateTime(ngayRaw));

            result.Add(new NhacNhoRow(
                MaPhieuMuon  : reader.GetInt32(reader.GetOrdinal("MaPhieuMuon")),
                NgayHanTra   : ngayHanTra,
                HoTen        : reader.GetString(reader.GetOrdinal("HoTen")),
                Email        : reader["Email"] as string,
                Lop          : reader["Lop"] as string,
                SoNgayConLai : reader.GetInt32(reader.GetOrdinal("SoNgayConLai"))
            ));
        }

        db.Database.CloseConnection();
        return result;
    }
}
