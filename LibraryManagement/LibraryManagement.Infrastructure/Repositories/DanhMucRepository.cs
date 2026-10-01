using LibraryManagement.Application.Common;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class DanhMucRepository(AppDbContext db) : IDanhMucRepository
{
    public async Task<IEnumerable<LookupItemDto>> GetTheLoaisAsync() =>
        await db.TheLoais.AsNoTracking()
            .OrderBy(t => t.TenTheLoai)
            .Select(t => new LookupItemDto { Id = t.MaTheLoai, Ten = t.TenTheLoai })
            .ToListAsync();

    public async Task<IEnumerable<LookupItemDto>> GetTacGiasAsync() =>
        await db.TacGias.AsNoTracking()
            .OrderBy(t => t.TenTacGia)
            .Select(t => new LookupItemDto { Id = t.MaTacGia, Ten = t.TenTacGia })
            .ToListAsync();

    public async Task<IEnumerable<LookupItemDto>> GetNhaXuatBansAsync() =>
        await db.NhaXuatBans.AsNoTracking()
            .OrderBy(n => n.TenNXB)
            .Select(n => new LookupItemDto { Id = n.MaNXB, Ten = n.TenNXB })
            .ToListAsync();

    public async Task AddTheLoaiAsync(string ten)
    {
        ten = ChuanHoa(ten);
        if (await db.TheLoais.AnyAsync(t => t.TenTheLoai == ten))
            throw new InvalidOperationException($"Thể loại \"{ten}\" đã tồn tại.");
        db.TheLoais.Add(new TheLoai { TenTheLoai = ten });
        await db.SaveChangesAsync();
    }

    public async Task AddTacGiaAsync(string ten)
    {
        ten = ChuanHoa(ten);
        if (await db.TacGias.AnyAsync(t => t.TenTacGia == ten))
            throw new InvalidOperationException($"Tác giả \"{ten}\" đã tồn tại.");
        db.TacGias.Add(new TacGia { TenTacGia = ten });
        await db.SaveChangesAsync();
    }

    public async Task AddNhaXuatBanAsync(string ten)
    {
        ten = ChuanHoa(ten);
        if (await db.NhaXuatBans.AnyAsync(n => n.TenNXB == ten))
            throw new InvalidOperationException($"NXB \"{ten}\" đã tồn tại.");
        db.NhaXuatBans.Add(new NhaXuatBan { TenNXB = ten });
        await db.SaveChangesAsync();
    }

    public async Task DeleteTheLoaiAsync(int id)
    {
        if (await db.Sachs.AnyAsync(s => s.MaTheLoai == id))
            throw new InvalidOperationException("Không thể xóa: đang có sách thuộc thể loại này.");
        await db.TheLoais.Where(t => t.MaTheLoai == id).ExecuteDeleteAsync();
    }

    public async Task DeleteTacGiaAsync(int id)
    {
        if (await db.Sachs.AnyAsync(s => s.MaTacGia == id))
            throw new InvalidOperationException("Không thể xóa: đang có sách của tác giả này.");
        await db.TacGias.Where(t => t.MaTacGia == id).ExecuteDeleteAsync();
    }

    public async Task DeleteNhaXuatBanAsync(int id)
    {
        if (await db.Sachs.AnyAsync(s => s.MaNXB == id))
            throw new InvalidOperationException("Không thể xóa: đang có sách của NXB này.");
        await db.NhaXuatBans.Where(n => n.MaNXB == id).ExecuteDeleteAsync();
    }

    private static string ChuanHoa(string ten)
    {
        if (string.IsNullOrWhiteSpace(ten))
            throw new ArgumentException("Tên không được để trống.");
        return ten.Trim();
    }
}
