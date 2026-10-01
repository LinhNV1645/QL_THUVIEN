-- ============================================================
-- SỬA DỮ LIỆU CHO DATABASE ĐÃ TẠO BẰNG BỘ SCRIPT CŨ
-- Thứ tự: chạy lại 02-stored-procedures.sql, 03-views-triggers.sql, rồi chạy file này.
-- KHÔNG cần chạy file này nếu database được tạo mới từ đầu bằng 01 → 04.
-- Script có thể chạy lại nhiều lần.
-- ============================================================

USE QuanLyThuVien;
GO

-- 1. Email: UNIQUE constraint chỉ cho phép một NULL → đổi sang filtered unique index
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_DocGia_Email')
    ALTER TABLE DocGia DROP CONSTRAINT UQ_DocGia_Email;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_DocGia_Email' AND object_id = OBJECT_ID('DocGia'))
    CREATE UNIQUE INDEX UQ_DocGia_Email ON DocGia(Email) WHERE Email IS NOT NULL;

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_TK_Email')
    ALTER TABLE TaiKhoan DROP CONSTRAINT UQ_TK_Email;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_TK_Email' AND object_id = OBJECT_ID('TaiKhoan'))
    CREATE UNIQUE INDEX UQ_TK_Email ON TaiKhoan(Email) WHERE Email IS NOT NULL;
GO

-- 2. Thay hash mật khẩu giả của tài khoản mẫu bằng hash thật
--    admin / Admin@123     nhanvien01 / NhanVien@123
UPDATE TaiKhoan SET MatKhau = '$2a$11$uUj85gppDR4tvGqJxX4j9.gtFDOd/WJ8FpIBlcc7/QoWPwJ1.hhIa'
WHERE TenDangNhap = 'admin' AND MatKhau LIKE '$2a$11$examplehash%';

UPDATE TaiKhoan SET MatKhau = '$2a$11$JTdwC6swDIGs.pX7gFG1POqiL2tqTcZPNGlup/WOa/K1CYkfOX.Ru'
WHERE TenDangNhap = 'nhanvien01' AND MatKhau LIKE '$2a$11$examplehash%';
GO

-- 3. Tính lại tồn kho (trigger cũ đã cộng tồn kho hai lần mỗi khi trả sách)
--    Tồn = Nhập − đang được mượn − đã mất
UPDATE s SET s.SoLuongTon = CASE WHEN x.Ton < 0 THEN 0 ELSE x.Ton END
FROM Sach s
CROSS APPLY (
    SELECT s.SoLuongNhap
        - ISNULL((SELECT SUM(ct.SoLuongMuon)
                  FROM CTPhieuMuon ct
                  WHERE ct.MaSach = s.MaSach AND ct.TrangThaiCT = 1), 0)
        - ISNULL((SELECT SUM(ct.SoLuongMuon)
                  FROM CTPhieuMuon ct
                  JOIN PhieuTra pt ON pt.MaPhieuMuon = ct.MaPhieuMuon
                  WHERE ct.MaSach = s.MaSach AND pt.TrangThaiSach = 3), 0) AS Ton
) x
WHERE s.SoLuongTon <> CASE WHEN x.Ton < 0 THEN 0 ELSE x.Ton END;

SELECT @@ROWCOUNT AS SoSachDaSuaTonKho;
GO

-- 4. Đánh dấu các phiếu đã quá hạn
EXEC sp_CapNhatQuaHan;
GO
