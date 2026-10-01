-- ============================================================
-- DASHBOARD — bổ sung 3 danh sách cho màn hình Dashboard
--   1. sp_Dashboard_SapDenHan  : phiếu mượn sắp đến hạn trả
--   2. sp_Dashboard_TopSach    : Top N sách được mượn nhiều nhất
--   3. sp_Dashboard_TopDocGia  : Top N độc giả mượn nhiều nhất
-- Chạy sau 01 → 03. Chạy lại nhiều lần vẫn an toàn.
-- ============================================================

USE QuanLyThuVien;
GO

-- Index hỗ trợ lọc phiếu chưa trả theo hạn trả
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_PM_TrangThai_NgayHanTra' AND object_id = OBJECT_ID('PhieuMuon'))
    CREATE INDEX IX_PM_TrangThai_NgayHanTra
        ON PhieuMuon(TrangThai, NgayHanTra) INCLUDE (MaDocGia, NgayMuon);
GO

-- ============================================================
-- 1. Phiếu mượn sắp đến hạn trả (từ hôm nay đến hôm nay + @SoNgay)
--    TongSo = tổng số phiếu thỏa điều kiện (trước khi cắt TOP)
-- ============================================================
CREATE OR ALTER PROCEDURE sp_Dashboard_SapDenHan
    @SoNgay INT = 7,
    @TopN   INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @HomNay  DATE = CAST(GETDATE() AS DATE);
    DECLARE @DenNgay DATE = DATEADD(DAY, @SoNgay, @HomNay);

    SELECT TOP (@TopN)
        pm.MaPhieuMuon, pm.NgayMuon, pm.NgayHanTra, pm.TrangThai,
        dg.MaDocGia, dg.HoTen, dg.Lop, dg.SoDienThoai,
        DATEDIFF(DAY, @HomNay, pm.NgayHanTra)                         AS SoNgayConLai,
        SUM(ct.SoLuongMuon)                                           AS SoCuon,
        STRING_AGG(CAST(s.TenSach AS NVARCHAR(MAX)), N', ')
            WITHIN GROUP (ORDER BY s.TenSach)                         AS DanhSachSach,
        COUNT(*) OVER ()                                              AS TongSo
    FROM PhieuMuon pm
    JOIN DocGia dg      ON dg.MaDocGia    = pm.MaDocGia
    JOIN CTPhieuMuon ct ON ct.MaPhieuMuon = pm.MaPhieuMuon
    JOIN Sach s         ON s.MaSach       = ct.MaSach
    WHERE pm.TrangThai IN (1, 4)
      AND pm.NgayHanTra BETWEEN @HomNay AND @DenNgay
      AND ct.TrangThaiCT = 1
    GROUP BY pm.MaPhieuMuon, pm.NgayMuon, pm.NgayHanTra, pm.TrangThai,
             dg.MaDocGia, dg.HoTen, dg.Lop, dg.SoDienThoai
    ORDER BY pm.NgayHanTra, pm.MaPhieuMuon;
END;
GO

-- ============================================================
-- 2. Top N sách được mượn nhiều nhất
--    @TuNgay / @DenNgay = NULL → không giới hạn thời gian
-- ============================================================
CREATE OR ALTER PROCEDURE sp_Dashboard_TopSach
    @TopN    INT  = 10,
    @TuNgay  DATE = NULL,
    @DenNgay DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@TopN)
        s.MaSach, s.TenSach, s.MaQR, tg.TenTacGia, tl.TenTheLoai,
        s.SoLuongNhap, s.SoLuongTon,
        COUNT(DISTINCT ct.MaPhieuMuon) AS SoLuotMuon,
        SUM(ct.SoLuongMuon)            AS TongCuonMuon
    FROM CTPhieuMuon ct
    JOIN PhieuMuon pm ON pm.MaPhieuMuon = ct.MaPhieuMuon
    JOIN Sach s       ON s.MaSach       = ct.MaSach
    JOIN TacGia tg    ON tg.MaTacGia    = s.MaTacGia
    JOIN TheLoai tl   ON tl.MaTheLoai   = s.MaTheLoai
    WHERE (@TuNgay  IS NULL OR pm.NgayMuon >= @TuNgay)
      AND (@DenNgay IS NULL OR pm.NgayMuon <= @DenNgay)
    GROUP BY s.MaSach, s.TenSach, s.MaQR, tg.TenTacGia, tl.TenTheLoai,
             s.SoLuongNhap, s.SoLuongTon
    ORDER BY SoLuotMuon DESC, TongCuonMuon DESC, s.TenSach
    OPTION (RECOMPILE);
END;
GO

-- ============================================================
-- 3. Top N độc giả mượn nhiều nhất
--    SoLanMuon      = số phiếu mượn
--    TongSachMuon   = tổng số cuốn đã mượn
--    SoPhieuChuaTra = số phiếu đang mượn / quá hạn / đã gia hạn
-- ============================================================
CREATE OR ALTER PROCEDURE sp_Dashboard_TopDocGia
    @TopN    INT  = 10,
    @TuNgay  DATE = NULL,
    @DenNgay DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    WITH PhieuLoc AS (
        SELECT pm.MaPhieuMuon, pm.MaDocGia, pm.NgayMuon, pm.TrangThai,
               (SELECT SUM(ct.SoLuongMuon) FROM CTPhieuMuon ct
                WHERE ct.MaPhieuMuon = pm.MaPhieuMuon) AS SoCuon
        FROM PhieuMuon pm
        WHERE (@TuNgay  IS NULL OR pm.NgayMuon >= @TuNgay)
          AND (@DenNgay IS NULL OR pm.NgayMuon <= @DenNgay)
    )
    SELECT TOP (@TopN)
        dg.MaDocGia, dg.HoTen, dg.Lop,
        COUNT(*)                                                     AS SoLanMuon,
        ISNULL(SUM(p.SoCuon), 0)                                     AS TongSachMuon,
        SUM(CASE WHEN p.TrangThai IN (1, 3, 4) THEN 1 ELSE 0 END)    AS SoPhieuChuaTra,
        MAX(p.NgayMuon)                                              AS LanMuonGanNhat
    FROM PhieuLoc p
    JOIN DocGia dg ON dg.MaDocGia = p.MaDocGia
    GROUP BY dg.MaDocGia, dg.HoTen, dg.Lop
    ORDER BY SoLanMuon DESC, TongSachMuon DESC, dg.HoTen
    OPTION (RECOMPILE)
END;
GO

-- ============================================================
-- Kiểm tra nhanh
-- ============================================================
-- EXEC sp_Dashboard_SapDenHan @SoNgay = 7, @TopN = 10;
-- EXEC sp_Dashboard_TopSach   @TopN = 10;
-- EXEC sp_Dashboard_TopDocGia @TopN = 10, @TuNgay = '2026-01-01', @DenNgay = '2026-12-31';
