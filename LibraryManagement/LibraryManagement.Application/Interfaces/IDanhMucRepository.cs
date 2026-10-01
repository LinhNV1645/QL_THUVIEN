using LibraryManagement.Application.Common;
namespace LibraryManagement.Application.Interfaces;
public interface IDanhMucRepository {
    Task<IEnumerable<LookupItemDto>> GetTheLoaisAsync();
    Task<IEnumerable<LookupItemDto>> GetTacGiasAsync();
    Task<IEnumerable<LookupItemDto>> GetNhaXuatBansAsync();

    Task AddTheLoaiAsync(string ten);
    Task AddTacGiaAsync(string ten);
    Task AddNhaXuatBanAsync(string ten);

    /// <exception cref="InvalidOperationException">Khi danh mục đang được sách sử dụng.</exception>
    Task DeleteTheLoaiAsync(int id);
    /// <exception cref="InvalidOperationException">Khi danh mục đang được sách sử dụng.</exception>
    Task DeleteTacGiaAsync(int id);
    /// <exception cref="InvalidOperationException">Khi danh mục đang được sách sử dụng.</exception>
    Task DeleteNhaXuatBanAsync(int id);
}
