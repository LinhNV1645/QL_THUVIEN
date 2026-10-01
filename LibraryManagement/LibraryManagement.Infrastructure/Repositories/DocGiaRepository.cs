using LibraryManagement.Application.Common;
using LibraryManagement.Application.DTOs.DocGia;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class DocGiaRepository(AppDbContext db) : IDocGiaRepository
{
    public async Task<PaginatedResult<DocGiaDto>> GetAllAsync(
        string? keyword, string? lop, int page, int pageSize)
    {
        var q = db.DocGias.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(d => d.HoTen.Contains(keyword)
                           || (d.Email != null && d.Email.Contains(keyword))
                           || (d.SoDienThoai != null && d.SoDienThoai.Contains(keyword)));

        if (!string.IsNullOrWhiteSpace(lop))
            q = q.Where(d => d.Lop == lop);

        var total = await q.CountAsync();
        var items = await q
            .OrderBy(d => d.HoTen)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => ToDto(d))
            .ToListAsync();

        return new PaginatedResult<DocGiaDto>
        {
            Items = items, TotalCount = total,
            Page = page, PageSize = pageSize
        };
    }

    public async Task<DocGiaDto?> GetByIdAsync(int maDocGia)
    {
        var d = await db.DocGias.FindAsync(maDocGia);
        return d is null ? null : ToDto(d);
    }

    public async Task<int> CreateAsync(CreateDocGiaDto dto)
    {
        var docGia = new DocGia
        {
            HoTen       = dto.HoTen,
            Lop         = dto.Lop,
            NgaySinh    = dto.NgaySinh,
            GioiTinh    = dto.GioiTinh,
            DiaChi      = dto.DiaChi,
            Email       = dto.Email,
            SoDienThoai = dto.SoDienThoai,
            NgayDangKy  = DateOnly.FromDateTime(DateTime.Today),
            TrangThai   = 1
        };
        db.DocGias.Add(docGia);
        await db.SaveChangesAsync();
        return docGia.MaDocGia;
    }

    public async Task UpdateAsync(UpdateDocGiaDto dto)
    {
        var docGia = await db.DocGias.FindAsync(dto.MaDocGia)
            ?? throw new KeyNotFoundException($"Độc giả {dto.MaDocGia} không tồn tại");
        docGia.HoTen       = dto.HoTen;
        docGia.Lop         = dto.Lop;
        docGia.NgaySinh    = dto.NgaySinh;
        docGia.GioiTinh    = dto.GioiTinh;
        docGia.DiaChi      = dto.DiaChi;
        docGia.Email       = dto.Email;
        docGia.SoDienThoai = dto.SoDienThoai;
        await db.SaveChangesAsync();
    }

    public async Task SetTrangThaiAsync(int maDocGia, byte trangThai)
    {
        var docGia = await db.DocGias.FindAsync(maDocGia)
            ?? throw new KeyNotFoundException($"Độc giả {maDocGia} không tồn tại");
        docGia.TrangThai = trangThai;
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<LichSuMuonDto>> GetLichSuAsync(int maDocGia)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await db.PhieuMuons
            .AsNoTracking()
            .Where(p => p.MaDocGia == maDocGia)
            .OrderByDescending(p => p.NgayMuon)
            .SelectMany(p => p.CTPhieuMuons.Select(ct => new LichSuMuonDto
            {
                MaPhieuMuon  = p.MaPhieuMuon,
                NgayMuon     = p.NgayMuon,
                NgayHanTra   = p.NgayHanTra,
                NgayTra      = p.PhieuTra != null ? p.PhieuTra.NgayTra : null,
                TenSach      = ct.Sach.TenSach,
                TinhTrangTra = p.TrangThai == 2 ? "Đã trả"
                             : p.TrangThai == 3 || p.NgayHanTra < today ? "Quá hạn"
                             : p.TrangThai == 4 ? "Đã gia hạn"
                             : "Đang mượn",
                TienPhat     = p.PhieuTra != null ? p.PhieuTra.TienPhat : 0
            }))
            .ToListAsync();
    }

    // ── helper ──────────────────────────────────────────────────────────────
    private static DocGiaDto ToDto(DocGia d) => new()
    {
        MaDocGia    = d.MaDocGia,
        HoTen       = d.HoTen,
        Lop         = d.Lop,
        NgaySinh    = d.NgaySinh,
        GioiTinh    = d.GioiTinh,
        DiaChi      = d.DiaChi,
        Email       = d.Email,
        SoDienThoai = d.SoDienThoai,
        NgayDangKy  = d.NgayDangKy,
        TrangThai   = d.TrangThai
    };
}
