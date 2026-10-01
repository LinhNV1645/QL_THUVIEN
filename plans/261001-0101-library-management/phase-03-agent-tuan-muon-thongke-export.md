# Phase 03 — Agent Tuân: Mượn + Gia hạn + Tìm kiếm + Thống kê + Export

**Thành viên**: Tạ Văn Tuân (B22DTCN039)  
**Status**: ⬜ Pending — chờ Phase 01 xong  
**Parallel**: Chạy đồng thời Phase 02 và 04  
**Ước tính**: 4–5 giờ

---

## File Ownership (KHÔNG đụng file ngoài danh sách này)

```
Domain/Entities/PhieuMuon.cs
Domain/Entities/CTPhieuMuon.cs
Domain/Entities/GiaHan.cs

Application/UseCases/MuonSach/
Application/UseCases/GiaHan/
Application/UseCases/TimKiem/
Application/UseCases/ThongKe/
Application/DTOs/MuonSach/
Application/DTOs/ThongKe/

Infrastructure/Repositories/PhieuMuonRepository.cs
Infrastructure/Repositories/GiaHanRepository.cs
Infrastructure/Services/ExportService.cs

Web/Controllers/MuonSachController.cs
Web/Controllers/GiaHanController.cs
Web/Controllers/TimKiemController.cs
Web/Controllers/ThongKeController.cs
Web/ViewModels/MuonSach/
Web/ViewModels/ThongKe/
Web/Views/MuonSach/
Web/Views/GiaHan/
Web/Views/TimKiem/
Web/Views/ThongKe/
wwwroot/exports/     ← thư mục lưu file Excel/PDF tạm
```

---

## Todo List

### Repositories
- [ ] `PhieuMuonRepository.cs` — implement `IPhieuMuonRepository`
  - `LapPhieuMuonAsync(dto)` → gọi `sp_LapPhieuMuon`
  - `GetByIdAsync`, `GetByDocGiaAsync`
  - `GetDangMuonAsync` — lấy phiếu chưa trả
  - `GetQuaHanAsync` → từ `vw_PhieuMuonQuaHan`
  - `GetLichSuAsync(maDocGia)` → từ `vw_LichSuMuonTra`
- [ ] `GiaHanRepository.cs` — implement `IGiaHanRepository`
  - `GiaHanAsync(maPhieu, nhanVien)` → gọi `sp_GiaHanPhieuMuon`
  - `GetByPhieuMuonAsync`

### Use Cases — Mượn sách
- [ ] `LapPhieuMuonUseCase.cs`
  - Validate: kiểm tra độc giả tồn tại, sách tồn kho
  - Gọi `sp_LapPhieuMuon` qua Repository
  - Return: MaPhieuMuon mới
- [ ] `GetPhieuMuonListUseCase.cs` — filter + phân trang
- [ ] `GetPhieuMuonDetailUseCase.cs`
- [ ] `GetPhieuQuaHanUseCase.cs` → `vw_PhieuMuonQuaHan`

### Use Cases — Gia hạn
- [ ] `GiaHanUseCase.cs` → gọi `sp_GiaHanPhieuMuon`
- [ ] `GetGiaHanHistoryUseCase.cs`

### Use Cases — Tìm kiếm
- [ ] `TimKiemSachUseCase.cs` → gọi `sp_TimKiemSach` (phân trang)
- [ ] `TimKiemDocGiaUseCase.cs` — tìm theo tên/lớp

### Use Cases — Thống kê
- [ ] `ThongKeSachMuonNhieuUseCase.cs` → `sp_ThongKeSachMuonNhieu`
- [ ] `ThongKeDocGiaMuonNhieuUseCase.cs` → `sp_ThongKeDocGiaMuonNhieu`
- [ ] `ThongKeTongQuanUseCase.cs` → `sp_Dashboard`

### Export Service
- [ ] `ExportService.cs` implement `IExportService`
  - `ExportSachToExcel(filters)` → ClosedXML → `.xlsx`
  - `ExportLichSuMuonToExcel(maDocGia?, thang, nam)` → `.xlsx`
  - `ExportThongKeToExcel(thang, nam)` → `.xlsx`
  - `ExportSachToPdf(filters)` → DinkToPdf → `.pdf`
  - Mỗi method trả `byte[]` (stream download, không lưu file)

