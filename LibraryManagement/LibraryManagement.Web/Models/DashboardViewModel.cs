using LibraryManagement.Application.DTOs.MuonSach;
using LibraryManagement.Application.DTOs.ThongKe;

namespace LibraryManagement.Web.Models;

public class DashboardViewModel
{
    public DashboardDto TongQuan { get; set; } = new();
    public IReadOnlyList<PhieuMuonQuaHanDto> QuaHan { get; set; } = [];

    public IReadOnlyList<PhieuMuonSapDenHanDto> SapDenHan { get; set; } = [];
    public int TongSoSapDenHan { get; set; }
    public int SoNgaySapDenHan { get; set; }

    public IReadOnlyList<SachMuonNhieuDto> TopSach { get; set; } = [];
    public IReadOnlyList<DocGiaMuonNhieuDto> TopDocGia { get; set; } = [];
    public string KyThongKe { get; set; } = "nam";
    public string TenKyThongKe { get; set; } = "";

    public bool LaNhanVien { get; set; }
}
