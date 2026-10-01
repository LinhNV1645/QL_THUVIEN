using LibraryManagement.Application.DTOs.CauHinh;
namespace LibraryManagement.Application.Interfaces;
public interface ICauHinhRepository {
    Task<string?> GetValueAsync(string tenCauHinh);
    Task<IEnumerable<CauHinhDto>> GetAllAsync();
    Task UpdateAsync(string tenCauHinh, string giaTri);
}
