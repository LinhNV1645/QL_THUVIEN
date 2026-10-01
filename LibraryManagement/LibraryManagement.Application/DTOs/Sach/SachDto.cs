namespace LibraryManagement.Application.DTOs.Sach;
public class SachDto {
    public int MaSach { get; set; }
    public string TenSach { get; set; } = "";
    public string TenTheLoai { get; set; } = "";
    public string TenTacGia { get; set; } = "";
    public string TenNXB { get; set; } = "";
    public int MaTheLoai { get; set; }
    public int MaTacGia { get; set; }
    public int MaNXB { get; set; }
    public short? NamXuatBan { get; set; }
    public short? SoTrang { get; set; }
    public int SoLuongTon { get; set; }
    public int SoLuongNhap { get; set; }
    public string? ViTri { get; set; }
    public string? MaQR { get; set; }
    public string? MoTa { get; set; }
    public byte TrangThai { get; set; }
}
