namespace LibraryManagement.Domain.Entities;
public class Sach {
    public int MaSach { get; set; }
    public int MaTheLoai { get; set; }
    public int MaTacGia { get; set; }
    public int MaNXB { get; set; }
    public string TenSach { get; set; } = "";
    public short? NamXuatBan { get; set; }
    public short? SoTrang { get; set; }
    public int SoLuongNhap { get; set; }
    public int SoLuongTon { get; set; }
    public string? ViTri { get; set; }
    public string? MaQR { get; set; }
    public string? MoTa { get; set; }
    public DateOnly NgayNhap { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public byte TrangThai { get; set; } = 1;
    public TheLoai TheLoai { get; set; } = null!;
    public TacGia TacGia { get; set; } = null!;
    public NhaXuatBan NhaXuatBan { get; set; } = null!;
    public ICollection<CTPhieuMuon> CTPhieuMuons { get; set; } = [];
}
