namespace LibraryManagement.Application.DTOs.MuonSach;
public class PhieuMuonQuaHanDto {
    public int MaPhieuMuon { get; set; }
    public string TenDocGia { get; set; } = "";
    public string? Lop { get; set; }
    public string? Email { get; set; }
    public DateOnly NgayHanTra { get; set; }
    public int SoNgayTre { get; set; }
    public decimal TienPhatUocTinh { get; set; }
}
