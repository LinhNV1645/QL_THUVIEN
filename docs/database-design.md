# Thiết Kế Cơ Sở Dữ Liệu — Quản Lý Thư Viện THCS Thanh Tuân

**Phiên bản**: 1.0 | **Ngày**: 01/10/2026 | **DBMS**: SQL Server 2019+

---

## 1. ER Diagram (Sơ đồ quan hệ)

```
TheLoai          TacGia          NhaXuatBan
(MaTheLoai PK)   (MaTacGia PK)   (MaNXB PK)
     │                │                │
     └────────────────┼────────────────┘
                      ▼
                   Sach (MaSach PK)
                   ├─ MaTheLoai FK → TheLoai
                   ├─ MaTacGia  FK → TacGia
                   ├─ MaNXB     FK → NhaXuatBan
                   ├─ TenSach, NamXuatBan, SoTrang
                   ├─ SoLuongNhap, SoLuongTon
                   ├─ ViTri, MaQR (UNIQUE), MoTa
                   └─ NgayNhap, TrangThai
                        │
                        │ FK (MaSach)
                        ▼
VaiTro           CTPhieuMuon (MaCT PK)
(MaVaiTro PK)    ├─ MaPhieuMuon FK ──────────┐
     │           ├─ MaSach      FK            │
     │           ├─ SoLuongMuon               │
     │           └─ TrangThaiCT               │
     │                                        │
     │ FK (MaVaiTro)                          │
     ▼                                        ▼
TaiKhoan (MaTaiKhoan PK)         PhieuMuon (MaPhieuMuon PK)
├─ MaVaiTro FK → VaiTro          ├─ MaDocGia FK ──────┐
├─ MaDocGia FK → DocGia (NULL)   ├─ NgayMuon           │
├─ TenDangNhap (UNIQUE)          ├─ NgayHanTra         │
├─ MatKhau (BCrypt hash)         ├─ TrangThai          │
├─ HoTen, Email, SoDienThoai     ├─ GhiChu             │
└─ NgayTao, LanDangNhapCuoi,     └─ NhanVienLap        │
   TrangThai                          │                 │
                                      │ FK              │
                               ┌──────┘      DocGia (MaDocGia PK)
                               │             ├─ HoTen, Lop
                               ▼             ├─ NgaySinh, GioiTinh
                          GiaHan             ├─ DiaChi, Email
                          (MaGiaHan PK)      ├─ SoDienThoai
                          ├─ MaPhieuMuon FK  └─ NgayDangKy, TrangThai
                          ├─ NgayGiaHan
                          ├─ HanTraCu, HanTraMoi
                          ├─ LanGiaHan
                          └─ NhanVienDuyet

                          PhieuTra (MaPhieuTra PK)
                          ├─ MaPhieuMuon FK (UNIQUE)
                          ├─ NgayTra
                          ├─ SoNgayMuon, SoNgayTreHan
                          ├─ TienPhat, DaThuPhat
                          ├─ TrangThaiSach
                          └─ NhanVienThu

CauHinhHeThong (MaCauHinh PK)
├─ TenCauHinh (UNIQUE)
└─ GiaTri, GhiChu
```

---

## 2. Mô Tả Từng Bảng

### 2.1 TheLoai — Thể loại sách

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaTheLoai | INT | PK, IDENTITY | Mã thể loại tự tăng |
| TenTheLoai | NVARCHAR(100) | NOT NULL | Tên thể loại (Văn học, Khoa học,...) |

### 2.2 TacGia — Tác giả

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaTacGia | INT | PK, IDENTITY | Mã tác giả tự tăng |
| TenTacGia | NVARCHAR(150) | NOT NULL | Tên đầy đủ tác giả |

### 2.3 NhaXuatBan — Nhà xuất bản

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaNXB | INT | PK, IDENTITY | Mã NXB tự tăng |
| TenNXB | NVARCHAR(150) | NOT NULL | Tên nhà xuất bản |

### 2.4 Sach — Sách

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaSach | INT | PK, IDENTITY | Mã sách tự tăng |
| MaTheLoai | INT | FK → TheLoai | Thể loại sách |
| MaTacGia | INT | FK → TacGia | Tác giả |
| MaNXB | INT | FK → NhaXuatBan | Nhà xuất bản |
| TenSach | NVARCHAR(255) | NOT NULL | Tên sách |
| NamXuatBan | INT | NULL | Năm xuất bản |
| SoTrang | INT | NULL | Số trang |
| SoLuongNhap | INT | NOT NULL, DEFAULT 0 | Tổng số lượng nhập |
| SoLuongTon | INT | NOT NULL, DEFAULT 0 | Số lượng còn trong kho |
| ViTri | NVARCHAR(50) | NULL | Vị trí kệ sách |
| MaQR | NVARCHAR(100) | UNIQUE, NULL | Đường dẫn file QR Code |
| MoTa | NVARCHAR(MAX) | NULL | Mô tả nội dung sách |
| NgayNhap | DATE | DEFAULT GETDATE() | Ngày nhập sách |
| TrangThai | BIT | DEFAULT 1 | 1 = Đang lưu hành, 0 = Ngừng |

