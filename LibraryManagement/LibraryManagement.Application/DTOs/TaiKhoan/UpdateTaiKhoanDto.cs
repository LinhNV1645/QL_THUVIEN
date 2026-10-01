namespace LibraryManagement.Application.DTOs.TaiKhoan;
public class UpdateTaiKhoanDto {
    public int MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = "";
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public int MaVaiTro { get; set; }
}
