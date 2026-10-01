namespace LibraryManagement.Application.DTOs.ThongKe;
public class PhieuMuonSapDenHanDto {
    public int MaPhieuMuon { get; set; }
    public DateOnly NgayMuon { get; set; }
    public DateOnly NgayHanTra { get; set; }
    public bool DaGiaHan { get; set; }
    public int MaDocGia { get; set; }
    public string HoTen { get; set; } = "";
    public string? Lop { get; set; }
    public string? SoDienThoai { get; set; }
    public int SoNgayConLai { get; set; }
    public int SoCuon { get; set; }
    public string DanhSachSach { get; set; } = "";
}
