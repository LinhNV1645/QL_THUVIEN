using LibraryManagement.Application.Common;
using LibraryManagement.Application.DTOs.Sach;
namespace LibraryManagement.Application.Interfaces;
public interface ISachRepository {
    Task<PaginatedResult<SachDto>> GetAllAsync(SachFilterDto filter);
    Task<SachDto?> GetByIdAsync(int maSach);
    Task<SachDto?> GetByQrAsync(string maQR);
    Task<int> CreateAsync(CreateSachDto dto);
    Task UpdateAsync(UpdateSachDto dto);
    Task SoftDeleteAsync(int maSach);
    Task<int> GetTonKhoAsync(int maSach);
}