### Controllers & Views — Mượn sách
- [ ] `MuonSachController.cs`
  - `GET  /MuonSach`              → danh sách phiếu + filter
  - `GET  /MuonSach/Create`       → form lập phiếu
  - `POST /MuonSach/Create`       → gọi sp_LapPhieuMuon
  - `GET  /MuonSach/Detail/{id}`  → chi tiết phiếu
  - `GET  /MuonSach/QuaHan`       → danh sách quá hạn
  - `GET  /MuonSach/LichSu/{maDocGia}` → lịch sử 1 độc giả
- [ ] Views: `Index.cshtml`, `Create.cshtml`, `Detail.cshtml`, `QuaHan.cshtml`
  - **Create.cshtml**: có ô quét QR (camera API) + JS tự điền thông tin sách
  - QR scan: gọi `GET /api/qr/scan?code=xxx` (từ Phase 02) → điền form

### QR Scan UI (JavaScript)
```javascript
// Dùng thư viện html5-qrcode (CDN)
const html5Qr = new Html5Qrcode("qr-reader");
html5Qr.start(cameraId, config, (decodedText) => {
    fetch(`/api/qr/scan?code=${decodedText}`)
        .then(r => r.json())
        .then(sach => {
            // Tự điền MaSach và TenSach vào form
            document.getElementById('maSach').value = sach.maSach;
            document.getElementById('tenSach').value = sach.tenSach;
        });
});
```

### Controllers & Views — Gia hạn
- [ ] `GiaHanController.cs`
  - `POST /GiaHan/Create` → gọi sp_GiaHanPhieuMuon
  - Nút Gia hạn hiển thị trong trang Detail phiếu mượn
- [ ] View: modal confirm gia hạn (hiển thị hạn cũ → hạn mới)

### Controllers & Views — Tìm kiếm
- [ ] `TimKiemController.cs`
  - `GET /TimKiem?q=&theloai=&tacgia=` → gọi `sp_TimKiemSach`
  - Kết quả: hiển thị sách + số lượng tồn + nút lập phiếu mượn ngay
- [ ] View: `Index.cshtml` — search bar nổi bật + bảng kết quả

### Controllers & Views — Thống kê
- [ ] `ThongKeController.cs`
  - `GET /ThongKe`            → dashboard tổng quan
  - `GET /ThongKe/SachMuonNhieu?thang=&nam=`
  - `GET /ThongKe/DocGiaMuonNhieu?thang=&nam=`
  - `GET /ThongKe/ExportExcel?type=sach|docgia|lichsu`
  - `GET /ThongKe/ExportPdf?type=sach|docgia`
- [ ] Views: `Index.cshtml` (chart đơn giản bằng Chart.js CDN), `SachMuonNhieu.cshtml`

---

## ViewModels

```csharp
public record LapPhieuMuonViewModel(
    int MaDocGia, string TenDocGia,
    List<ChonSachItem> DanhSachSach  // [{MaSach, TenSach, SoLuong}]
);

public record ThongKeThangViewModel(
    int Thang, int Nam,
    IEnumerable<SachMuonNhieuDto> SachTop10,
    IEnumerable<DocGiaMuonNhieuDto> DocGiaTop10
);
```

---

## Lưu ý quan trọng

- **Không tự implement tồn kho logic** — tất cả qua SP, không viết lại trong C#
- **Export**: trả `FileResult` trực tiếp, không lưu file lên server
- **QR scan UI**: dùng `html5-qrcode` từ CDN, không cài npm
- Phiếu quá hạn: đọc từ `vw_PhieuMuonQuaHan`, không tự tính trong C#

---

## Success Criteria

- [ ] Lập phiếu mượn: kiểm tra tồn kho, tạo phiếu, trừ số lượng (qua SP)
- [ ] Quét QR bằng camera → tự điền thông tin sách vào form
- [ ] Gia hạn: validate số lần, cập nhật hạn trả
- [ ] Tìm kiếm đa tiêu chí, phân trang
- [ ] Thống kê top 10 sách/độc giả theo tháng
- [ ] Export Excel + PDF tải về được
- [ ] Chart.js hiển thị top sách mượn
