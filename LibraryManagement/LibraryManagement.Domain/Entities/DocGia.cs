namespace LibraryManagement.Domain.Entities;
public class DocGia {
    public int MaDocGia { get; set; }
    public string HoTen { get; set; } = "";
    public string? Lop { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public bool? GioiTinh { get; set; }
    public string? DiaChi { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public DateOnly NgayDangKy { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public byte TrangThai { get; set; } = 1;
    public ICollection<PhieuMuon> PhieuMuons { get; set; } = [];
}
