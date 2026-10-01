# BÁO CÁO ĐỒ ÁN MÔN HỌC
## Hệ Thống Quản Lý Thư Viện THCS Thanh Tuân

---

**Học viện**: Học viện Công nghệ Bưu chính Viễn thông (PTIT)
**Môn học**: Lập trình ứng dụng Web / Công nghệ phần mềm
**Ngày hoàn thành**: 01/10/2026

---

## I. THÔNG TIN NHÓM

| STT | Họ và Tên | MSSV | Vai trò |
|-----|-----------|------|---------|
| 1 | Nguyễn Văn Linh | B22DTCN022 | Nhóm trưởng |
| 2 | Bùi Văn Cơ | B22DTCN001 | Thành viên |
| 3 | Tạ Văn Tuân | B22DTCN039 | Thành viên |

---

## II. GIỚI THIỆU BÀI TOÁN

### 2.1 Bối cảnh

Trường THCS Thanh Tuân hiện quản lý thư viện theo phương pháp thủ công: thủ thư ghi chép phiếu mượn bằng tay vào sổ, tra cứu sách mất nhiều thời gian, và không có hệ thống nhắc nhở khi sách sắp đến hạn trả. Điều này dẫn đến nhiều sách bị mất, thất lạc hoặc trả trễ mà không được xử lý kịp thời.

### 2.2 Vấn đề cần giải quyết

- **Quản lý kho sách**: Theo dõi số lượng tồn, vị trí kệ, tình trạng từng đầu sách
- **Quản lý mượn — trả**: Lập phiếu nhanh, kiểm soát giới hạn mượn, tính tiền phạt trễ hạn
- **Nhắc nhở tự động**: Gửi email khi sách sắp đến hạn trả, tránh trễ hạn
- **Tra cứu nhanh**: Tìm sách theo nhiều tiêu chí; quét mã QR để mượn sách nhanh
- **Thống kê — báo cáo**: Sách mượn nhiều, độc giả tích cực, xu hướng theo tháng
- **Phân quyền**: Quản lý (toàn quyền) và Thủ thư (nghiệp vụ hàng ngày)

### 2.3 Phạm vi hệ thống

Hệ thống phục vụ **nội bộ trường**, vận hành trên mạng LAN hoặc máy chủ nội bộ. Người dùng gồm Quản lý thư viện và Thủ thư — không có giao diện dành cho học sinh tự tra cứu trong phiên bản này.

---

## III. PHÂN TÍCH YÊU CẦU

### 3.1 Yêu cầu chức năng (Functional Requirements)

#### Nhóm Danh mục

| Mã | Chức năng | Mô tả |
|----|-----------|-------|
| F01 | Quản lý Sách | Thêm, sửa, xóa (ẩn), xem chi tiết sách; upload QR Code |
| F02 | Quản lý Thể loại | CRUD thể loại sách |
| F03 | Quản lý Tác giả | CRUD tác giả |
| F04 | Quản lý Nhà xuất bản | CRUD nhà xuất bản |
| F05 | Quản lý Độc giả | Thêm, sửa, xóa (vô hiệu hóa) hồ sơ độc giả |

#### Nhóm Nghiệp vụ

| Mã | Chức năng | Mô tả |
|----|-----------|-------|
| F06 | Mượn sách | Lập phiếu mượn; kiểm tra tồn kho và giới hạn mượn |
| F07 | Gia hạn | Gia hạn hạn trả; kiểm soát số lần gia hạn tối đa |
| F08 | Trả sách | Lập phiếu trả; tính tiền phạt theo số ngày trễ |
| F09 | Quét QR | Quét mã QR sách để điền tự động vào phiếu mượn |

#### Nhóm Tra cứu & Báo cáo

| Mã | Chức năng | Mô tả |
|----|-----------|-------|
| F10 | Tìm kiếm sách | Tìm theo tên, tác giả, thể loại, tình trạng; phân trang |
| F11 | Thống kê | Sách mượn nhiều, độc giả tích cực, biểu đồ theo tháng |
| F12 | Export Excel | Xuất báo cáo danh sách sách, lịch sử mượn ra file .xlsx |
| F13 | Dashboard | Tổng quan 7 chỉ số chính trên trang chủ |

#### Nhóm Hệ thống

| Mã | Chức năng | Mô tả |
|----|-----------|-------|
| F14 | Đăng nhập | Xác thực Cookie-based; phân quyền QuanLy / ThuThu |
| F15 | Quản lý Tài khoản | Tạo, sửa, vô hiệu hóa tài khoản; đổi mật khẩu |
| F16 | Cấu hình hệ thống | Chỉnh số ngày mượn, số lần gia hạn, tiền phạt, SMTP |
| F17 | Email nhắc nhở | Tự động gửi email 7AM hàng ngày cho sách sắp đến hạn |

