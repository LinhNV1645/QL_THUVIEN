namespace LibraryManagement.Application.DTOs.TraSach;
public class PhieuTraDto {
    public int MaPhieuTra { get; set; }
    public int MaPhieuMuon { get; set; }
    public string TenDocGia { get; set; } = "";
    public DateOnly NgayTra { get; set; }
    public int SoNgayMuon { get; set; }
    public int SoNgayTreHan { get; set; }
    public decimal TienPhat { get; set; }
    public bool DaThuPhat { get; set; }
    public byte TrangThaiSach { get; set; }
}
