namespace LibraryManagement.Domain.Entities;
public class CauHinhHeThong {
    public int MaCauHinh { get; set; }
    public string TenCauHinh { get; set; } = "";
    public string GiaTri { get; set; } = "";
    public string? GhiChu { get; set; }
}
