using LibraryManagement.Application.Common;
using LibraryManagement.Application.DTOs.DocGia;
namespace LibraryManagement.Application.Interfaces;
public interface IDocGiaRepository {
    Task<PaginatedResult<DocGiaDto>> GetAllAsync(string? keyword, string? lop, int page, int pageSize);
    Task<DocGiaDto?> GetByIdAsync(int maDocGia);
    Task<int> CreateAsync(CreateDocGiaDto dto);
    Task UpdateAsync(UpdateDocGiaDto dto);
    Task SetTrangThaiAsync(int maDocGia, byte trangThai);
    Task<IEnumerable<LichSuMuonDto>> GetLichSuAsync(int maDocGia);
}
