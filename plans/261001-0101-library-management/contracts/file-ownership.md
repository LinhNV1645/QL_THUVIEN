# Contract: File Ownership

> **CRITICAL**: Mỗi agent chỉ được tạo/sửa file trong vùng của mình.  
> Vi phạm → compile conflict → mất công cả nhóm.

---

## Phase 01 — Foundation Agent (làm trước, không song song)

```
LibraryManagement.Domain/
├── Exceptions/
│   ├── TonKhoKhongDuException.cs
│   ├── GioiHanMuonException.cs
│   └── QuaHanGiaHanException.cs

LibraryManagement.Application/
├── Common/
│   ├── PaginatedResult.cs
│   └── Result.cs
├── Interfaces/
│   ├── ISachRepository.cs
│   ├── IDocGiaRepository.cs
│   ├── IPhieuMuonRepository.cs
│   ├── IGiaHanRepository.cs
│   ├── IPhieuTraRepository.cs
│   ├── ITaiKhoanRepository.cs
│   ├── ICauHinhRepository.cs
│   ├── IEmailService.cs
│   ├── IExportService.cs
│   ├── IQrService.cs
│   └── IUnitOfWork.cs
├── DTOs/
│   ├── Common/PaginatedResult.cs
│   ├── Sach/SachFilterDto.cs, SachMuonItem.cs
│   ├── MuonSach/LapPhieuMuonDto.cs, TraSachDto.cs, TraSachResultDto.cs
│   └── Dashboard/DashboardDto.cs

LibraryManagement.Infrastructure/
├── Persistence/
│   ├── AppDbContext.cs
│   ├── DbInitializer.cs
│   └── Migrations/

LibraryManagement.Web/
├── Program.cs
├── appsettings.json
├── Controllers/LoginController.cs
├── Views/Login/Index.cshtml
├── Views/Shared/_Layout.cshtml
├── Views/Shared/_ValidationScriptsPartial.cshtml
├── Views/Error/ (403.cshtml, 404.cshtml)
└── wwwroot/css/site.css
```

---

## Phase 02 — Agent Cơ (Sách + DocGia + QR)

```
LibraryManagement.Domain/Entities/
├── Sach.cs          ✅ Cơ
├── TheLoai.cs       ✅ Cơ
├── TacGia.cs        ✅ Cơ
├── NhaXuatBan.cs    ✅ Cơ
└── DocGia.cs        ✅ Cơ

LibraryManagement.Application/
├── UseCases/Sach/
│   ├── GetSachListUseCase.cs
│   ├── GetSachByIdUseCase.cs
│   ├── CreateSachUseCase.cs
│   ├── UpdateSachUseCase.cs
│   ├── DeleteSachUseCase.cs
│   └── TimKiemSachUseCase.cs
├── UseCases/DocGia/
│   ├── GetDocGiaListUseCase.cs
│   ├── CreateDocGiaUseCase.cs
│   ├── UpdateDocGiaUseCase.cs
│   └── LockDocGiaUseCase.cs
└── DTOs/Sach/ + DTOs/DocGia/

LibraryManagement.Infrastructure/
├── Repositories/SachRepository.cs
├── Repositories/DocGiaRepository.cs
└── Services/QrService.cs

LibraryManagement.Web/
├── Controllers/SachController.cs
├── Controllers/DocGiaController.cs
├── Controllers/DanhMucController.cs
├── Controllers/Api/QrApiController.cs   ← /api/qr/scan
├── ViewModels/Sach/
├── ViewModels/DocGia/
├── Views/Sach/
├── Views/DocGia/
├── Views/DanhMuc/
└── wwwroot/qr/
```

---

## Phase 03 — Agent Tuân (Mượn + GiaHan + TimKiem + ThongKe + Export)