### 2.5 VaiTro — Vai trò người dùng

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaVaiTro | INT | PK, IDENTITY | Mã vai trò |
| TenVaiTro | NVARCHAR(50) | NOT NULL | QuanLy / ThuThu |

### 2.6 DocGia — Độc giả

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaDocGia | INT | PK, IDENTITY | Mã độc giả tự tăng |
| HoTen | NVARCHAR(150) | NOT NULL | Họ tên đầy đủ |
| Lop | NVARCHAR(20) | NULL | Lớp học |
| NgaySinh | DATE | NULL | Ngày sinh |
| GioiTinh | NVARCHAR(10) | NULL | Nam / Nữ |
| DiaChi | NVARCHAR(255) | NULL | Địa chỉ |
| Email | NVARCHAR(150) | NULL | Email nhận thông báo |
| SoDienThoai | NVARCHAR(20) | NULL | Số điện thoại phụ huynh |
| NgayDangKy | DATE | DEFAULT GETDATE() | Ngày đăng ký thẻ |
| TrangThai | BIT | DEFAULT 1 | 1 = Đang hoạt động |

### 2.7 TaiKhoan — Tài khoản đăng nhập

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaTaiKhoan | INT | PK, IDENTITY | Mã tài khoản |
| MaVaiTro | INT | FK → VaiTro | Vai trò |
| MaDocGia | INT | FK → DocGia, NULL | Liên kết độc giả (nếu là học sinh) |
| TenDangNhap | NVARCHAR(50) | UNIQUE, NOT NULL | Username |
| MatKhau | NVARCHAR(255) | NOT NULL | BCrypt hash |
| HoTen | NVARCHAR(150) | NOT NULL | Tên hiển thị |
| Email | NVARCHAR(150) | NULL | Email |
| SoDienThoai | NVARCHAR(20) | NULL | SĐT |
| NgayTao | DATETIME | DEFAULT GETDATE() | Ngày tạo tài khoản |
| LanDangNhapCuoi | DATETIME | NULL | Lần đăng nhập gần nhất |
| TrangThai | BIT | DEFAULT 1 | 1 = Hoạt động |

### 2.8 PhieuMuon — Phiếu mượn sách

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaPhieuMuon | INT | PK, IDENTITY | Mã phiếu mượn |
| MaDocGia | INT | FK → DocGia | Độc giả mượn |
| NgayMuon | DATE | DEFAULT GETDATE() | Ngày lập phiếu |
| NgayHanTra | DATE | NOT NULL | Hạn trả sách |
| TrangThai | NVARCHAR(20) | DEFAULT 'DangMuon' | DangMuon / DaHanTra / DaTra |
| GhiChu | NVARCHAR(500) | NULL | Ghi chú thêm |
| NhanVienLap | NVARCHAR(150) | NOT NULL | Tên thủ thư lập phiếu |

### 2.9 CTPhieuMuon — Chi tiết phiếu mượn

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaCT | INT | PK, IDENTITY | Mã chi tiết |
| MaPhieuMuon | INT | FK → PhieuMuon | Phiếu mượn cha |
| MaSach | INT | FK → Sach | Sách được mượn |
| SoLuongMuon | INT | NOT NULL, DEFAULT 1 | Số quyển mượn |
| TrangThaiCT | NVARCHAR(20) | DEFAULT 'DangMuon' | DangMuon / DaTra |

### 2.10 GiaHan — Gia hạn mượn

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaGiaHan | INT | PK, IDENTITY | Mã gia hạn |
| MaPhieuMuon | INT | FK → PhieuMuon | Phiếu mượn được gia hạn |
| NgayGiaHan | DATE | DEFAULT GETDATE() | Ngày thực hiện gia hạn |
| HanTraCu | DATE | NOT NULL | Hạn trả cũ |
| HanTraMoi | DATE | NOT NULL | Hạn trả mới sau gia hạn |
| LanGiaHan | INT | NOT NULL | Lần thứ mấy gia hạn |
| NhanVienDuyet | NVARCHAR(150) | NOT NULL | Nhân viên duyệt |
| GhiChu | NVARCHAR(500) | NULL | Lý do gia hạn |

