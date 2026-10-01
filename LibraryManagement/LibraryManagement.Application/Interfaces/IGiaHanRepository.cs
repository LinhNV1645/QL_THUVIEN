using LibraryManagement.Application.DTOs.MuonSach;
namespace LibraryManagement.Application.Interfaces;
public interface IGiaHanRepository {
    Task<DateOnly> GiaHanAsync(int maPhieuMuon, int nhanVienDuyet, string? ghiChu);
    Task<IEnumerable<GiaHanDto>> GetByPhieuMuonAsync(int maPhieuMuon);
}