### 3.2 Yêu cầu phi chức năng (Non-Functional Requirements)

| Hạng mục | Yêu cầu |
|----------|---------|
| **Bảo mật** | Mật khẩu lưu BCrypt hash; HTTPS khuyến nghị; phân quyền role-based |
| **Hiệu năng** | Trang danh sách tải < 2 giây với 1.000 bản ghi; phân trang 10–20 item/trang |
| **Tính sẵn sàng** | Hệ thống hoạt động trong giờ học; Hangfire job chạy 7AM không ảnh hưởng UI |
| **Khả năng bảo trì** | Cấu hình hệ thống thay đổi không cần deploy lại; code Clean Architecture dễ mở rộng |
| **Tính dùng được** | Giao diện Bootstrap 5 responsive; thao tác không quá 3 click cho nghiệp vụ thường ngày |

---

## IV. KIẾN TRÚC HỆ THỐNG

### 4.1 Tổng quan

Hệ thống xây dựng theo mô hình **Clean Architecture** 4 lớp, đảm bảo tách biệt hoàn toàn logic nghiệp vụ khỏi hạ tầng kỹ thuật:

```
Web (MVC) → Application (Interfaces/DTOs) ← Infrastructure (EF Core/MailKit/QRCoder)
                        ↑
                    Domain (Entities/Exceptions)
```

### 4.2 Luồng xử lý Request

```
HTTP Request
    → Controller (validate input)
        → Repository/Service Interface (Application layer)
            → Repository/Service Implementation (Infrastructure)
                → SQL Server (qua EF Core hoặc Stored Procedure)
            ← Trả DTO
        ← Trả DTO
    ← Controller gán vào ViewModel
← View render HTML
```

### 4.3 Công nghệ sử dụng

| Lớp | Công nghệ |
|-----|-----------|
| Frontend | ASP.NET Core MVC Razor Views, Bootstrap 5, Bootstrap Icons |
| Backend | ASP.NET Core 8, C# 12 |
| ORM | Entity Framework Core 8 (Code First) |
| ADO.NET | Gọi Stored Procedure hiệu năng cao |
| Database | SQL Server 2019+ (12 bảng, 8 SP, 5 Views, 2 Triggers) |
| Authentication | ASP.NET Core Cookie Authentication |
| Email | MailKit 4.7.1 (SMTP, SSL/TLS) |
| QR Code | QRCoder 1.6.0 |
| Export | ClosedXML 0.104.2 (file .xlsx) |
| Background Jobs | Hangfire 1.8.20 + Hangfire.SqlServer |
| Password | BCrypt.Net-Next 4.0.3 |

---

## V. MÔ TẢ CÁC CHỨC NĂNG CHÍNH

### 5.1 Mượn Sách

Thủ thư chọn độc giả, chọn sách (có thể quét QR hoặc tìm kiếm), nhập số lượng và ngày hạn trả. Hệ thống tự động:
- Kiểm tra tồn kho thực tế (qua Stored Procedure `sp_LapPhieuMuon`)
- Kiểm tra giới hạn số sách đang mượn của độc giả (lấy từ `CauHinhHeThong`)
- Trừ tồn kho và tạo phiếu mượn trong một transaction
- Hiển thị cảnh báo rõ ràng nếu không đủ điều kiện

### 5.2 Trả Sách

Thủ thư tra phiếu mượn của độc giả, chọn phiếu cần trả. Hệ thống tự động:
- Tính số ngày trễ = `MAX(0, NgàyTrả - NgàyHạnTrả)`
- Tính tiền phạt = `SoNgayTreHan × TienPhatMoiNgay` (lấy từ cấu hình)
- Lưu tình trạng sách (Tốt / Hư hỏng / Mất sách)
- Hoàn kho và cập nhật trạng thái phiếu trong một transaction

### 5.3 Gia Hạn

Tìm phiếu mượn cần gia hạn, nhập số ngày muốn gia hạn thêm. Hệ thống:
- Kiểm tra phiếu chưa được trả và chưa quá hạn gia hạn
- Kiểm tra số lần gia hạn chưa vượt giới hạn trong cấu hình
- Ghi nhận lịch sử gia hạn với ngày cũ, ngày mới, nhân viên duyệt

### 5.4 QR Code