### 2.11 PhieuTra — Phiếu trả sách

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaPhieuTra | INT | PK, IDENTITY | Mã phiếu trả |
| MaPhieuMuon | INT | FK → PhieuMuon, UNIQUE | 1 phiếu mượn → 1 phiếu trả |
| NgayTra | DATE | DEFAULT GETDATE() | Ngày trả thực tế |
| SoNgayMuon | INT | NOT NULL | Tổng số ngày đã mượn |
| SoNgayTreHan | INT | DEFAULT 0 | Số ngày trễ (0 nếu đúng hạn) |
| TienPhat | DECIMAL(18,2) | DEFAULT 0 | Số tiền phạt (VNĐ) |
| DaThuPhat | BIT | DEFAULT 0 | Đã thu tiền phạt chưa |
| TrangThaiSach | NVARCHAR(50) | NOT NULL | Tot / HuHong / MatSach |
| GhiChu | NVARCHAR(500) | NULL | Ghi chú tình trạng sách |
| NhanVienThu | NVARCHAR(150) | NOT NULL | Nhân viên thu sách |

### 2.12 CauHinhHeThong — Cấu hình hệ thống

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| MaCauHinh | INT | PK, IDENTITY | Mã cấu hình |
| TenCauHinh | NVARCHAR(100) | UNIQUE, NOT NULL | Key cấu hình |
| GiaTri | NVARCHAR(500) | NOT NULL | Giá trị |
| GhiChu | NVARCHAR(500) | NULL | Mô tả ý nghĩa |

**Seed data mặc định**:

| TenCauHinh | GiaTri | Ý nghĩa |
|------------|--------|---------|
| SoNgayMuonToiDa | 14 | Số ngày mượn tối đa |
| SoLanGiaHanToiDa | 2 | Số lần gia hạn tối đa |
| SoSachMuonToiDa | 3 | Số sách mượn cùng lúc tối đa |
| TienPhatMoiNgay | 1000 | Tiền phạt mỗi ngày trễ (VNĐ) |
| SmtpHost | smtp.gmail.com | SMTP server gửi email |
| SmtpPort | 587 | SMTP port |

---

## 3. Stored Procedures (8 SP)

### sp_LapPhieuMuon
**Mục đích**: Lập phiếu mượn trong một transaction an toàn.
**Input**: `@MaDocGia, @NgayHanTra, @DanhSachSach (TVP), @NhanVienLap`
**Logic**:
1. Kiểm tra `SoLuongTon >= SoLuongMuon` cho từng sách — báo lỗi nếu không đủ
2. Đếm số sách đang mượn của `@MaDocGia` — báo lỗi nếu vượt `SoSachMuonToiDa`
3. `INSERT INTO PhieuMuon` → lấy `SCOPE_IDENTITY()`
4. `INSERT INTO CTPhieuMuon` cho từng sách
5. `UPDATE Sach SET SoLuongTon -= SoLuongMuon`
6. Commit hoặc Rollback

### sp_GiaHanPhieuMuon
**Mục đích**: Gia hạn hạn trả của một phiếu mượn.
**Input**: `@MaPhieuMuon, @SoNgayGiaHan, @NhanVienDuyet, @GhiChu`
**Logic**:
1. Kiểm tra phiếu tồn tại và chưa trả
2. Đếm số lần đã gia hạn — báo lỗi nếu vượt `SoLanGiaHanToiDa`
3. `INSERT INTO GiaHan` (HanTraCu = NgayHanTra hiện tại)
4. `UPDATE PhieuMuon SET NgayHanTra += @SoNgayGiaHan`

### sp_TraSach
**Mục đích**: Xử lý trả sách và tính tiền phạt.
**Input**: `@MaPhieuMuon, @TrangThaiSach, @GhiChu, @NhanVienThu`
**Logic**:
1. Tính `SoNgayTreHan = MAX(0, NgayTra - NgayHanTra)`
2. Tính `TienPhat = SoNgayTreHan × TienPhatMoiNgay`
3. `INSERT INTO PhieuTra`
4. `UPDATE Sach SET SoLuongTon += SoLuongMuon` (hoàn kho)
5. `UPDATE CTPhieuMuon SET TrangThaiCT = 'DaTra'`
6. `UPDATE PhieuMuon SET TrangThai = 'DaTra'`

### sp_TimKiemSach
**Mục đích**: Tìm kiếm sách đa tiêu chí với phân trang.
**Input**: `@TuKhoa, @MaTheLoai, @MaTacGia, @TrangThai, @PageIndex, @PageSize`
**Logic**: CTE + ROW_NUMBER() OVER (ORDER BY TenSach) để phân trang hiệu quả.

