namespace LibraryManagement.Domain.Entities;
public class VaiTro {
    public int MaVaiTro { get; set; }
    public string TenVaiTro { get; set; } = ""; // Admin|NhanVien|DocGia
    public string? MoTa { get; set; }
    public ICollection<TaiKhoan> TaiKhoans { get; set; } = [];
}
