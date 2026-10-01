# Kiến Trúc Hệ Thống — Quản Lý Thư Viện THCS Thanh Tuân

**Phiên bản**: 1.0 | **Ngày**: 01/10/2026 | **Công nghệ**: ASP.NET Core 8 MVC

---

## 1. Tổng Quan Kiến Trúc

Hệ thống áp dụng **Clean Architecture** (còn gọi là Onion Architecture) với 4 lớp độc lập, giúp tách biệt logic nghiệp vụ khỏi hạ tầng kỹ thuật và UI.

```
┌─────────────────────────────────────────────────────────────┐
│                  LibraryManagement.Web                       │
│         (Controllers, Views, Middlewares, Program.cs)        │
│                  ASP.NET Core 8 MVC                          │
└─────────────────────┬───────────────────────────────────────┘
                      │  phụ thuộc vào
┌─────────────────────▼───────────────────────────────────────┐
│              LibraryManagement.Application                   │
│         (Interfaces, DTOs, Use Cases, Validators)            │
│              KHÔNG phụ thuộc vào Infrastructure             │
└────────┬────────────────────────────────┬────────────────────┘
         │  định nghĩa contracts          │  định nghĩa contracts
┌────────▼────────────┐       ┌───────────▼──────────────────┐
│  LibraryManagement  │       │  LibraryManagement           │
│      .Domain        │       │      .Infrastructure         │
│  (Entities,         │       │  (Repositories, Services,    │
│   Exceptions,       │       │   AppDbContext, Jobs)        │
│   Domain Logic)     │       │  implement Application       │
│  KHÔNG phụ thuộc    │       │  Interfaces                  │
│  vào layer khác     │       │                              │
└─────────────────────┘       └──────────────────────────────┘
```

### Dependency Rule (Quy tắc phụ thuộc)

- **Domain**: lõi trung tâm, không phụ thuộc bất kỳ layer nào
- **Application**: phụ thuộc Domain; định nghĩa Interfaces, không biết implementation
- **Infrastructure**: phụ thuộc Application + Domain; implement Interfaces bằng EF Core, MailKit, QRCoder
- **Web**: phụ thuộc Application (qua DI); không gọi trực tiếp Infrastructure

Dependency Injection (DI Container của ASP.NET Core) nối Application Interfaces với Infrastructure Implementations tại `Program.cs`.

---

## 2. Mô Tả Từng Layer

### 2.1 Domain Layer — `LibraryManagement.Domain`

**Vai trò**: Chứa toàn bộ thực thể nghiệp vụ và domain logic thuần túy.

| Thành phần | Mô tả |
|------------|-------|
| `Entities/Sach.cs` | Thực thể sách: MaSach, TenSach, SoLuongTon, MaQR,... |
| `Entities/DocGia.cs` | Thực thể độc giả: MaDocGia, HoTen, Lop, Email,... |
| `Entities/PhieuMuon.cs` | Phiếu mượn: MaPhieuMuon, NgayMuon, NgayHanTra, TrangThai |
| `Entities/CTPhieuMuon.cs` | Chi tiết phiếu mượn (1 phiếu — nhiều sách) |
| `Entities/GiaHan.cs` | Lịch sử gia hạn: LanGiaHan, HanTraCu, HanTraMoi |
| `Entities/PhieuTra.cs` | Phiếu trả: SoNgayTreHan, TienPhat, TrangThaiSach |
| `Entities/TaiKhoan.cs` | Tài khoản đăng nhập: TenDangNhap, MatKhau (BCrypt) |
| `Entities/CauHinhHeThong.cs` | Cấu hình động: SoNgayMuonToiDa, SoLanGiaHanToiDa,... |
| `Exceptions/TonKhoKhongDuException.cs` | Ném khi số lượng tồn không đủ để mượn |
| `Exceptions/GioiHanMuonException.cs` | Ném khi độc giả vượt giới hạn số sách đang mượn |
| `Exceptions/QuaHanGiaHanException.cs` | Ném khi đã vượt số lần gia hạn cho phép |

