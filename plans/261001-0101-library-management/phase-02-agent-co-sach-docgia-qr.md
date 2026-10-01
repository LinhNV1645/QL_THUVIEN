# Phase 02 — Agent Cơ: Sách + Độc giả + QR

**Thành viên**: Bùi Văn Cơ (B22DTCN001)  
**Status**: ⬜ Pending — chờ Phase 01 xong  
**Parallel**: Chạy đồng thời Phase 03 và 04  
**Ước tính**: 3–4 giờ

---

## File Ownership (KHÔNG đụng file ngoài danh sách này)

```
Domain/Entities/Sach.cs
Domain/Entities/TheLoai.cs
Domain/Entities/TacGia.cs
Domain/Entities/NhaXuatBan.cs
Domain/Entities/DocGia.cs

Application/UseCases/Sach/
Application/UseCases/DocGia/
Application/DTOs/Sach/
Application/DTOs/DocGia/

Infrastructure/Repositories/SachRepository.cs
Infrastructure/Repositories/DocGiaRepository.cs
Infrastructure/Services/QrService.cs

Web/Controllers/SachController.cs
Web/Controllers/DocGiaController.cs
Web/ViewModels/Sach/
Web/ViewModels/DocGia/
Web/Views/Sach/
Web/Views/DocGia/
wwwroot/qr/          ← thư mục lưu ảnh QR
```

---

## Todo List

### Repositories
- [ ] `SachRepository.cs` — implement `ISachRepository`
  - `GetAllAsync(filter, page)` → gọi `sp_TimKiemSach`
  - `GetByIdAsync(id)`
  - `GetByQrAsync(maQR)`
  - `CreateAsync`, `UpdateAsync`, `SoftDeleteAsync`
  - `GetTonKhoAsync(maSach)`
- [ ] `DocGiaRepository.cs` — implement `IDocGiaRepository`
  - CRUD + tìm kiếm theo tên/lớp

### Use Cases — Sách
- [ ] `GetSachListUseCase.cs` — phân trang + filter
- [ ] `GetSachByIdUseCase.cs`
- [ ] `CreateSachUseCase.cs` — tạo + sinh QR tự động
- [ ] `UpdateSachUseCase.cs`
- [ ] `DeleteSachUseCase.cs` — soft delete, check ràng buộc
- [ ] `TimKiemSachUseCase.cs` → gọi `sp_TimKiemSach`

### Use Cases — DocGia
- [ ] `GetDocGiaListUseCase.cs`
- [ ] `CreateDocGiaUseCase.cs`
- [ ] `UpdateDocGiaUseCase.cs`
- [ ] `LockDocGiaUseCase.cs` — khóa tài khoản độc giả

### QR Service
- [ ] `QrService.cs` implement `IQrService`
  - `GenerateQr(maSach)` → tạo ảnh PNG vào `wwwroot/qr/{maSach}.png`
  - Dùng thư viện `QRCoder`
  - Nội dung QR = `MaQR` của sách (vd: `QR001`)
- [ ] API endpoint `GET /api/qr/scan?code=QR001` → trả JSON thông tin sách
  - Dùng cho: form mượn/trả tự động điền khi quét QR

### Controllers & Views — Sách
- [ ] `SachController.cs`
  - `GET  /Sach`           → danh sách + filter + phân trang
  - `GET  /Sach/Create`    → form thêm
  - `POST /Sach/Create`    → lưu + tự sinh QR
  - `GET  /Sach/Edit/{id}` → form sửa
  - `POST /Sach/Edit/{id}` → cập nhật
  - `POST /Sach/Delete/{id}` → soft delete
  - `GET  /Sach/Detail/{id}` → chi tiết + hiển thị QR
  - `GET  /Sach/QrImage/{id}` → trả file PNG mã QR
- [ ] Views: `Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Detail.cshtml`
  - Index: bảng + filter theo tên/thể loại/tác giả/NXB + badge tồn kho
  - Detail: hiển thị ảnh QR + nút in QR
  - Create/Edit: dropdown TheLoai, TacGia, NXB

### Controllers & Views — Độc giả
- [ ] `DocGiaController.cs`
  - CRUD đầy đủ + tìm kiếm theo tên/lớp
  - `GET /DocGia/LichSu/{id}` → lịch sử mượn từ `vw_LichSuMuonTra`
- [ ] Views: `Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `LichSu.cshtml`

### Lookup — TheLoai, TacGia, NXB
- [ ] `DanhMucController.cs` — CRUD cho 3 bảng lookup
- [ ] Views đơn giản (modal Bootstrap trong trang Sách)

---

## ViewModels

```csharp
// SachListViewModel
public record SachListViewModel(
    IEnumerable<SachDto> Items, int TotalCount, int Page, int PageSize,
    string? TuKhoa, int? MaTheLoai, int? MaTacGia, bool ChiConTon
);

// SachDetailViewModel
public record SachDetailViewModel(SachDto Sach, string QrImageUrl);

// DocGiaLichSuViewModel
public record DocGiaLichSuViewModel(DocGiaDto DocGia, IEnumerable<LichSuMuonDto> LichSu);
```

---

## API endpoint cho QR (dùng bởi Phase 03 — form mượn)

```csharp
// Trả về JSON để JS tự điền form
[HttpGet("/api/qr/scan")]
public async Task<IActionResult> Scan(string code)
{
    var sach = await _sachRepo.GetByQrAsync(code);
    if (sach == null) return NotFound();
    return Ok(new { sach.MaSach, sach.TenSach, sach.SoLuongTon });
}
```

> **Quan trọng**: Agent Tuân (Phase 03) sẽ gọi endpoint này từ JS khi quét QR trên form mượn.

---

## Success Criteria

- [ ] CRUD sách hoạt động đầy đủ
- [ ] Tìm kiếm theo tên/thể loại/tác giả/NXB/mã QR
- [ ] Mỗi sách có ảnh QR hiển thị và in được
- [ ] API `/api/qr/scan` trả đúng thông tin sách
- [ ] CRUD độc giả + xem lịch sử mượn
- [ ] Phân quyền: NhanVien+ mới được thêm/sửa/xóa
