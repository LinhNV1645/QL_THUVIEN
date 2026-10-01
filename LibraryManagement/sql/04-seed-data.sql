-- ============================================================
-- SEED DATA MẪU — QuanLyThuVien
-- Chỉ chạy MỘT LẦN trên database mới tạo (sau 01, 02, 03).
-- ============================================================

USE QuanLyThuVien;
GO

INSERT INTO TheLoai (TenTheLoai, MoTa) VALUES
    (N'Văn học',        N'Tiểu thuyết, truyện ngắn, thơ'),
    (N'Khoa học',       N'Vật lý, Hóa học, Sinh học'),
    (N'Lịch sử',        N'Lịch sử Việt Nam và thế giới'),
    (N'Toán học',       N'Giáo khoa và tham khảo toán'),
    (N'Tiếng Anh',      N'SGK và sách luyện tập tiếng Anh'),
    (N'Truyện thiếu nhi', N'Truyện tranh, truyện cổ tích');

INSERT INTO TacGia (TenTacGia, QuocTich) VALUES
    (N'Nguyễn Du',          N'Việt Nam'),
    (N'Nam Quốc Chánh',     N'Việt Nam'),
    (N'Tô Hoài',            N'Việt Nam'),
    (N'Antoine de Saint-Exupéry', N'Pháp'),
    (N'Nhiều tác giả',      N'Việt Nam');

INSERT INTO NhaXuatBan (TenNXB, DiaChi) VALUES
    (N'NXB Giáo dục Việt Nam',  N'Hà Nội'),
    (N'NXB Kim Đồng',           N'Hà Nội'),
    (N'NXB Trẻ',                N'TP.HCM'),
    (N'NXB Văn học',            N'Hà Nội');

INSERT INTO DocGia (HoTen, Lop, Email, SoDienThoai) VALUES
    (N'Nguyễn Thị An',   N'6A1', N'an.nguyen@email.com',   N'0901000001'),
    (N'Trần Văn Bình',   N'7B2', N'binh.tran@email.com',   N'0901000002'),
    (N'Lê Thị Cúc',      N'8C3', N'cuc.le@email.com',      N'0901000003'),
    (N'Phạm Minh Đức',   N'9A1', N'duc.pham@email.com',    N'0901000004'),
    (N'Hoàng Thị Em',    N'6B2', N'em.hoang@email.com',    N'0901000005');

INSERT INTO Sach (MaTheLoai, MaTacGia, MaNXB, TenSach, NamXuatBan, SoLuongNhap, SoLuongTon, ViTri, MaQR) VALUES
    (1, 1, 4, N'Truyện Kiều',              2020, 5, 5, N'A1-01', N'QR001'),
    (1, 3, 2, N'Dế Mèn Phiêu Lưu Ký',     2019, 8, 8, N'A1-02', N'QR002'),
    (1, 4, 3, N'Hoàng Tử Bé',             2021, 4, 4, N'A1-03', N'QR003'),
    (2, 5, 1, N'Vật Lý 9',                2022, 6, 6, N'B2-01', N'QR004'),
    (4, 5, 1, N'Toán 8 - Tập 1',          2022, 7, 7, N'B2-02', N'QR005'),
    (5, 5, 1, N'Tiếng Anh 7',             2022, 5, 5, N'B2-03', N'QR006'),
    (3, 5, 1, N'Lịch Sử Việt Nam',        2021, 3, 3, N'C3-01', N'QR007'),
    (6, 5, 2, N'Thám Tử Lừng Danh Conan', 2020, 10, 10, N'A2-01', N'QR008');

-- Tài khoản mặc định (BCrypt hash thật). Đổi mật khẩu ngay sau lần đăng nhập đầu tiên.
--   admin      / Admin@123
--   nhanvien01 / NhanVien@123
INSERT INTO TaiKhoan (MaVaiTro, TenDangNhap, MatKhau, HoTen, Email) VALUES
    (1, 'admin',      '$2a$11$uUj85gppDR4tvGqJxX4j9.gtFDOd/WJ8FpIBlcc7/QoWPwJ1.hhIa', N'Quản trị viên',  N'admin@thuvien.edu.vn'),
    (2, 'nhanvien01', '$2a$11$JTdwC6swDIGs.pX7gFG1POqiL2tqTcZPNGlup/WOa/K1CYkfOX.Ru', N'Nguyễn Thị Hoa', N'hoa.nguyen@thuvien.edu.vn');
GO
