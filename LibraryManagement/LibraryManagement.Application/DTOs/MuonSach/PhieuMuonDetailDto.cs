using LibraryManagement.Application.DTOs.Sach;
namespace LibraryManagement.Application.DTOs.MuonSach;
public class PhieuMuonDetailDto : PhieuMuonDto {
    public int MaDocGia { get; set; }
    public string? Email { get; set; }
    public string? GhiChu { get; set; }
    public List<SachDto> DanhSachSach { get; set; } = [];
    public List<GiaHanDto> LichSuGiaHan { get; set; } = [];
}
