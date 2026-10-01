namespace LibraryManagement.Application.DTOs.ThongKe;
public class SachMuonNhieuDto {
    public int MaSach { get; set; }
    public string TenSach { get; set; } = "";
    public string TenTacGia { get; set; } = "";
    public string TenTheLoai { get; set; } = "";
    public int SoLuotMuon { get; set; }
    public int TongCuonMuon { get; set; }
    public int SoLuongNhap { get; set; }
    public int SoLuongTon { get; set; }
}
