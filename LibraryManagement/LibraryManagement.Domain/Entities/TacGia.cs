namespace LibraryManagement.Domain.Entities;
public class TacGia {
    public int MaTacGia { get; set; }
    public string TenTacGia { get; set; } = "";
    public string? QuocTich { get; set; }
    public string? GhiChu { get; set; }
    public ICollection<Sach> Sachs { get; set; } = [];
}
