using LibraryManagement.Application.DTOs.TraSach;
namespace LibraryManagement.Application.Interfaces;
public interface IPhieuTraRepository {
    Task<TraSachResultDto> TraSachAsync(TraSachDto dto);
    Task<PhieuTraDto?> GetByIdAsync(int maPhieuTra);
    Task<PhieuTraDto?> GetByPhieuMuonAsync(int maPhieuMuon);
    Task<IEnumerable<PhieuTraDto>> GetChuaThuPhatAsync();
    Task ThuPhatAsync(int maPhieuTra);
}
