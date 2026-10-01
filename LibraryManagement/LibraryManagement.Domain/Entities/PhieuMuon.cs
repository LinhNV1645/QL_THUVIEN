namespace LibraryManagement.Domain.Entities;
public class PhieuMuon {
    public int MaPhieuMuon { get; set; }
    public int MaDocGia { get; set; }
    public DateOnly NgayMuon { get; set; }
    public DateOnly NgayHanTra { get; set; }
    public byte TrangThai { get; set; } = 1; // 1=Đang mượn,2=Đã trả,3=Quá hạn,4=Đã gia hạn
    public string? GhiChu { get; set; }
    public int? NhanVienLap { get; set; }
    public DocGia DocGia { get; set; } = null!;
    public ICollection<CTPhieuMuon> CTPhieuMuons { get; set; } = [];
    public ICollection<GiaHan> GiaHans { get; set; } = [];
    public PhieuTra? PhieuTra { get; set; }
}
