using LibraryManagement.Application.Common;
using LibraryManagement.Application.DTOs.MuonSach;
namespace LibraryManagement.Application.Interfaces;
public interface IPhieuMuonRepository {
    Task<int> LapPhieuMuonAsync(LapPhieuMuonDto dto);
    Task<PhieuMuonDetailDto?> GetByIdAsync(int maPhieu);
    Task<PaginatedResult<PhieuMuonDto>> GetAllAsync(PhieuMuonFilterDto filter);
    Task<IEnumerable<PhieuMuonQuaHanDto>> GetQuaHanAsync();
    Task<IEnumerable<PhieuMuonDto>> GetDangMuonByDocGiaAsync(int maDocGia);
}
