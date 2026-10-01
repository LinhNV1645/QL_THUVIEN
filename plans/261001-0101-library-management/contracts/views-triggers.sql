-- ============================================================
-- VIEWS + TRIGGERS — QuanLyThuVien
-- ============================================================

USE QuanLyThuVien;
GO

-- ============================================================
-- VIEWS
-- ============================================================

-- V1: Danh sách sách đầy đủ thông tin
CREATE OR ALTER VIEW vw_SachDayDu AS
SELECT
    s.MaSach, s.TenSach, s.NamXuatBan, s.SoTrang,
    s.SoLuongNhap, s.SoLuongTon, s.ViTri, s.MaQR, s.TrangThai,
    tl.MaTheLoai, tl.TenTheLoai,
    tg.MaTacGia, tg.TenTacGia,
    nxb.MaNXB, nxb.TenNXB
FROM Sach s
JOIN TheLoai tl     ON s.MaTheLoai = tl.MaTheLoai
JOIN TacGia tg      ON s.MaTacGia  = tg.MaTacGia
JOIN NhaXuatBan nxb ON s.MaNXB     = nxb.MaNXB;
GO

-- V2: Phiếu mượn kèm thông tin độc giả và sách
CREATE OR ALTER VIEW vw_PhieuMuonDayDu AS
SELECT
    pm.MaPhieuMuon, pm.NgayMuon, pm.NgayHanTra, pm.TrangThai,
    pm.GhiChu,
    dg.MaDocGia, dg.HoTen AS TenDocGia, dg.Lop, dg.Email,
    DATEDIFF(DAY, pm.NgayHanTra, CAST(GETDATE() AS DATE)) AS SoNgayTreHan,
    CASE
        WHEN pm.TrangThai = 2 THEN N'Đã trả'
        WHEN pm.NgayHanTra < CAST(GETDATE() AS DATE) AND pm.TrangThai IN (1,4) THEN N'Quá hạn'
        WHEN pm.TrangThai = 4 THEN N'Đã gia hạn'
        ELSE N'Đang mượn'
    END AS TrangThaiText,
    COUNT(ct.MaCT) AS SoSachMuon
FROM PhieuMuon pm
JOIN DocGia dg      ON pm.MaDocGia    = dg.MaDocGia
JOIN CTPhieuMuon ct ON pm.MaPhieuMuon = ct.MaPhieuMuon
GROUP BY
    pm.MaPhieuMuon, pm.NgayMuon, pm.NgayHanTra, pm.TrangThai, pm.GhiChu,
    dg.MaDocGia, dg.HoTen, dg.Lop, dg.Email;
GO

-- V3: Thống kê sách mượn nhiều nhất tháng hiện tại
CREATE OR ALTER VIEW vw_SachMuonNhieuNhatThang AS
SELECT TOP 10
    s.MaSach, s.TenSach, tg.TenTacGia, tl.TenTheLoai,
    COUNT(ct.MaCT) AS SoLuotMuon,
    MONTH(GETDATE()) AS Thang,
    YEAR(GETDATE())  AS Nam
FROM CTPhieuMuon ct
JOIN PhieuMuon pm ON ct.MaPhieuMuon = pm.MaPhieuMuon
JOIN Sach s        ON ct.MaSach      = s.MaSach
JOIN TacGia tg     ON s.MaTacGia     = tg.MaTacGia
JOIN TheLoai tl    ON s.MaTheLoai    = tl.MaTheLoai
WHERE MONTH(pm.NgayMuon) = MONTH(GETDATE())
  AND YEAR(pm.NgayMuon)  = YEAR(GETDATE())
GROUP BY s.MaSach, s.TenSach, tg.TenTacGia, tl.TenTheLoai
ORDER BY SoLuotMuon DESC;
GO

-- V4: Danh sách phiếu mượn quá hạn
CREATE OR ALTER VIEW vw_PhieuMuonQuaHan AS
SELECT
    pm.MaPhieuMuon, pm.NgayMuon, pm.NgayHanTra,
    dg.MaDocGia, dg.HoTen, dg.Lop, dg.Email, dg.SoDienThoai,
    DATEDIFF(DAY, pm.NgayHanTra, CAST(GETDATE() AS DATE)) AS SoNgayTre,
    DATEDIFF(DAY, pm.NgayHanTra, CAST(GETDATE() AS DATE)) *
        CAST((SELECT GiaTri FROM CauHinhHeThong WHERE TenCauHinh = N'MucPhatNgayTreHan') AS DECIMAL) AS TienPhatUocTinh
FROM PhieuMuon pm
JOIN DocGia dg ON pm.MaDocGia = dg.MaDocGia
WHERE pm.TrangThai IN (1, 4)
  AND pm.NgayHanTra < CAST(GETDATE() AS DATE);
