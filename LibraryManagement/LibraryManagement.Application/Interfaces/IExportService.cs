using LibraryManagement.Application.DTOs.Sach;
namespace LibraryManagement.Application.Interfaces;
public interface IExportService {
    Task<byte[]> ExportSachToExcelAsync(SachFilterDto? filter = null);
    Task<byte[]> ExportLichSuMuonToExcelAsync(int? maDocGia, int thang, int nam);
    Task<byte[]> ExportThongKeToExcelAsync(int thang, int nam);
    Task<byte[]> ExportSachToPdfAsync(SachFilterDto? filter = null);
}