### 2.2 Application Layer — `LibraryManagement.Application`

**Vai trò**: Định nghĩa các contract (Interfaces) và Data Transfer Objects. Không chứa code hạ tầng.

**Interfaces (9 interface)**:

| Interface | Mục đích |
|-----------|----------|
| `ISachRepository` | CRUD sách, tìm kiếm theo tiêu chí |
| `IDocGiaRepository` | CRUD độc giả, kiểm tra trạng thái |
| `IPhieuMuonRepository` | Lập phiếu mượn, truy vấn phiếu theo độc giả |
| `IGiaHanRepository` | Gia hạn phiếu mượn, lịch sử gia hạn |
| `IPhieuTraRepository` | Trả sách, tính tiền phạt |
| `ITaiKhoanRepository` | Xác thực, quản lý tài khoản |
| `ICauHinhRepository` | Đọc/ghi cấu hình hệ thống |
| `IEmailService` | Gửi email nhắc nhở hạn trả |
| `IQrService` | Tạo và đọc QR Code sách |
| `IExportService` | Xuất báo cáo Excel |

**DTOs (35 DTOs)**: Các class transfer data giữa Controller và Repository, tránh expose Entity trực tiếp.

### 2.3 Infrastructure Layer — `LibraryManagement.Infrastructure`

**Vai trò**: Implement các Interface của Application bằng công nghệ cụ thể.

| Thành phần | Công nghệ |
|------------|-----------|
| `AppDbContext.cs` | EF Core 8 DbContext, cấu hình Fluent API |
| `SachRepository.cs` | EF Core + ADO.NET (gọi Stored Procedure) |
| `PhieuMuonRepository.cs` | ADO.NET, gọi `sp_LapPhieuMuon` qua SqlCommand |
| `PhieuTraRepository.cs` | ADO.NET, gọi `sp_TraSach` qua SqlCommand |
| `GiaHanRepository.cs` | ADO.NET, gọi `sp_GiaHanPhieuMuon` |
| `EmailService.cs` | MailKit/MimeKit, SMTP với SSL/TLS |
| `QrService.cs` | QRCoder — tạo PNG từ MaSach |
| `ExportService.cs` | ClosedXML — tạo file `.xlsx` |
| `NhacNhoHanTraJob.cs` | Hangfire RecurringJob, chạy lúc 7AM hàng ngày |

### 2.4 Web Layer — `LibraryManagement.Web`

**Vai trò**: Giao diện MVC — nhận HTTP request, gọi Application qua DI, trả View.

| Thành phần | Chi tiết |
|------------|----------|
| 12 Controllers | Login, Home, Sach, DocGia, DanhMuc, MuonSach, GiaHan, TraSach, TimKiem, ThongKe, TaiKhoan, CauHinh |
| 41 Views | `.cshtml` + Bootstrap 5 + Bootstrap Icons, sidebar xanh lá |
| Middleware | Cookie Authentication, Session, Hangfire Dashboard |
| `Program.cs` | Đăng ký DI, cấu hình pipeline, Hangfire, EF Core |

---

## 3. Technology Stack

| Hạng mục | Công nghệ | Phiên bản |
|----------|-----------|-----------|
| Framework | ASP.NET Core MVC | 8.0 |
| ORM | Entity Framework Core | 8.0.13 |
| Database | SQL Server | 2019+ |
| UI Framework | Bootstrap | 5.x |
| Icon | Bootstrap Icons | 1.x |
| Password Hashing | BCrypt.Net-Next | 4.0.3 |
| Email | MailKit | 4.7.1 |
| QR Code | QRCoder | 1.6.0 |
| Export Excel | ClosedXML | 0.104.2 |
| Background Jobs | Hangfire + Hangfire.SqlServer | 1.8.20 |
| Authentication | ASP.NET Core Cookie Auth | built-in |

---

## 4. Authentication Flow (Cookie-based)