Mỗi sách được gán một mã QR duy nhất chứa `MaSach`. Quy trình:
- **Tạo QR**: Quản lý vào trang chi tiết sách → nhấn "Tạo QR" → hệ thống sinh file PNG và lưu đường dẫn vào `Sach.MaQR`
- **Dùng QR**: Thủ thư mở trang lập phiếu mượn → nhấn "Quét QR" → camera đọc mã → `MaSach` tự điền vào form

### 5.5 Email Nhắc Nhở Tự Động

Hangfire `RecurringJob` chạy mỗi ngày lúc 7:00 AM:
1. Gọi `sp_LayPhieuMuonSapDenHan` để lấy danh sách phiếu sắp hết hạn trong 2 ngày tới
2. Với mỗi phiếu, gửi email HTML đến địa chỉ email của độc giả
3. Email chứa: tên sách, ngày hạn trả, hướng dẫn gia hạn nếu cần
4. Thủ thư có thể theo dõi lịch sử job qua Hangfire Dashboard (`/hangfire`)

### 5.6 Tìm Kiếm Đa Tiêu Chí

Người dùng có thể tìm kiếm sách theo nhiều tiêu chí đồng thời:
- Từ khóa trong tên sách
- Thể loại (dropdown)
- Tác giả (dropdown)
- Tình trạng (Còn sách / Hết sách / Ngừng lưu hành)

Kết quả phân trang, sử dụng CTE + ROW_NUMBER trong Stored Procedure `sp_TimKiemSach` để hiệu năng cao ngay cả với kho sách lớn.

### 5.7 Thống Kê và Export

**Thống kê**:
- Top 10 sách mượn nhiều nhất theo tháng/năm
- Top 10 độc giả mượn sách nhiều nhất theo tháng/năm
- Biểu đồ số lượt mượn theo từng tháng trong năm

**Export Excel**: Xuất file `.xlsx` gồm:
- Danh sách toàn bộ sách với đầy đủ thông tin
- Lịch sử mượn theo khoảng thời gian
- Danh sách phiếu chưa trả / quá hạn

### 5.8 Cấu Hình Hệ Thống

Quản lý có thể điều chỉnh các thông số hệ thống mà không cần thay đổi code:
- Số ngày mượn tối đa (mặc định 14 ngày)
- Số lần gia hạn tối đa (mặc định 2 lần)
- Số sách mượn cùng lúc tối đa (mặc định 3 cuốn)
- Tiền phạt mỗi ngày trễ (mặc định 1.000 VNĐ)
- Thông tin SMTP để gửi email

---

## VI. PHÂN CÔNG CÔNG VIỆC

### Nguyễn Văn Linh — B22DTCN022 (Nhóm trưởng)

**Phụ trách module**: Trả sách, Gia hạn, Cấu hình hệ thống, Email, Tài khoản

| Task | Chi tiết |
|------|----------|
| Domain Layer | Thiết kế toàn bộ 12 Entity, cấu hình quan hệ EF Core (Fluent API) |
| Trả sách | `TraSachController`, `IPhieuTraRepository`, `PhieuTraRepository`, Stored Procedure `sp_TraSach`, Views trả sách (form trả, tính tiền phạt, xác nhận) |
| Gia hạn | `GiaHanController`, `IGiaHanRepository`, `GiaHanRepository`, Stored Procedure `sp_GiaHanPhieuMuon`, Views gia hạn |
| Email | `IEmailService`, `EmailService` (MailKit), `NhacNhoHanTraJob` (Hangfire), Stored Procedure `sp_LayPhieuMuonSapDenHan`, cấu hình RecurringJob |
| Tài khoản | `TaiKhoanController`, `ITaiKhoanRepository`, `TaiKhoanRepository`, Views quản lý tài khoản, đổi mật khẩu |
| Cấu hình | `CauHinhController`, `ICauHinhRepository`, `CauHinhRepository`, Views cài đặt hệ thống |
| Hệ thống | `Program.cs` (DI, Hangfire, Cookie Auth), `AppDbContext.cs`, thiết kế toàn bộ kiến trúc Clean Architecture |

### Bùi Văn Cơ — B22DTCN001

**Phụ trách module**: Sách, Độc giả, QR Code, Danh mục (Thể loại / Tác giả / NXB)