GO

-- V5: Lịch sử mượn trả của độc giả
CREATE OR ALTER VIEW vw_LichSuMuonTra AS
SELECT
    pm.MaPhieuMuon, pm.NgayMuon, pm.NgayHanTra,
    dg.MaDocGia, dg.HoTen, dg.Lop,
    s.MaSach, s.TenSach, ct.SoLuongMuon,
    pt.NgayTra, pt.TienPhat, pt.TrangThaiSach,
    CASE pt.TrangThaiSach
        WHEN 1 THEN N'Bình thường'
        WHEN 2 THEN N'Hư hỏng'
        WHEN 3 THEN N'Mất sách'
        ELSE N'Chưa trả'
    END AS TinhTrangTra
FROM PhieuMuon pm
JOIN DocGia dg      ON pm.MaDocGia    = dg.MaDocGia
JOIN CTPhieuMuon ct ON pm.MaPhieuMuon = ct.MaPhieuMuon
JOIN Sach s         ON ct.MaSach      = s.MaSach
LEFT JOIN PhieuTra pt ON pm.MaPhieuMuon = pt.MaPhieuMuon;
GO

-- ============================================================
-- TRIGGERS
-- ============================================================

-- T1: Cập nhật trạng thái Quá hạn khi query (dùng INSTEAD OF không phù hợp,
--     nên dùng SP sp_CapNhatTrangThaiQuaHan gọi hàng ngày qua job)
--     Trigger này tự động cập nhật khi INSERT vào PhieuTra
CREATE OR ALTER TRIGGER trg_PhieuTra_CapNhatSoLuongTon
ON PhieuTra
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    -- Chỉ hoàn trả tồn kho khi sách không bị mất (TrangThaiSach <> 3)
    UPDATE s
    SET s.SoLuongTon = s.SoLuongTon + ct.SoLuongMuon
    FROM Sach s
    JOIN CTPhieuMuon ct ON s.MaSach = ct.MaSach
    JOIN inserted i     ON ct.MaPhieuMuon = i.MaPhieuMuon
    WHERE i.TrangThaiSach <> 3;

    -- Đánh dấu chi tiết phiếu mượn đã trả
    UPDATE ct SET ct.TrangThaiCT = 2
    FROM CTPhieuMuon ct
    JOIN inserted i ON ct.MaPhieuMuon = i.MaPhieuMuon;

    -- Đánh dấu phiếu mượn đã trả
    UPDATE pm SET pm.TrangThai = 2
    FROM PhieuMuon pm
    JOIN inserted i ON pm.MaPhieuMuon = i.MaPhieuMuon;
END;
GO

-- T2: Ngăn xóa sách đang được mượn
CREATE OR ALTER TRIGGER trg_Sach_NgaXoaKhiDangMuon
ON Sach
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM CTPhieuMuon ct
        JOIN inserted i ON ct.MaSach = i.MaSach
        WHERE ct.TrangThaiCT = 1
    )
    BEGIN
        THROW 50030, N'Không thể xóa sách đang được mượn. Hãy đổi trạng thái thành Ngừng lưu hành.', 1;
    END

    -- Nếu không có ai đang mượn thì cho xóa (soft delete)
    UPDATE Sach SET TrangThai = 0
    WHERE MaSach IN (SELECT MaSach FROM inserted);
END;
GO

-- T3: Tự động cập nhật trạng thái Quá hạn khi đọc phiếu mượn
--     (chạy qua stored procedure, không phải trigger — trigger SELECT không hợp lệ trong SQL Server)
--     Thay vào đó: SP dưới đây gọi qua Hangfire/scheduled job hàng ngày
CREATE OR ALTER PROCEDURE sp_CapNhatQuaHan
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PhieuMuon
    SET TrangThai = 3   -- Quá hạn
    WHERE TrangThai = 1
      AND NgayHanTra < CAST(GETDATE() AS DATE);

    SELECT @@ROWCOUNT AS SoPhieuCapNhat;
END;
GO

-- ============================================================
-- SEED DATA MẪU
-- ============================================================

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

-- Tài khoản admin mặc định (password: Admin@123 — BCrypt hash ví dụ)
INSERT INTO TaiKhoan (MaVaiTro, TenDangNhap, MatKhau, HoTen, Email) VALUES
    (1, 'admin', '$2a$11$examplehashforadmin123456789012345678', N'Quản trị viên', N'admin@thuvien.edu.vn'),
    (2, 'nhanvien01', '$2a$11$examplehashfornhanvien01234567890', N'Nguyễn Thị Hoa', N'hoa.nguyen@thuvien.edu.vn');
GO
