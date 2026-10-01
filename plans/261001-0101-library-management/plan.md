# Plan: Quản lý Thư viện THCS Thanh Tuân

**Ngày tạo**: 2026-10-01 | **Nhóm**: 3 thành viên | **Điểm mục tiêu**: 9+

---

## Tổng quan

| Hạng mục | Chi tiết |
|---|---|
| Tech stack | ASP.NET Core 8 MVC, SQL Server, Bootstrap 5 |
| Kiến trúc | Clean Architecture (4 layers) |
| Database | 12 bảng, 8 SP, 5 View, 2 Trigger, 6 Index |
| Phân công | Mỗi phase = 1 agent độc lập, file ownership tách biệt |

---

## Phases & Trạng thái

| Phase | Tên | Agent | Trạng thái | File |
|---|---|---|---|---|
| 01 | Foundation — DB, Domain, Auth | 1 agent (sequential) | ⬜ Pending | [phase-01](phase-01-foundation.md) |
| 02 | Book + DocGia + QR | Agent Cơ | ⬜ Pending | [phase-02](phase-02-agent-co-sach-docgia-qr.md) |
| 03 | Mượn + Gia hạn + Tìm kiếm + Thống kê + Export | Agent Tuân | ⬜ Pending | [phase-03](phase-03-agent-tuan-muon-thongke-export.md) |
| 04 | Trả + Cấu hình + Tài khoản + Email | Agent Linh | ⬜ Pending | [phase-04](phase-04-agent-linh-tra-cauhinh-email.md) |

> Phase 02, 03, 04 chạy **song song** sau khi Phase 01 hoàn thành và review xong.

---

## Contracts (đọc trước khi implement)

| File | Nội dung |
|---|---|
| [contracts/db-schema.sql](contracts/db-schema.sql) | Toàn bộ DDL: tables, FK, indexes, seed data |
| [contracts/stored-procedures.sql](contracts/stored-procedures.sql) | 8 Stored Procedures |
| [contracts/views-triggers.sql](contracts/views-triggers.sql) | 5 Views, 2 Triggers, seed data |
| [contracts/entities.md](contracts/entities.md) | C# entity classes — tên field chuẩn |
| [contracts/interfaces.md](contracts/interfaces.md) | Application layer interfaces |
| [contracts/file-ownership.md](contracts/file-ownership.md) | Ai sở hữu file nào |

---

## Cấu trúc Solution

```
LibraryManagement.sln
├── LibraryManagement.Domain/          ← Entities, Domain Exceptions
├── LibraryManagement.Application/     ← Use Cases, Interfaces, DTOs
├── LibraryManagement.Infrastructure/  ← EF Core, Repositories, Services
└── LibraryManagement.Web/             ← Controllers, Views, ViewModels
```

---

## Dependency Rules (Clean Architecture)

```
Web → Application → Domain
Infrastructure → Application → Domain
(Domain không import gì cả)
```

---

## SQL Server Connection String (sau khi VPN lên)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=<IP_SQL_SERVER>;Database=QuanLyThuVien;User Id=sa;Password=<PASSWORD>;TrustServerCertificate=True;"
}
```

> IP SQL Server: hỏi thầy/admin sau khi VPN 10.x.x.x/24 lên

---

## Checklist điểm 9+

- [ ] DB: 3NF, FK đầy đủ, 6+ index
- [ ] SP: transaction + rollback cho mượn/trả
- [ ] Trigger: ngăn xóa sách đang mượn, cập nhật tồn kho
- [ ] View: thống kê, phiếu quá hạn, lịch sử
- [ ] Clean Architecture: dependency đúng chiều
- [ ] Auth: Cookie + phân quyền 3 role (Admin, NhanVien, DocGia)
- [ ] QR: generate + scan điền form tự động
- [ ] Email: Hangfire job nhắc trước 2-3 ngày
- [ ] Export: Excel (ClosedXML) + PDF (DinkToPdf)
- [ ] Responsive: Bootstrap 5, test mobile
- [ ] Dashboard: SP sp_Dashboard hiển thị tổng quan
