namespace LibraryManagement.Application.Interfaces;
public interface IEmailService {
    Task SendNhacNhoHanTraAsync(string toEmail, string tenDocGia, IEnumerable<string> danhSachSach, DateOnly ngayHanTra, int soNgayConLai);
    Task SendNhacNhoQuaHanAsync(string toEmail, string tenDocGia, IEnumerable<string> danhSachSach, DateOnly ngayHanTra, int soNgayTre, decimal tienPhat);
}
