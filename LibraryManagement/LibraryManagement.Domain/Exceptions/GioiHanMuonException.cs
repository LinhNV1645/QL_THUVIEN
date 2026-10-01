namespace LibraryManagement.Domain.Exceptions;
public class GioiHanMuonException(int soSachToiDa)
    : Exception($"Độc giả đã đạt giới hạn {soSachToiDa} sách mượn đồng thời.");
