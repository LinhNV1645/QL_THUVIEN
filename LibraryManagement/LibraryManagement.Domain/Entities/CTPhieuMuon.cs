namespace LibraryManagement.Domain.Entities;
public class CTPhieuMuon {
    public int MaCT { get; set; }
    public int MaPhieuMuon { get; set; }
    public int MaSach { get; set; }
    public int SoLuongMuon { get; set; } = 1;
    public byte TrangThaiCT { get; set; } = 1; // 1=Đang mượn,2=Đã trả
    public PhieuMuon PhieuMuon { get; set; } = null!;
    public Sach Sach { get; set; } = null!;
}
