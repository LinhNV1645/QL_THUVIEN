namespace LibraryManagement.Application.Interfaces;
public interface IQrService {
    string GenerateQrImagePath(string maQR);
    byte[] GenerateQrBytes(string maQR);
}
