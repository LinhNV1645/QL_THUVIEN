# Phase 04 — Agent Linh: Trả sách + Cấu hình + Tài khoản + Email

**Thành viên**: Nguyễn Văn Linh (B22DTCN022) — Nhóm trưởng  
**Status**: ⬜ Pending — chờ Phase 01 xong  
**Parallel**: Chạy đồng thời Phase 02 và 03  
**Ước tính**: 4–5 giờ

---

## File Ownership (KHÔNG đụng file ngoài danh sách này)

```
Domain/Entities/PhieuTra.cs
Domain/Entities/TaiKhoan.cs
Domain/Entities/VaiTro.cs
Domain/Entities/CauHinhHeThong.cs

Application/UseCases/TraSach/
Application/UseCases/TaiKhoan/
Application/UseCases/CauHinh/
Application/DTOs/TraSach/
Application/DTOs/TaiKhoan/

Infrastructure/Repositories/PhieuTraRepository.cs
Infrastructure/Repositories/TaiKhoanRepository.cs
Infrastructure/Repositories/CauHinhRepository.cs
Infrastructure/Services/EmailService.cs
Infrastructure/Jobs/NhacNhoHanTraJob.cs

Web/Controllers/TraSachController.cs
Web/Controllers/TaiKhoanController.cs
Web/Controllers/CauHinhController.cs
Web/Controllers/HomeController.cs      ← Dashboard
Web/ViewModels/TraSach/
Web/ViewModels/TaiKhoan/
Web/Views/TraSach/
Web/Views/TaiKhoan/
Web/Views/CauHinh/
Web/Views/Home/
```

---

## Todo List

### Repositories
- [ ] `PhieuTraRepository.cs` — implement `IPhieuTraRepository`
  - `TraSachAsync(dto)` → gọi `sp_TraSach`
  - `GetByIdAsync`, `GetByPhieuMuonAsync`
  - `GetChuaThuPhatAsync` — danh sách phạt chưa thu
  - `ThuPhatAsync(maPhieuTra)` — cập nhật `DaThuPhat = true`
- [ ] `TaiKhoanRepository.cs` — implement `ITaiKhoanRepository`
  - `GetByUsernameAsync`, `GetAllAsync`
  - `CreateAsync`, `UpdateAsync`, `ChangePasswordAsync`
  - `LockAsync`, `UnlockAsync`
- [ ] `CauHinhRepository.cs` — implement `ICauHinhRepository`
  - `GetValueAsync(tenCauHinh)` → trả string
  - `GetAllAsync`, `UpdateAsync(tenCauHinh, giaTri)`

### Use Cases — Trả sách
- [ ] `TraSachUseCase.cs`
  - Gọi `sp_TraSach` → trả về MaPhieuTra, TienPhat, SoNgayTreHan
  - Nếu TienPhat > 0: hiển thị dialog xác nhận phạt
- [ ] `GetPhieuTraDetailUseCase.cs`
- [ ] `ThuPhatUseCase.cs` — đánh dấu đã thu tiền phạt
- [ ] `GetChuaThuPhatUseCase.cs` — tổng hợp nợ phạt

### Use Cases — Tài khoản
- [ ] `CreateTaiKhoanUseCase.cs` — hash BCrypt, gán role
- [ ] `UpdateTaiKhoanUseCase.cs`
- [ ] `ChangePasswordUseCase.cs` — verify mật khẩu cũ trước
- [ ] `LockUnlockTaiKhoanUseCase.cs`
- [ ] `LoginUseCase.cs` — verify BCrypt, tạo ClaimsPrincipal

### Use Cases — Cấu hình
- [ ] `GetCauHinhUseCase.cs`
- [ ] `UpdateCauHinhUseCase.cs` — Admin only
  - Validate: SoNgayMuon > 0, MucPhat >= 0, SoSachToiDa >= 1

### Email Service
- [ ] `EmailService.cs` implement `IEmailService`
  - Dùng **MailKit** + SMTP config từ `appsettings.json`
  - `SendNhacNhoHanTraAsync(toEmail, tenDocGia, danhSachSach, ngayHanTra)`
  - Template HTML email (inline CSS, hiển thị tên sách + hạn trả)
  ```csharp
  public async Task SendNhacNhoHanTraAsync(
      string toEmail, string tenDocGia,
      string danhSachSach, DateTime ngayHanTra)
  {
      var body = $@"
      <h2>Nhắc nhở trả sách — Thư viện THCS Thanh Tuân</h2>
      <p>Bạn <strong>{tenDocGia}</strong> ơi,</p>
      <p>Bạn đang mượn sách: <strong>{danhSachSach}</strong></p>
      <p>Hạn trả: <strong>{ngayHanTra:dd/MM/yyyy}</strong></p>
      <p style='color:red'>Vui lòng trả đúng hạn để tránh bị phạt.</p>";
      // gửi qua MailKit MimeMessage
  }
  ```

