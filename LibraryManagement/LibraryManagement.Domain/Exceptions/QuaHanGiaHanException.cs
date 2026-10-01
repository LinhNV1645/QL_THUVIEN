namespace LibraryManagement.Domain.Exceptions;
public class QuaHanGiaHanException(int soLanToiDa)
    : Exception($"Đã đạt số lần gia hạn tối đa ({soLanToiDa} lần).");
