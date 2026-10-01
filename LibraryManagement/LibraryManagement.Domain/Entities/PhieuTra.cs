namespace LibraryManagement.Domain.Entities;
public class PhieuTra {
    public int MaPhieuTra { get; set; }
    public int MaPhieuMuon { get; set; }
    public DateOnly NgayTra { get; set; }
    public int SoNgayMuon { get; set; }
    public int SoNgayTreHan { get; set; }
    public decimal TienPhat { get; set; }
    public bool DaThuPhat { get; set; }
    public byte TrangThaiSach { get; set; } = 1; // 1=OK,2=Hư,3=Mất
    public string? GhiChu { get; set; }
    public int? NhanVienThu { get; set; }
    public PhieuMuon PhieuMuon { get; set; } = null!;
}
