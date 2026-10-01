namespace LibraryManagement.Domain.Entities;
public class TheLoai {
    public int MaTheLoai { get; set; }
    public string TenTheLoai { get; set; } = "";
    public string? MoTa { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public ICollection<Sach> Sachs { get; set; } = [];
}
