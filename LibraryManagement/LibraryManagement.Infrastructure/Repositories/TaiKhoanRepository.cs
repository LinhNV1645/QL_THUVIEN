using LibraryManagement.Application.Common;
using LibraryManagement.Application.DTOs.TaiKhoan;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class TaiKhoanRepository(AppDbContext db) : ITaiKhoanRepository
{
    public async Task<TaiKhoanDto?> XacThucAsync(string tenDangNhap, string matKhau)
    {
        if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            return null;

        var tk = await db.TaiKhoans
            .Include(t => t.VaiTro)
            .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap.Trim() && t.TrangThai == 1);
        if (tk is null) return null;

        bool hopLe;
        try
        {
            hopLe = BCrypt.Net.BCrypt.Verify(matKhau, tk.MatKhau);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash trong DB không đúng định dạng BCrypt
            hopLe = false;
        }
        if (!hopLe) return null;

        tk.LanDangNhapCuoi = DateTime.Now;
        await db.SaveChangesAsync();
        return ToDto(tk);
    }

    public async Task<TaiKhoanDto?> GetByIdAsync(int maTK)
    {
        var tk = await db.TaiKhoans
            .AsNoTracking()
            .Include(t => t.VaiTro)
            .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTK);
        return tk is null ? null : ToDto(tk);
    }

    public async Task<TaiKhoanDto?> GetByUsernameAsync(string tenDangNhap)
    {
        var tk = await db.TaiKhoans
            .AsNoTracking()
            .Include(t => t.VaiTro)
            .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);
        return tk is null ? null : ToDto(tk);
    }

    public async Task<IEnumerable<TaiKhoanDto>> GetAllAsync()
    {
        return await db.TaiKhoans
            .AsNoTracking()
            .Include(t => t.VaiTro)
            .OrderBy(t => t.HoTen)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItemDto>> GetVaiTrosAsync()
    {
        return await db.VaiTros
            .AsNoTracking()
            .OrderBy(v => v.MaVaiTro)
            .Select(v => new LookupItemDto { Id = v.MaVaiTro, Ten = v.TenVaiTro })
            .ToListAsync();
    }

    public async Task<int> CreateAsync(CreateTaiKhoanDto dto)
    {
        var tenDangNhap = dto.TenDangNhap.Trim();
        if (await db.TaiKhoans.AnyAsync(t => t.TenDangNhap == tenDangNhap))
            throw new InvalidOperationException($"Tên đăng nhập '{tenDangNhap}' đã tồn tại.");

        var tk = new TaiKhoan
        {
            MaVaiTro    = dto.MaVaiTro,
            TenDangNhap = tenDangNhap,
            MatKhau     = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
            HoTen       = dto.HoTen,
            Email       = dto.Email,
            SoDienThoai = dto.SoDienThoai,
            NgayTao     = DateTime.Now,
            TrangThai   = 1
        };
        db.TaiKhoans.Add(tk);
        await db.SaveChangesAsync();
        return tk.MaTaiKhoan;
    }

    public async Task UpdateAsync(UpdateTaiKhoanDto dto)
    {
        var tk = await db.TaiKhoans.FindAsync(dto.MaTaiKhoan)
            ?? throw new KeyNotFoundException($"Tài khoản {dto.MaTaiKhoan} không tồn tại");
        tk.HoTen       = dto.HoTen;
        tk.Email       = dto.Email;
        tk.SoDienThoai = dto.SoDienThoai;
        tk.MaVaiTro    = dto.MaVaiTro;
        await db.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int maTK, string matKhauMoi)
    {
        var tk = await db.TaiKhoans.FindAsync(maTK)
            ?? throw new KeyNotFoundException($"Tài khoản {maTK} không tồn tại");
        tk.MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);
        await db.SaveChangesAsync();
    }

    public async Task SetTrangThaiAsync(int maTK, byte trangThai)
    {
        var tk = await db.TaiKhoans.FindAsync(maTK)
            ?? throw new KeyNotFoundException($"Tài khoản {maTK} không tồn tại");
        tk.TrangThai = trangThai;
        await db.SaveChangesAsync();
    }

    // ── helper ──────────────────────────────────────────────────────────────
    private static TaiKhoanDto ToDto(TaiKhoan t) => new()
    {
        MaTaiKhoan  = t.MaTaiKhoan,
        TenDangNhap = t.TenDangNhap,
        HoTen       = t.HoTen,
        TenVaiTro   = t.VaiTro.TenVaiTro,
        MaVaiTro    = t.MaVaiTro,
        Email       = t.Email,
        SoDienThoai = t.SoDienThoai,
        TrangThai   = t.TrangThai
    };
}
