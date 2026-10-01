using LibraryManagement.Application.DTOs.CauHinh;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class CauHinhRepository(AppDbContext db) : ICauHinhRepository
{
    public async Task<string?> GetValueAsync(string tenCauHinh)
    {
        var ch = await db.CauHinhHeThongs
            .FirstOrDefaultAsync(c => c.TenCauHinh == tenCauHinh);
        return ch?.GiaTri;
    }

    public async Task<IEnumerable<CauHinhDto>> GetAllAsync()
    {
        return await db.CauHinhHeThongs
            .OrderBy(c => c.TenCauHinh)
            .Select(c => new CauHinhDto
            {
                MaCauHinh  = c.MaCauHinh,
                TenCauHinh = c.TenCauHinh,
                GiaTri     = c.GiaTri,
                GhiChu     = c.GhiChu
            })
            .ToListAsync();
    }

    public async Task UpdateAsync(string tenCauHinh, string giaTri)
    {
        var ch = await db.CauHinhHeThongs
            .FirstOrDefaultAsync(c => c.TenCauHinh == tenCauHinh)
            ?? throw new KeyNotFoundException($"Cấu hình '{tenCauHinh}' không tồn tại");
        ch.GiaTri = giaTri;
        await db.SaveChangesAsync();
    }
}
