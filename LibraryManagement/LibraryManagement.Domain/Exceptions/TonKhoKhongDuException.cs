namespace LibraryManagement.Domain.Exceptions;
public class TonKhoKhongDuException(string tenSach)
    : Exception($"Sách '{tenSach}' không đủ số lượng tồn kho.");