```
LibraryManagement.Domain/Entities/
├── PhieuMuon.cs      ✅ Tuân
├── CTPhieuMuon.cs    ✅ Tuân
└── GiaHan.cs         ✅ Tuân

LibraryManagement.Application/
├── UseCases/MuonSach/
│   ├── LapPhieuMuonUseCase.cs
│   ├── GetPhieuMuonListUseCase.cs
│   ├── GetPhieuMuonDetailUseCase.cs
│   └── GetPhieuQuaHanUseCase.cs
├── UseCases/GiaHan/
│   ├── GiaHanUseCase.cs
│   └── GetGiaHanHistoryUseCase.cs
├── UseCases/TimKiem/
│   ├── TimKiemSachUseCase.cs
│   └── TimKiemDocGiaUseCase.cs
├── UseCases/ThongKe/
│   ├── ThongKeSachMuonNhieuUseCase.cs
│   ├── ThongKeDocGiaMuonNhieuUseCase.cs
│   └── ThongKeTongQuanUseCase.cs
└── DTOs/MuonSach/ + DTOs/ThongKe/

LibraryManagement.Infrastructure/
├── Repositories/PhieuMuonRepository.cs
├── Repositories/GiaHanRepository.cs
└── Services/ExportService.cs

LibraryManagement.Web/
├── Controllers/MuonSachController.cs
├── Controllers/GiaHanController.cs
├── Controllers/TimKiemController.cs
├── Controllers/ThongKeController.cs
├── ViewModels/MuonSach/
├── ViewModels/ThongKe/
├── Views/MuonSach/
├── Views/GiaHan/
├── Views/TimKiem/
├── Views/ThongKe/
└── wwwroot/exports/
```

---

## Phase 04 — Agent Linh (Trả + CauHinh + TaiKhoan + Email + Dashboard)

```
LibraryManagement.Domain/Entities/
├── PhieuTra.cs          ✅ Linh
├── TaiKhoan.cs          ✅ Linh
├── VaiTro.cs            ✅ Linh
└── CauHinhHeThong.cs    ✅ Linh

LibraryManagement.Application/
├── UseCases/TraSach/
│   ├── TraSachUseCase.cs
│   ├── GetPhieuTraDetailUseCase.cs
│   ├── ThuPhatUseCase.cs
│   └── GetChuaThuPhatUseCase.cs
├── UseCases/TaiKhoan/
│   ├── CreateTaiKhoanUseCase.cs
│   ├── UpdateTaiKhoanUseCase.cs
│   ├── ChangePasswordUseCase.cs
│   ├── LockUnlockTaiKhoanUseCase.cs
│   └── LoginUseCase.cs
├── UseCases/CauHinh/
│   ├── GetCauHinhUseCase.cs
│   └── UpdateCauHinhUseCase.cs
└── DTOs/TraSach/ + DTOs/TaiKhoan/

LibraryManagement.Infrastructure/
├── Repositories/PhieuTraRepository.cs
├── Repositories/TaiKhoanRepository.cs
├── Repositories/CauHinhRepository.cs
├── Services/EmailService.cs
└── Jobs/NhacNhoHanTraJob.cs

LibraryManagement.Web/
├── Controllers/TraSachController.cs
├── Controllers/TaiKhoanController.cs
├── Controllers/CauHinhController.cs
├── Controllers/HomeController.cs        ← Dashboard
├── ViewModels/TraSach/
├── ViewModels/TaiKhoan/
├── Views/TraSach/
├── Views/TaiKhoan/
├── Views/CauHinh/
└── Views/Home/Index.cshtml
```

---

## Shared — KHÔNG agent nào được sửa đơn phương

```
AppDbContext.cs          ← Phase 01 tạo; cần thêm DbSet → báo Phase 01 agent
Program.cs               ← Phase 01 tạo; cần thêm DI → báo Phase 01 agent
appsettings.json         ← Phase 01 tạo; cần thêm config → báo Phase 01 agent
_Layout.cshtml           ← Phase 01 tạo; chỉ sửa nếu cần thêm menu item
```

> Nếu cần sửa shared file: **comment vào plan** để Phase 01 agent hoặc nhóm trưởng xử lý.
