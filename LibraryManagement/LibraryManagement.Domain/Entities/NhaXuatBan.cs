namespace LibraryManagement.Domain.Entities;
public class NhaXuatBan {
    public int MaNXB { get; set; }
    public string TenNXB { get; set; } = "";
    public string? DiaChi { get; set; }
    public string? DienThoai { get; set; }
    public ICollection<Sach> Sachs { get; set; } = [];
}
