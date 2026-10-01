namespace LibraryManagement.Application.DTOs.MuonSach;
public class PhieuMuonFilterDto {
    public int? MaDocGia { get; set; }
    public byte? TrangThai { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
