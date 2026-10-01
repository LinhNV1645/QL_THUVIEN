using BCrypt.Net;
using LibraryManagement.Application.DTOs.TaiKhoan;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class TaiKhoanRepository(AppDbContext db) : ITaiKhoanRepository
{
    public async Task<TaiKhoanDto?> GetByUsernameAsync(string tenDangNhap)
    {
        var tk = await db.TaiKhoans
            .Include(t => t.VaiTro)
            .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);
        return tk is null ? null : ToDto(tk);
    }

    public async Task<IEnumerable<TaiKhoanDto>> GetAllAsync()
    {
        return await db.TaiKhoans
            .Include(t => t.VaiTro)
            .OrderBy(t => t.HoTen)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<int> CreateAsync(CreateTaiKhoanDto dto)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau);
        var tk = new TaiKhoan
        {
            MaVaiTro    = dto.MaVaiTro,
            TenDangNhap = dto.TenDangNhap,
            MatKhau     = hash,
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
        TrangThai   = t.TrangThai,
        MatKhau     = t.MatKhau
    };
}
