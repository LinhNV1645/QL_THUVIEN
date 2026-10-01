namespace LibraryManagement.Application.DTOs.TaiKhoan;
public class CreateTaiKhoanDto {
    public int MaVaiTro { get; set; }
    public string TenDangNhap { get; set; } = "";
    public string MatKhau { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
}
