namespace LibraryManagement.Application.DTOs.DocGia;
public class LichSuMuonDto {
    public int MaPhieuMuon { get; set; }
    public DateOnly NgayMuon { get; set; }
    public DateOnly NgayHanTra { get; set; }
    public DateOnly? NgayTra { get; set; }
    public string TenSach { get; set; } = "";
    public string TinhTrangTra { get; set; } = "";
    public decimal TienPhat { get; set; }
}
