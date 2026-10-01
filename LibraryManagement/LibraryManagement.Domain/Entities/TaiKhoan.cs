namespace LibraryManagement.Domain.Entities;
public class TaiKhoan {
    public int MaTaiKhoan { get; set; }
    public int MaVaiTro { get; set; }
    public int? MaDocGia { get; set; }
    public string TenDangNhap { get; set; } = "";
    public string MatKhau { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public DateTime? LanDangNhapCuoi { get; set; }
    public byte TrangThai { get; set; } = 1;
    public VaiTro VaiTro { get; set; } = null!;
    public DocGia? DocGia { get; set; }
}