```
[Browser]                [LoginController]          [TaiKhoanRepository]
    │                           │                           │
    │── POST /Login ────────────►│                           │
    │   {tenDangNhap, matKhau}  │── KiemTraDangNhap() ─────►│
    │                           │                           │── SELECT TaiKhoan
    │                           │                           │   WHERE TenDangNhap=?
    │                           │◄── TaiKhoanDTO ───────────│
    │                           │                           │
    │                           │── BCrypt.Verify(          │
    │                           │     matKhau, hash)        │
    │                           │                           │
    │                           │── SignInAsync()            │
    │                           │   (tạo Auth Cookie)       │
    │◄── 302 Redirect /Home ────│                           │
    │   Set-Cookie: .AspNet...  │                           │
    │                           │                           │
    │── GET /Sach ──────────────►│                           │
    │   Cookie: .AspNet...      │── [Authorize] check       │
    │                           │   cookie valid → OK       │
    │◄── 200 Views/Sach ────────│                           │
```

**Phân quyền**: Role-based qua `[Authorize(Roles = "QuanLy")]` hoặc `[Authorize(Roles = "ThuThu")]` trên Controller/Action.

---

## 5. Hangfire Email Job Flow

```
[Startup / Program.cs]
    │── RecurringJob.AddOrUpdate(
    │     "nhac-nho-han-tra",
    │     () => job.Execute(),
    │     Cron.Daily(7, 0))          ← 7:00 AM hàng ngày
    │
[7:00 AM — NhacNhoHanTraJob.Execute()]
    │
    │── sp_LayPhieuMuonSapDenHan()   ← lấy phiếu sắp đến hạn (2 ngày tới)
    │
    │── foreach phiếu:
    │     │── Lấy Email độc giả
    │     │── EmailService.SendAsync(
    │     │     to: docGia.Email,
    │     │     subject: "Nhắc nhở hạn trả sách",
    │     │     body: HTML template + TenSach + NgayHanTra)
    │     └── Ghi log kết quả
    │
    └── Hangfire Dashboard ghi lịch sử job
```

---

## 6. QR Code Generation Flow

```
[SachController.TaoQrCode(maSach)]
    │
    │── IQrService.TaoQrCode(maSach)
    │       │── QRCodeGenerator.CreateQrCode(
    │       │     data: maSach,
    │       │     eccLevel: L)
    │       │── PngByteQRCode(qrCode).GetGraphic(20)
    │       └── return byte[] (PNG)
    │
    │── Lưu đường dẫn PNG vào Sach.MaQR
    │── ISachRepository.CapNhatQrCode(maSach, duongDan)
    │
    └── Response: FileResult (image/png)

[DocGia dùng camera scan QR]
    │── POST /MuonSach/ScanQR
    │── Đọc nội dung QR = MaSach
    └── Redirect đến trang lập phiếu mượn với MaSach đã điền sẵn
```

---

## 7. Luồng Dữ Liệu Mượn Sách (End-to-End)

```
[ThuThu nhập thông tin mượn]
         │
[MuonSachController.LapPhieu(dto)]
         │
[IPhieuMuonRepository.LapPhieu(dto)]
         │
[ADO.NET SqlCommand → sp_LapPhieuMuon]
         │
    ┌────▼─────────────────────────────────────────────┐
    │  BEGIN TRANSACTION                               │
    │  1. Kiểm tra SoLuongTon >= SoLuongMuon           │
    │     → Không đủ: throw TonKhoKhongDuException     │
    │  2. Đếm sách đang mượn của DocGia                │
    │     → Vượt giới hạn: throw GioiHanMuonException  │
    │  3. INSERT INTO PhieuMuon                        │
    │  4. INSERT INTO CTPhieuMuon (1..N sách)          │
    │  5. UPDATE Sach SET SoLuongTon -= SoLuongMuon    │
    │  COMMIT TRANSACTION                              │
    └──────────────────────────────────────────────────┘
         │
[View hiển thị phiếu mượn thành công]
```
