using LibraryManagement.Application.Common;
using LibraryManagement.Application.DTOs.TaiKhoan;
namespace LibraryManagement.Application.Interfaces;
public interface ITaiKhoanRepository {
    /// <summary>Trả về tài khoản nếu đúng mật khẩu và đang hoạt động, đồng thời ghi nhận lần đăng nhập cuối.</summary>
    Task<TaiKhoanDto?> XacThucAsync(string tenDangNhap, string matKhau);
    Task<TaiKhoanDto?> GetByIdAsync(int maTK);
    Task<TaiKhoanDto?> GetByUsernameAsync(string tenDangNhap);
    Task<IEnumerable<TaiKhoanDto>> GetAllAsync();
    Task<IEnumerable<LookupItemDto>> GetVaiTrosAsync();
    Task<int> CreateAsync(CreateTaiKhoanDto dto);
    Task UpdateAsync(UpdateTaiKhoanDto dto);
    Task ChangePasswordAsync(int maTK, string matKhauMoi);
    Task SetTrangThaiAsync(int maTK, byte trangThai);
}