### Hangfire Job — Nhắc hạn tự động
- [ ] `NhacNhoHanTraJob.cs`
  - Gọi `sp_LayPhieuMuonSapDenHan(@SoNgayTruoc = 3)`
  - Với mỗi kết quả → gọi `EmailService.SendNhacNhoHanTraAsync`
  - Đăng ký Hangfire recurring: chạy lúc 7:00 sáng mỗi ngày
  ```csharp
  RecurringJob.AddOrUpdate<NhacNhoHanTraJob>(
      "nhac-nho-han-tra",
      job => job.ExecuteAsync(),
      "0 7 * * *");  // 7:00 AM hàng ngày
  ```

### Controllers & Views — Trả sách
- [ ] `TraSachController.cs`
  - `GET  /TraSach`              → danh sách phiếu chờ trả (TrangThai IN 1,3,4)
  - `GET  /TraSach/Create/{maPhieuMuon}` → form xác nhận trả
  - `POST /TraSach/Create`       → gọi sp_TraSach, hiển thị phiếu phạt
  - `GET  /TraSach/Detail/{id}`  → chi tiết phiếu trả
  - `GET  /TraSach/ChuaThuPhat`  → danh sách nợ phạt chưa thu
  - `POST /TraSach/ThuPhat/{id}` → đánh dấu đã thu
- [ ] Views: `Index.cshtml`, `Create.cshtml`, `Detail.cshtml`, `ChuaThuPhat.cshtml`
  - **Create.cshtml**: hiển thị số ngày trễ + tiền phạt tính được trước khi submit
  - JS tính phạt real-time: `soNgayTre * mucPhat`

### Controllers & Views — Tài khoản
- [ ] `TaiKhoanController.cs` — [Authorize(Roles="Admin")]
  - CRUD tài khoản nhân viên
  - `POST /TaiKhoan/ChangePassword/{id}`
  - `POST /TaiKhoan/Lock/{id}`, `Unlock/{id}`
- [ ] Views: `Index.cshtml`, `Create.cshtml`, `Edit.cshtml`

### Controllers & Views — Cấu hình
- [ ] `CauHinhController.cs` — [Authorize(Roles="Admin")]
  - `GET  /CauHinh`    → bảng tất cả cấu hình
  - `POST /CauHinh/Update` → cập nhật từng giá trị (form inline)
- [ ] View: `Index.cshtml` — bảng editable inline

### Dashboard
- [ ] `HomeController.cs`
  - `GET /` → gọi `sp_Dashboard` → trả về DashboardViewModel
- [ ] `Views/Home/Index.cshtml`
  - Cards: Tổng đầu sách | Sách tồn | Đang mượn | Quá hạn | Tiền phạt chưa thu
  - Bảng phiếu quá hạn mới nhất (5 dòng)
  - Bảng sách sắp hết tồn (SoLuongTon <= 2)

---

## appsettings.json — Email config

```json
"EmailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "UserName": "thuvien.thcsthanhtuan@gmail.com",
  "Password": "<APP_PASSWORD>",
  "DisplayName": "Thư viện THCS Thanh Tuân"
}
```

---

## ViewModels

```csharp
public record TraSachViewModel(
    PhieuMuonDto PhieuMuon,
    List<CTPhieuMuonDto> DanhSachSach,
    int SoNgayTreHan,
    decimal TienPhat,
    decimal MucPhatNgay
);

public record DashboardViewModel(
    int TongDauSach, int TongSachTon,
    int TongDocGia, int DangMuon,
    int QuaHan, decimal TienPhatChuaThu,
    int MuonTrongThang,
    IEnumerable<PhieuMuonQuaHanDto> QuaHanMoiNhat
);
```

---

## Success Criteria

- [ ] Trả sách: tính đúng tiền phạt, cập nhật tồn kho qua SP
- [ ] Thu tiền phạt: đánh dấu DaThuPhat
- [ ] Dashboard hiển thị đủ 7 chỉ số từ `sp_Dashboard`
- [ ] CRUD tài khoản + đổi mật khẩu (BCrypt)
- [ ] Phân quyền đúng: Admin mới vào được /TaiKhoan, /CauHinh
- [ ] Email gửi được qua SMTP (test với Gmail App Password)
- [ ] Hangfire job chạy 7:00 sáng, nhắc đúng phiếu sắp đến hạn
- [ ] Cấu hình hệ thống: thay đổi được mức phạt, số ngày mượn
