namespace LibraryManagement.Application.DTOs.TaiKhoan;
public class TaiKhoanDto {
    public int MaTaiKhoan { get; set; }
    public string TenDangNhap { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string TenVaiTro { get; set; } = "";
    public int MaVaiTro { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public byte TrangThai { get; set; }
}
