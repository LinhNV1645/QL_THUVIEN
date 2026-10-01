namespace LibraryManagement.Application.DTOs.TraSach;
public class TraSachDto {
    public int MaPhieuMuon { get; set; }
    public byte TrangThaiSach { get; set; } = 1;
    public int NhanVienThu { get; set; }
    public string? GhiChu { get; set; }
}
