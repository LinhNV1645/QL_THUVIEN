namespace LibraryManagement.Application.DTOs.Sach;
public class CreateSachDto {
    public int MaTheLoai { get; set; }
    public int MaTacGia { get; set; }
    public int MaNXB { get; set; }
    public string TenSach { get; set; } = "";
    public short? NamXuatBan { get; set; }
    public short? SoTrang { get; set; }
    public int SoLuongNhap { get; set; }
    public string? ViTri { get; set; }
    public string? MoTa { get; set; }
}