### sp_ThongKeSachMuonNhieu
**Mục đích**: Top N sách được mượn nhiều nhất theo tháng/năm.
**Input**: `@Thang, @Nam, @TopN`
**Output**: `MaSach, TenSach, SoLanMuon` (ORDER BY SoLanMuon DESC)

### sp_ThongKeDocGiaMuonNhieu
**Mục đích**: Top N độc giả mượn sách nhiều nhất theo tháng/năm.
**Input**: `@Thang, @Nam, @TopN`
**Output**: `MaDocGia, HoTen, Lop, SoLanMuon`

### sp_LayPhieuMuonSapDenHan
**Mục đích**: Lấy danh sách phiếu mượn sắp đến hạn để gửi email nhắc nhở.
**Input**: `@SoNgayTruoc` (mặc định 2)
**Output**: `MaPhieuMuon, HoTen, Email, TenSach, NgayHanTra`
**Điều kiện**: `TrangThai = 'DangMuon' AND NgayHanTra = CAST(GETDATE() AS DATE) + @SoNgayTruoc`

### sp_Dashboard
**Mục đích**: Tổng hợp 7 chỉ số cho trang chủ trong 1 lần query.
**Output** (7 SELECT con):
1. Tổng số sách (distinct tiêu đề)
2. Tổng số độc giả đang hoạt động
3. Số phiếu đang mượn
4. Số phiếu quá hạn
5. Số sách mượn trong tháng hiện tại
6. Số tiền phạt chưa thu
7. Số phiếu trả trong tháng hiện tại

---

## 4. Views (5 Views)

| View | Mục đích |
|------|----------|
| `v_SachDayDu` | JOIN Sach + TheLoai + TacGia + NhaXuatBan — dùng cho trang danh sách sách |
| `v_PhieuMuonDayDu` | JOIN PhieuMuon + DocGia + CTPhieuMuon + Sach — dùng cho trang quản lý mượn |
| `v_PhieuMuonQuaHan` | Filter `TrangThai = 'DangMuon' AND NgayHanTra < GETDATE()` — cảnh báo quá hạn |
| `v_TaiKhoanDayDu` | JOIN TaiKhoan + VaiTro + DocGia — dùng cho trang quản lý tài khoản |
| `v_ThongKeMuonTheoThang` | GROUP BY YEAR(NgayMuon), MONTH(NgayMuon) — dùng cho biểu đồ thống kê |

---

## 5. Triggers (2 Triggers)

### trg_CapNhatTrangThaiQuaHan
**Bảng**: PhieuMuon | **Sự kiện**: AFTER UPDATE
**Mục đích**: Tự động cập nhật `TrangThai = 'DaHanTra'` cho các phiếu có `NgayHanTra < GETDATE()` và chưa được trả. Được gọi bởi Hangfire job hàng ngày.

### trg_KiemTraGioiHanMuon
**Bảng**: CTPhieuMuon | **Sự kiện**: INSTEAD OF INSERT
**Mục đích**: Kiểm tra lần cuối trước khi INSERT chi tiết phiếu mượn — đảm bảo `SoLuongTon > 0`. Là lớp bảo vệ thứ hai sau Stored Procedure.

---

## 6. Indexes và Lý Do Tối Ưu

| Index | Bảng | Cột | Lý do |
|-------|------|-----|-------|
| IX_Sach_TenSach | Sach | TenSach | Tìm kiếm sách theo tên (LIKE '%...%') |
| IX_Sach_MaTheLoai | Sach | MaTheLoai | Filter theo thể loại |
| IX_Sach_MaQR | Sach | MaQR (UNIQUE) | Tra cứu nhanh khi scan QR |
| IX_DocGia_HoTen | DocGia | HoTen | Tìm kiếm độc giả theo tên |
| IX_PhieuMuon_MaDocGia | PhieuMuon | MaDocGia | Lấy lịch sử mượn của 1 độc giả |
| IX_PhieuMuon_TrangThai | PhieuMuon | TrangThai | Filter phiếu đang mượn / quá hạn |
| IX_PhieuMuon_NgayHanTra | PhieuMuon | NgayHanTra | Query phiếu sắp đến hạn (job email) |
| IX_TaiKhoan_TenDangNhap | TaiKhoan | TenDangNhap (UNIQUE) | Đăng nhập nhanh |
| IX_CTPhieuMuon_MaPhieuMuon | CTPhieuMuon | MaPhieuMuon | JOIN chi tiết phiếu mượn |
| IX_GiaHan_MaPhieuMuon | GiaHan | MaPhieuMuon | Lấy lịch sử gia hạn |
