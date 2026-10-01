namespace LibraryManagement.Application.Interfaces;
public interface IEmailService {
    Task SendNhacNhoHanTraAsync(string toEmail, string tenDocGia, string danhSachSach, DateOnly ngayHanTra, int soNgayConLai);
}
