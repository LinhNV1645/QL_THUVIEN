# Contract: Application Interfaces

> **BẮT BUỘC**: Infrastructure implements đúng interface này. Không thêm method ngoài danh sách.

---

## ISachRepository
```csharp
public interface ISachRepository {
    Task<PaginatedResult<SachDto>> GetAllAsync(SachFilterDto filter);
    Task<SachDto?> GetByIdAsync(int maSach);
    Task<SachDto?> GetByQrAsync(string maQR);
    Task<int> CreateAsync(CreateSachDto dto);       // trả MaSach mới
    Task UpdateAsync(UpdateSachDto dto);
    Task SoftDeleteAsync(int maSach);               // TrangThai = 0
    Task<int> GetTonKhoAsync(int maSach);
}
```

## IDocGiaRepository
```csharp
public interface IDocGiaRepository {
    Task<PaginatedResult<DocGiaDto>> GetAllAsync(string? keyword, string? lop, int page, int pageSize);
    Task<DocGiaDto?> GetByIdAsync(int maDocGia);
    Task<int> CreateAsync(CreateDocGiaDto dto);
    Task UpdateAsync(UpdateDocGiaDto dto);
    Task SetTrangThaiAsync(int maDocGia, byte trangThai);
    Task<IEnumerable<LichSuMuonDto>> GetLichSuAsync(int maDocGia);
}
```

## IPhieuMuonRepository
```csharp
public interface IPhieuMuonRepository {
    Task<int> LapPhieuMuonAsync(LapPhieuMuonDto dto);  // gọi sp_LapPhieuMuon
    Task<PhieuMuonDetailDto?> GetByIdAsync(int maPhieu);
    Task<PaginatedResult<PhieuMuonDto>> GetAllAsync(PhieuMuonFilterDto filter);
    Task<IEnumerable<PhieuMuonQuaHanDto>> GetQuaHanAsync();
    Task<IEnumerable<PhieuMuonDto>> GetDangMuonByDocGiaAsync(int maDocGia);
}
```

## IGiaHanRepository
```csharp
public interface IGiaHanRepository {
    Task<DateOnly> GiaHanAsync(int maPhieuMuon, int nhanVienDuyet, string? ghiChu);
    // gọi sp_GiaHanPhieuMuon, trả HanTraMoi
    Task<IEnumerable<GiaHanDto>> GetByPhieuMuonAsync(int maPhieuMuon);
}
```

## IPhieuTraRepository
```csharp
public interface IPhieuTraRepository {
    Task<TraSachResultDto> TraSachAsync(TraSachDto dto); // gọi sp_TraSach
    Task<PhieuTraDto?> GetByIdAsync(int maPhieuTra);
    Task<PhieuTraDto?> GetByPhieuMuonAsync(int maPhieuMuon);
    Task<IEnumerable<PhieuTraDto>> GetChuaThuPhatAsync();
    Task ThuPhatAsync(int maPhieuTra);
}
```

## ITaiKhoanRepository
```csharp
public interface ITaiKhoanRepository {
    Task<TaiKhoanDto?> GetByUsernameAsync(string tenDangNhap);
    Task<IEnumerable<TaiKhoanDto>> GetAllAsync();
    Task<int> CreateAsync(CreateTaiKhoanDto dto);
    Task UpdateAsync(UpdateTaiKhoanDto dto);
    Task ChangePasswordAsync(int maTK, string matKhauMoi);
    Task SetTrangThaiAsync(int maTK, byte trangThai);
}
```

## ICauHinhRepository
```csharp
public interface ICauHinhRepository {
    Task<string?> GetValueAsync(string tenCauHinh);
    Task<IEnumerable<CauHinhDto>> GetAllAsync();
    Task UpdateAsync(string tenCauHinh, string giaTri);
}
```

## IEmailService
```csharp
public interface IEmailService {
    Task SendNhacNhoHanTraAsync(
        string toEmail,
        string tenDocGia,
        string danhSachSach,
        DateOnly ngayHanTra,
        int soNgayConLai);
}
```

## IExportService
```csharp
public interface IExportService {
    Task<byte[]> ExportSachToExcelAsync(SachFilterDto? filter = null);
    Task<byte[]> ExportLichSuMuonToExcelAsync(int? maDocGia, int thang, int nam);
    Task<byte[]> ExportThongKeToExcelAsync(int thang, int nam);
    Task<byte[]> ExportSachToPdfAsync(SachFilterDto? filter = null);
    Task<byte[]> ExportThongKeToPdfAsync(int thang, int nam);
}
```

## IQrService
```csharp
public interface IQrService {
    string GenerateQrImagePath(string maQR);  // trả đường dẫn file PNG
    byte[] GenerateQrBytes(string maQR);      // trả raw bytes để stream
}
```

## IUnitOfWork
```csharp
public interface IUnitOfWork : IDisposable {
    ISachRepository Sachs { get; }
    IDocGiaRepository DocGias { get; }
    IPhieuMuonRepository PhieuMuons { get; }
    IGiaHanRepository GiaHans { get; }
    IPhieuTraRepository PhieuTras { get; }
    ITaiKhoanRepository TaiKhoans { get; }
    ICauHinhRepository CauHinhs { get; }
    Task<int> SaveChangesAsync();
}
```

---

## DTOs dùng chung (Application layer)

```csharp
// Phân trang
public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
) {
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;
}

// Kết quả trả sách (từ sp_TraSach)
public record TraSachResultDto(int MaPhieuTra, decimal TienPhat, int SoNgayTreHan);

// Filter sách
public record SachFilterDto(
    string? TuKhoa, int? MaTheLoai, int? MaTacGia, int? MaNXB,
    bool ChiConTon = false, int Page = 1, int PageSize = 20
);

// Lập phiếu mượn
public record LapPhieuMuonDto(
    int MaDocGia,
    List<SachMuonItem> DanhSachSach,
    int NhanVienLap
);
public record SachMuonItem(int MaSach, int SoLuong);

// Trả sách
public record TraSachDto(
    int MaPhieuMuon,
    byte TrangThaiSach,  // 1=OK, 2=Hư, 3=Mất
    int NhanVienThu,
    string? GhiChu
);
```
