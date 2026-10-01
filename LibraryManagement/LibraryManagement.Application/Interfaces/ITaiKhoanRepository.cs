using LibraryManagement.Application.DTOs.TaiKhoan;
namespace LibraryManagement.Application.Interfaces;
public interface ITaiKhoanRepository {
    Task<TaiKhoanDto?> GetByUsernameAsync(string tenDangNhap);
    Task<IEnumerable<TaiKhoanDto>> GetAllAsync();
    Task<int> CreateAsync(CreateTaiKhoanDto dto);
    Task UpdateAsync(UpdateTaiKhoanDto dto);
    Task ChangePasswordAsync(int maTK, string matKhauMoi);
    Task SetTrangThaiAsync(int maTK, byte trangThai);
}
