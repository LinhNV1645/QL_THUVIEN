namespace LibraryManagement.Domain.Entities;
public class GiaHan {
    public int MaGiaHan { get; set; }
    public int MaPhieuMuon { get; set; }
    public DateOnly NgayGiaHan { get; set; }
    public DateOnly HanTraCu { get; set; }
    public DateOnly HanTraMoi { get; set; }
    public byte LanGiaHan { get; set; }
    public int? NhanVienDuyet { get; set; }
    public string? GhiChu { get; set; }
    public PhieuMuon PhieuMuon { get; set; } = null!;
}
