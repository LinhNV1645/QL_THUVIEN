using LibraryManagement.Application.Common;
using LibraryManagement.Application.DTOs.Sach;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class SachRepository(AppDbContext db) : ISachRepository
{
    public async Task<PaginatedResult<SachDto>> GetAllAsync(SachFilterDto filter)
    {
        var q = db.Sachs
            .Include(s => s.TheLoai)
            .Include(s => s.TacGia)
            .Include(s => s.NhaXuatBan)
            .Where(s => s.TrangThai != 0)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.TuKhoa))
        {
            var kw = filter.TuKhoa.Trim();
            q = q.Where(s => s.TenSach.Contains(kw)
                           || s.TacGia.TenTacGia.Contains(kw)
                           || s.MaQR == kw
                           || (s.MoTa != null && s.MoTa.Contains(kw)));
        }

        if (filter.MaTheLoai.HasValue) q = q.Where(s => s.MaTheLoai == filter.MaTheLoai.Value);
        if (filter.MaTacGia.HasValue)  q = q.Where(s => s.MaTacGia  == filter.MaTacGia.Value);
        if (filter.MaNXB.HasValue)     q = q.Where(s => s.MaNXB     == filter.MaNXB.Value);
        if (filter.ChiConTon)          q = q.Where(s => s.SoLuongTon > 0);

        var page     = Math.Max(1, filter.Page);
        var pageSize = Math.Max(1, filter.PageSize);

        var total = await q.CountAsync();
        var items = await q
            .AsNoTracking()
            .OrderBy(s => s.TenSach)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => ToDto(s))
            .ToListAsync();

        return new PaginatedResult<SachDto>
        {
            Items = items, TotalCount = total,
            Page = page, PageSize = pageSize
        };
    }

    public async Task<SachDto?> GetByIdAsync(int maSach)
    {
        var s = await db.Sachs
            .Include(x => x.TheLoai).Include(x => x.TacGia).Include(x => x.NhaXuatBan)
            .FirstOrDefaultAsync(x => x.MaSach == maSach);
        return s is null ? null : ToDto(s);
    }

    public async Task<SachDto?> GetByQrAsync(string maQR)
    {
        var s = await db.Sachs
            .Include(x => x.TheLoai).Include(x => x.TacGia).Include(x => x.NhaXuatBan)
            .FirstOrDefaultAsync(x => x.MaQR == maQR);
        return s is null ? null : ToDto(s);
    }

    public async Task<int> CreateAsync(CreateSachDto dto)
    {
        var maQR = $"SACH-{Guid.NewGuid():N}".ToUpper()[..20];
        var sach = new Sach
        {
            MaTheLoai    = dto.MaTheLoai,
            MaTacGia     = dto.MaTacGia,
            MaNXB        = dto.MaNXB,
            TenSach      = dto.TenSach,
            NamXuatBan   = dto.NamXuatBan,
            SoTrang      = dto.SoTrang,
            SoLuongNhap  = dto.SoLuongNhap,
            SoLuongTon   = dto.SoLuongNhap,
            ViTri        = dto.ViTri,
            MaQR         = maQR,
            MoTa         = dto.MoTa,
            NgayNhap     = DateOnly.FromDateTime(DateTime.Today),
            TrangThai    = 1
        };
        db.Sachs.Add(sach);
        await db.SaveChangesAsync();
        return sach.MaSach;
    }

    public async Task UpdateAsync(UpdateSachDto dto)
    {
        var sach = await db.Sachs.FindAsync(dto.MaSach)
            ?? throw new KeyNotFoundException($"Sách {dto.MaSach} không tồn tại");

        // Điều chỉnh tồn kho theo phần chênh lệch số lượng nhập
        var chenhLech = dto.SoLuongNhap - sach.SoLuongNhap;
        if (sach.SoLuongTon + chenhLech < 0)
            throw new InvalidOperationException(
                $"Không thể giảm số lượng nhập xuống {dto.SoLuongNhap}: " +
                $"đang có {sach.SoLuongNhap - sach.SoLuongTon} cuốn được mượn hoặc đã mất.");
        sach.SoLuongNhap += chenhLech;
        sach.SoLuongTon  += chenhLech;

        sach.MaTheLoai   = dto.MaTheLoai;
        sach.MaTacGia    = dto.MaTacGia;
        sach.MaNXB       = dto.MaNXB;
        sach.TenSach     = dto.TenSach;
        sach.NamXuatBan  = dto.NamXuatBan;
        sach.SoTrang     = dto.SoTrang;
        sach.ViTri       = dto.ViTri;
        sach.MoTa        = dto.MoTa;
        await db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int maSach)
    {
        var sach = await db.Sachs.FindAsync(maSach)
            ?? throw new KeyNotFoundException($"Sách {maSach} không tồn tại");

        var dangMuon = await db.CTPhieuMuons.AnyAsync(ct => ct.MaSach == maSach && ct.TrangThaiCT == 1);
        if (dangMuon)
            throw new InvalidOperationException(
                "Không thể ẩn sách đang được mượn. Vui lòng chờ độc giả trả sách.");

        sach.TrangThai = 0;
        await db.SaveChangesAsync();
    }

    public async Task<int> GetTonKhoAsync(int maSach)
    {
        var s = await db.Sachs.FindAsync(maSach);
        return s?.SoLuongTon ?? 0;
    }

    // ── helper ──────────────────────────────────────────────────────────────
    private static SachDto ToDto(Sach s) => new()
    {
        MaSach      = s.MaSach,
        TenSach     = s.TenSach,
        TenTheLoai  = s.TheLoai.TenTheLoai,
        TenTacGia   = s.TacGia.TenTacGia,
        TenNXB      = s.NhaXuatBan.TenNXB,
        MaTheLoai   = s.MaTheLoai,
        MaTacGia    = s.MaTacGia,
        MaNXB       = s.MaNXB,
        NamXuatBan  = s.NamXuatBan,
        SoTrang     = s.SoTrang,
        SoLuongTon  = s.SoLuongTon,
        SoLuongNhap = s.SoLuongNhap,
        ViTri       = s.ViTri,
        MaQR        = s.MaQR,
        MoTa        = s.MoTa,
        TrangThai   = s.TrangThai
    };
}
