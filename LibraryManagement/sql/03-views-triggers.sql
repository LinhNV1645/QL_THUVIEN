-- ============================================================
-- VIEWS + TRIGGERS — QuanLyThuVien
-- Script này có thể chạy lại nhiều lần (idempotent).
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
        WHEN pm.TrangThai = 3 THEN N'Quá hạn'
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
WHERE pm.TrangThai IN (1, 3, 4)
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

-- sp_TraSach là nơi duy nhất cập nhật tồn kho và trạng thái khi trả sách.
-- Trigger cũ trên PhieuTra làm tồn kho bị cộng hai lần nên phải xóa.
DROP TRIGGER IF EXISTS trg_PhieuTra_CapNhatSoLuongTon;
GO

-- T1: Ngăn xóa cứng sách — chuyển thành xóa mềm, chặn nếu sách đang được mượn
CREATE OR ALTER TRIGGER trg_Sach_NgaXoaKhiDangMuon
ON Sach
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM CTPhieuMuon ct
        JOIN deleted d ON ct.MaSach = d.MaSach
        WHERE ct.TrangThaiCT = 1
    )
    BEGIN
        THROW 50030, N'Không thể xóa sách đang được mượn. Hãy đổi trạng thái thành Ngừng lưu hành.', 1;
    END

    UPDATE Sach SET TrangThai = 0
    WHERE MaSach IN (SELECT MaSach FROM deleted);
END;
GO

-- ============================================================
-- SP cập nhật trạng thái Quá hạn — được Hangfire gọi hằng ngày (CapNhatQuaHanJob)
-- ============================================================
CREATE OR ALTER PROCEDURE sp_CapNhatQuaHan
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PhieuMuon
    SET TrangThai = 3   -- Quá hạn
    WHERE TrangThai IN (1, 4)
      AND NgayHanTra < CAST(GETDATE() AS DATE);

    SELECT @@ROWCOUNT AS SoPhieuCapNhat;
END;
GO
