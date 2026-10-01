using LibraryManagement.Application.DTOs.MuonSach;
using LibraryManagement.Application.DTOs.ThongKe;
namespace LibraryManagement.Application.Interfaces;
public interface IThongKeRepository {
    Task<DashboardDto> GetDashboardAsync();
    Task<IEnumerable<PhieuMuonQuaHanDto>> GetTopQuaHanAsync(int top);
    Task<IEnumerable<SachMuonNhieuDto>> GetSachMuonNhieuAsync(int thang, int nam, int topN);
    Task<IEnumerable<DocGiaMuonNhieuDto>> GetDocGiaMuonNhieuAsync(int thang, int nam, int topN);

    // Dashboard
    Task<(IReadOnlyList<PhieuMuonSapDenHanDto> Items, int TongSo)> GetSapDenHanAsync(int soNgay, int topN);
    Task<IReadOnlyList<SachMuonNhieuDto>> GetTopSachAsync(DateOnly? tuNgay, DateOnly? denNgay, int topN);
    Task<IReadOnlyList<DocGiaMuonNhieuDto>> GetTopDocGiaAsync(DateOnly? tuNgay, DateOnly? denNgay, int topN);
}
