-- ============================================================
-- THÊM 10 CUỐN SÁCH VÀO BẢNG Sach — QuanLyThuVien
-- Chỉ dùng thể loại / tác giả / NXB ĐÃ CÓ (không tạo mới danh mục).
-- Mã khóa ngoại theo dữ liệu mẫu (04-seed-data.sql):
--   TheLoai    : 1 Văn học, 2 Khoa học, 3 Lịch sử, 4 Toán học, 5 Tiếng Anh, 6 Truyện thiếu nhi
--   TacGia     : 1 Nguyễn Du, 2 Nam Quốc Chánh, 3 Tô Hoài, 4 Antoine de Saint-Exupéry, 5 Nhiều tác giả
--   NhaXuatBan : 1 NXB Giáo dục Việt Nam, 2 NXB Kim Đồng, 3 NXB Trẻ, 4 NXB Văn học
-- Nếu mã trong CSDL của bạn khác, chạy 3 câu SELECT dưới đây để xem rồi sửa lại cho khớp.
-- ============================================================

USE QuanLyThuVien;
GO

-- SELECT MaTheLoai, TenTheLoai FROM TheLoai;
-- SELECT MaTacGia,  TenTacGia  FROM TacGia;
-- SELECT MaNXB,     TenNXB     FROM NhaXuatBan;

INSERT INTO Sach (MaTheLoai, MaTacGia, MaNXB, TenSach, NamXuatBan, SoTrang, SoLuongNhap, SoLuongTon, ViTri, MaQR, MoTa) VALUES
    (1, 3, 4, N'Vợ Chồng A Phủ',              2020, 120, 5, 5, N'A1-04', 'QR009', N'Truyện ngắn về cuộc sống người Mông ở Tây Bắc'),
    (6, 3, 2, N'O Chuột',                     2019,  96, 6, 6, N'A2-02', 'QR010', N'Tập truyện đồng thoại cho thiếu nhi'),
    (6, 3, 2, N'Đảo Hoang',                   2021, 152, 4, 4, N'A2-03', 'QR011', N'Câu chuyện về Mai An Tiêm trên đảo hoang'),
    (1, 1, 4, N'Thơ Chữ Hán Nguyễn Du',       2018, 260, 3, 3, N'A1-05', 'QR012', N'Tuyển tập thơ chữ Hán kèm bản dịch'),
    (1, 4, 3, N'Bay Đêm',                     2022, 168, 4, 4, N'A1-06', 'QR013', N'Tiểu thuyết về những phi công đưa thư ban đêm'),
    (4, 5, 1, N'Toán 9 - Tập 1',              2024, 132, 8, 8, N'B2-04', 'QR014', N'Sách giáo khoa Toán lớp 9'),
    (2, 5, 1, N'Khoa Học Tự Nhiên 8',         2023, 196, 8, 8, N'B2-05', 'QR015', N'Sách giáo khoa Khoa học tự nhiên lớp 8'),
    (5, 5, 1, N'Tiếng Anh 9',                 2024, 144, 7, 7, N'B2-06', 'QR016', N'Sách giáo khoa Tiếng Anh lớp 9'),
    (3, 5, 1, N'Lịch Sử và Địa Lí 8',         2023, 176, 6, 6, N'C3-02', 'QR017', N'Sách giáo khoa Lịch sử và Địa lí lớp 8'),
    (1, 5, 1, N'Ngữ Văn 7 - Tập 1',           2022, 140, 8, 8, N'B2-07', 'QR018', N'Sách giáo khoa Ngữ văn lớp 7');
GO

-- Kiểm tra kết quả
SELECT s.MaSach, s.TenSach, tl.TenTheLoai, tg.TenTacGia, nxb.TenNXB,
       s.NamXuatBan, s.SoLuongNhap, s.SoLuongTon, s.ViTri, s.MaQR
FROM Sach s
JOIN TheLoai tl     ON tl.MaTheLoai = s.MaTheLoai
JOIN TacGia tg      ON tg.MaTacGia  = s.MaTacGia
JOIN NhaXuatBan nxb ON nxb.MaNXB    = s.MaNXB
WHERE s.MaQR BETWEEN 'QR009' AND 'QR018'
ORDER BY s.MaQR;
GO
