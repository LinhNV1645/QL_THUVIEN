namespace LibraryManagement.Application.DTOs.MuonSach;
public class PhieuMuonDto {
    public int MaPhieuMuon { get; set; }
    public string TenDocGia { get; set; } = "";
    public string Lop { get; set; } = "";
    public DateOnly NgayMuon { get; set; }
    public DateOnly NgayHanTra { get; set; }
    public byte TrangThai { get; set; }
    public string TrangThaiText { get; set; } = "";
    public int SoNgayTreHan { get; set; }
    public int SoSachMuon { get; set; }
}