| Task | Chi tiết |
|------|----------|
| Sách | `SachController`, `ISachRepository`, `SachRepository`, Views sách (danh sách, chi tiết, thêm/sửa) |
| QR Code | `IQrService`, `QrService` (QRCoder), tích hợp nút Tạo QR vào trang sách, luồng quét QR trong phiếu mượn |
| Độc giả | `DocGiaController`, `IDocGiaRepository`, `DocGiaRepository`, Views quản lý độc giả (thêm/sửa/tìm kiếm) |
| Danh mục | `DanhMucController`, Views CRUD thể loại / tác giả / nhà xuất bản |
| Database | Thiết kế bảng Sach, TheLoai, TacGia, NhaXuatBan, DocGia; seed data danh mục; indexes cho tìm kiếm sách |
| UI Component | Thiết kế sidebar, layout chung, CSS custom (màu xanh lá thư viện) |

### Tạ Văn Tuân — B22DTCN039

**Phụ trách module**: Mượn sách, Tìm kiếm, Thống kê, Export Excel

| Task | Chi tiết |
|------|----------|
| Mượn sách | `MuonSachController`, `IPhieuMuonRepository`, `PhieuMuonRepository`, Stored Procedure `sp_LapPhieuMuon`, Views lập phiếu mượn, danh sách phiếu đang mượn |
| Tìm kiếm | `TimKiemController`, Stored Procedure `sp_TimKiemSach` (CTE + ROW_NUMBER + phân trang), Views tìm kiếm với filter động |
| Thống kê | `ThongKeController`, SP `sp_ThongKeSachMuonNhieu`, `sp_ThongKeDocGiaMuonNhieu`, SP `sp_Dashboard`, Views biểu đồ thống kê (Chart.js) |
| Export | `IExportService`, `ExportService` (ClosedXML), export sách/phiếu mượn ra `.xlsx` |
| Database | Thiết kế bảng PhieuMuon, CTPhieuMuon; Views `v_PhieuMuonDayDu`, `v_PhieuMuonQuaHan`, `v_ThongKeMuonTheoThang`; Triggers kiểm soát |
| SQL | Viết file `01-db-schema.sql`, `02-stored-procedures.sql`, `03-views-triggers.sql` |

---

## VII. HƯỚNG DẪN CÀI ĐẶT VÀ CHẠY

### 7.1 Yêu cầu môi trường

- .NET SDK 8.0 trở lên
- SQL Server 2019 (hoặc SQL Server Express)
- Visual Studio 2022 hoặc VS Code + C# Extension

### 7.2 Cài đặt Database

```sql
-- Bước 1: Tạo database
CREATE DATABASE LibraryManagement;

-- Bước 2: Chạy lần lượt 3 file SQL
-- (1) Tạo schema + seed data
USE LibraryManagement;
-- Chạy nội dung file: sql/01-db-schema.sql

-- (2) Tạo Stored Procedures
-- Chạy nội dung file: sql/02-stored-procedures.sql

-- (3) Tạo Views + Triggers
-- Chạy nội dung file: sql/03-views-triggers.sql
```

### 7.3 Cấu hình Connection String

Mở file `LibraryManagement.Web/appsettings.json`, cập nhật:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=LibraryManagement;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "SenderName": "Thư viện THCS Thanh Tuân"
  }
}
```

### 7.4 Chạy ứng dụng

```bash
cd LibraryManagement/LibraryManagement.Web
dotnet restore
dotnet run
```

Mở trình duyệt tại `https://localhost:5001` (hoặc port hiển thị trong console).

### 7.5 Tài khoản mặc định (seed data)

| Tài khoản | Mật khẩu | Vai trò |
|-----------|----------|---------|
| admin | Admin@123 | Quản lý |
| thuthu | ThuThu@123 | Thủ thư |

> **Lưu ý**: Đổi mật khẩu ngay sau lần đăng nhập đầu tiên.

### 7.6 Hangfire Dashboard

Sau khi chạy, truy cập `https://localhost:5001/hangfire` để xem lịch sử và trạng thái các background job.

---

## VIII. KẾT QUẢ ĐẠT ĐƯỢC

### 8.1 Chức năng đã hoàn thành

