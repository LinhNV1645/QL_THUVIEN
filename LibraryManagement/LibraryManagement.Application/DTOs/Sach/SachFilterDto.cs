namespace LibraryManagement.Application.DTOs.Sach;
public class SachFilterDto {
    public string? TuKhoa { get; set; }
    public int? MaTheLoai { get; set; }
    public int? MaTacGia { get; set; }
    public int? MaNXB { get; set; }
    public bool ChiConTon { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
