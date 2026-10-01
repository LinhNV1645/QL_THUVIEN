namespace LibraryManagement.Application.DTOs.MuonSach;
public class LapPhieuMuonDto {
    public int MaDocGia { get; set; }
    public List<SachMuonItem> DanhSachSach { get; set; } = [];
    public int NhanVienLap { get; set; }
    public string? GhiChu { get; set; }
}
public class SachMuonItem {
    public int MaSach { get; set; }
    public int SoLuong { get; set; } = 1;
}