| # | Chức năng | Trạng thái |
|---|-----------|------------|
| 1 | CRUD Sách, Thể loại, Tác giả, NXB | Hoàn thành |
| 2 | CRUD Độc giả | Hoàn thành |
| 3 | Lập phiếu mượn (kiểm tra tồn kho, giới hạn) | Hoàn thành |
| 4 | Gia hạn mượn (kiểm tra giới hạn lần gia hạn) | Hoàn thành |
| 5 | Trả sách (tính tiền phạt tự động) | Hoàn thành |
| 6 | Tạo và quét QR Code sách | Hoàn thành |
| 7 | Tìm kiếm đa tiêu chí có phân trang | Hoàn thành |
| 8 | Thống kê sách / độc giả mượn nhiều | Hoàn thành |
| 9 | Export danh sách sách và lịch sử mượn ra Excel | Hoàn thành |
| 10 | Dashboard tổng quan 7 chỉ số | Hoàn thành |
| 11 | Đăng nhập, phân quyền QuanLy / ThuThu | Hoàn thành |
| 12 | Quản lý tài khoản người dùng | Hoàn thành |
| 13 | Cấu hình hệ thống (số ngày mượn, tiền phạt,...) | Hoàn thành |
| 14 | Email nhắc nhở tự động (Hangfire, 7AM hàng ngày) | Hoàn thành |

### 8.2 Kỹ thuật nổi bật

- **Clean Architecture**: Tách biệt hoàn toàn domain logic khỏi hạ tầng — dễ thay đổi database hoặc thư viện email mà không ảnh hưởng business logic
- **Stored Procedures**: 8 SP xử lý các nghiệp vụ phức tạp trong transaction, đảm bảo tính toàn vẹn dữ liệu
- **Cấu hình động**: Mọi thông số nghiệp vụ đều lưu database, thay đổi tức thì không cần restart
- **Hangfire**: Background job email hoạt động độc lập, không block request của người dùng
- **BCrypt**: Mật khẩu lưu hash một chiều, không thể giải ngược

### 8.3 Kết quả kiểm thử

- Build thành công: **0 errors, 0 warnings**
- Các luồng nghiệp vụ chính đã test thủ công: mượn sách, trả sách, gia hạn, tính tiền phạt, gửi email
- Transaction rollback hoạt động đúng khi tồn kho không đủ hoặc vượt giới hạn

---

## IX. HẠN CHẾ VÀ HƯỚNG PHÁT TRIỂN

### 9.1 Hạn chế hiện tại

| Hạn chế | Mô tả |
|---------|-------|
| Không có API | Hiện tại chỉ có giao diện Web MVC, chưa có REST API để tích hợp mobile app |
| Không có portal học sinh | Học sinh chưa tự tra cứu sách hoặc xem lịch sử mượn của bản thân |
| Chưa có upload ảnh bìa | Sách chưa có ảnh bìa, giao diện kém trực quan |
| Email chưa có template đẹp | Email nhắc nhở là HTML đơn giản, chưa được thiết kế chuyên nghiệp |
| Chưa test tự động | Chỉ test thủ công, chưa có unit tests hay integration tests |
| Hangfire dùng SQL Server | Nếu scale lên cần chuyển sang Redis để hiệu năng tốt hơn |

### 9.2 Hướng phát triển

| Hướng | Mô tả |
|-------|-------|
| **REST API + Mobile App** | Xây dựng API layer để học sinh tra cứu và đặt mượn sách qua điện thoại |
| **Portal tự phục vụ** | Học sinh đăng nhập, xem sách mượn, nhận thông báo, gia hạn online |
| **Barcode scanner** | Hỗ trợ quét barcode ISBN thay vì chỉ QR Code nội bộ |
| **Ảnh bìa sách** | Upload và hiển thị ảnh bìa, tích hợp Google Books API để tự điền metadata |
| **Unit & Integration Tests** | Viết test cho Repository layer và Controller actions quan trọng |
| **Docker** | Containerize ứng dụng để dễ deploy lên server Linux |
| **Báo cáo nâng cao** | Dashboard với biểu đồ realtime, export PDF, gửi báo cáo tuần qua email |
| **Multi-tenant** | Mở rộng cho nhiều trường cùng dùng chung hạ tầng |

---

## X. TÀI LIỆU THAM KHẢO

1. Microsoft Documentation — ASP.NET Core 8 MVC: https://docs.microsoft.com/aspnet/core
2. Microsoft Documentation — Entity Framework Core 8: https://docs.microsoft.com/ef/core
3. Clean Architecture — Robert C. Martin (Uncle Bob)
4. MailKit Documentation: https://github.com/jstedfast/MailKit
5. QRCoder Documentation: https://github.com/codebude/QRCoder
6. ClosedXML Documentation: https://github.com/ClosedXML/ClosedXML
7. Hangfire Documentation: https://docs.hangfire.io
8. BCrypt.Net-Next: https://github.com/BcryptNet/bcrypt.net
9. Bootstrap 5 Documentation: https://getbootstrap.com/docs/5.0

---

*Tài liệu này được soạn thảo bởi Nhóm 3 — PTIT, phục vụ mục đích nộp báo cáo đồ án môn học.*
