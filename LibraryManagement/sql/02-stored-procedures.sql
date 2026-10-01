-- ============================================================
-- STORED PROCEDURES — QuanLyThuVien
-- ============================================================

USE QuanLyThuVien;
GO

-- ============================================================
-- SP 1: Lập phiếu mượn (transaction, kiểm tra tồn kho)
-- ============================================================
CREATE OR ALTER PROCEDURE sp_LapPhieuMuon
    @MaDocGia       INT,
    @DanhSachSach   NVARCHAR(MAX),  -- JSON: [{"MaSach":1,"SoLuong":1},...]
    @NhanVienLap    INT,
    @MaPhieuMuon    INT OUTPUT,
    @ThoiDiemLap    DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Lấy cấu hình
        DECLARE @SoNgayMuon  INT, @SoSachToiDa INT, @SoDangMuon INT;
        SELECT @SoNgayMuon = CAST(GiaTri AS INT) FROM CauHinhHeThong WHERE TenCauHinh = N'SoNgayMuonMacDinh';
        SELECT @SoSachToiDa = CAST(GiaTri AS INT) FROM CauHinhHeThong WHERE TenCauHinh = N'SoSachMuonToiDa';

        -- Kiểm tra trạng thái độc giả
        IF NOT EXISTS (SELECT 1 FROM DocGia WHERE MaDocGia = @MaDocGia AND TrangThai = 1)
            THROW 50001, N'Độc giả không tồn tại hoặc đã bị khóa.', 1;

        -- Kiểm tra số sách đang mượn
        SELECT @SoDangMuon = ISNULL(COUNT(*), 0)
        FROM CTPhieuMuon ct
        JOIN PhieuMuon pm ON ct.MaPhieuMuon = pm.MaPhieuMuon
        WHERE pm.MaDocGia = @MaDocGia AND pm.TrangThai IN (1, 4) AND ct.TrangThaiCT = 1;

        -- Đếm sách trong request
        DECLARE @SoSachMuon INT;
        SELECT @SoSachMuon = COUNT(*) FROM OPENJSON(@DanhSachSach)
            WITH (MaSach INT '$.MaSach', SoLuong INT '$.SoLuong');

        IF (@SoDangMuon + @SoSachMuon) > @SoSachToiDa
            THROW 50002, N'Độc giả đã đạt giới hạn số sách mượn tối đa.', 1;

        -- Kiểm tra tồn kho từng cuốn
        IF EXISTS (
            SELECT 1 FROM OPENJSON(@DanhSachSach)
                WITH (MaSach INT '$.MaSach', SoLuong INT '$.SoLuong') j
            JOIN Sach s ON j.MaSach = s.MaSach
            WHERE s.SoLuongTon < j.SoLuong OR s.TrangThai = 0
        )
            THROW 50003, N'Một hoặc nhiều sách không đủ số lượng hoặc đã ngừng lưu hành.', 1;

        -- Tạo phiếu mượn
        DECLARE @NgayMuon DATE = CAST(ISNULL(@ThoiDiemLap, GETDATE()) AS DATE);
        INSERT INTO PhieuMuon (MaDocGia, NgayMuon, NgayHanTra, TrangThai, NhanVienLap)
        VALUES (@MaDocGia, @NgayMuon, DATEADD(DAY, @SoNgayMuon, @NgayMuon), 1, @NhanVienLap);

        SET @MaPhieuMuon = SCOPE_IDENTITY();

        -- Thêm chi tiết + trừ tồn kho
        INSERT INTO CTPhieuMuon (MaPhieuMuon, MaSach, SoLuongMuon)
        SELECT @MaPhieuMuon, j.MaSach, j.SoLuong
        FROM OPENJSON(@DanhSachSach)
            WITH (MaSach INT '$.MaSach', SoLuong INT '$.SoLuong') j;

        UPDATE s SET s.SoLuongTon = s.SoLuongTon - j.SoLuong
        FROM Sach s
        JOIN OPENJSON(@DanhSachSach) WITH (MaSach INT '$.MaSach', SoLuong INT '$.SoLuong') j
            ON s.MaSach = j.MaSach;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ============================================================
-- SP 2: Gia hạn mượn sách
-- ============================================================
CREATE OR ALTER PROCEDURE sp_GiaHanPhieuMuon
    @MaPhieuMuon    INT,
    @NhanVienDuyet  INT,
    @GhiChu         NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SoLanGiaHanToiDa INT, @SoNgayGiaHan INT, @LanGiaHanHienTai INT;
        DECLARE @NgayHanTraCu DATE;

        SELECT @SoLanGiaHanToiDa = CAST(GiaTri AS INT) FROM CauHinhHeThong WHERE TenCauHinh = N'SoLanGiaHanToiDa';
        SELECT @SoNgayGiaHan = CAST(GiaTri AS INT) FROM CauHinhHeThong WHERE TenCauHinh = N'SoNgayGiaHanMoiLan';

        SELECT @LanGiaHanHienTai = ISNULL(COUNT(*), 0)
        FROM GiaHan WHERE MaPhieuMuon = @MaPhieuMuon;

        IF @LanGiaHanHienTai >= @SoLanGiaHanToiDa
            THROW 50010, N'Đã đạt số lần gia hạn tối đa.', 1;

        SELECT @NgayHanTraCu = NgayHanTra FROM PhieuMuon
        WHERE MaPhieuMuon = @MaPhieuMuon AND TrangThai IN (1, 4);

        IF @NgayHanTraCu IS NULL
            THROW 50011, N'Phiếu mượn không tồn tại hoặc không thể gia hạn.', 1;

        IF @NgayHanTraCu < CAST(GETDATE() AS DATE)
            THROW 50012, N'Phiếu mượn đã quá hạn, không thể gia hạn.', 1;

        DECLARE @NgayHanTraMoi DATE = DATEADD(DAY, @SoNgayGiaHan, @NgayHanTraCu);

        INSERT INTO GiaHan (MaPhieuMuon, NgayGiaHan, HanTraCu, HanTraMoi, LanGiaHan, NhanVienDuyet, GhiChu)
        VALUES (@MaPhieuMuon, CAST(GETDATE() AS DATE), @NgayHanTraCu, @NgayHanTraMoi,
                @LanGiaHanHienTai + 1, @NhanVienDuyet, @GhiChu);

        UPDATE PhieuMuon SET NgayHanTra = @NgayHanTraMoi, TrangThai = 4
        WHERE MaPhieuMuon = @MaPhieuMuon;

        COMMIT TRANSACTION;
        SELECT @NgayHanTraMoi AS NgayHanTraMoi;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ============================================================
-- SP 3: Xử lý trả sách
-- ============================================================
CREATE OR ALTER PROCEDURE sp_TraSach
    @MaPhieuMuon    INT,
    @TrangThaiSach  TINYINT = 1,    -- 1=Bình thường, 2=Hư hỏng, 3=Mất
    @NhanVienThu    INT,
    @GhiChu         NVARCHAR(500) = NULL,
    @MaPhieuTra     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @MucPhat DECIMAL(12,0), @NgayHanTra DATE, @NgayTra DATE;
        DECLARE @SoNgayTreHan INT, @TienPhat DECIMAL(12,0);

        SELECT @MucPhat = CAST(GiaTri AS DECIMAL) FROM CauHinhHeThong WHERE TenCauHinh = N'MucPhatNgayTreHan';

        SELECT @NgayHanTra = NgayHanTra FROM PhieuMuon
        WHERE MaPhieuMuon = @MaPhieuMuon AND TrangThai IN (1, 3, 4);

        IF @NgayHanTra IS NULL
            THROW 50020, N'Phiếu mượn không tồn tại hoặc đã được trả.', 1;

        SET @NgayTra = CAST(GETDATE() AS DATE);
        SET @SoNgayTreHan = CASE WHEN @NgayTra > @NgayHanTra
                                 THEN DATEDIFF(DAY, @NgayHanTra, @NgayTra) ELSE 0 END;
        SET @TienPhat = @SoNgayTreHan * @MucPhat;

        -- Phạt thêm nếu sách hư/mất (tạm tính gấp đôi, có thể cấu hình)
        IF @TrangThaiSach = 2 SET @TienPhat = @TienPhat + 50000;
        IF @TrangThaiSach = 3 SET @TienPhat = @TienPhat + 200000;

        INSERT INTO PhieuTra (MaPhieuMuon, NgayTra, SoNgayMuon, SoNgayTreHan,
                              TienPhat, TrangThaiSach, GhiChu, NhanVienThu)
        VALUES (@MaPhieuMuon, @NgayTra,
                DATEDIFF(DAY, (SELECT NgayMuon FROM PhieuMuon WHERE MaPhieuMuon = @MaPhieuMuon), @NgayTra),
                @SoNgayTreHan, @TienPhat, @TrangThaiSach, @GhiChu, @NhanVienThu);

        SET @MaPhieuTra = SCOPE_IDENTITY();

        -- Cập nhật trạng thái phiếu và chi tiết
        UPDATE PhieuMuon SET TrangThai = 2 WHERE MaPhieuMuon = @MaPhieuMuon;
        UPDATE CTPhieuMuon SET TrangThaiCT = 2 WHERE MaPhieuMuon = @MaPhieuMuon;

        -- Hoàn trả tồn kho (trừ sách mất)
        IF @TrangThaiSach <> 3
            UPDATE s SET s.SoLuongTon = s.SoLuongTon + ct.SoLuongMuon
            FROM Sach s
            JOIN CTPhieuMuon ct ON s.MaSach = ct.MaSach
            WHERE ct.MaPhieuMuon = @MaPhieuMuon;

        COMMIT TRANSACTION;
        SELECT @MaPhieuTra AS MaPhieuTra, @TienPhat AS TienPhat, @SoNgayTreHan AS SoNgayTreHan;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ============================================================
-- SP 4: Tìm kiếm sách (đa tiêu chí)
-- ============================================================
CREATE OR ALTER PROCEDURE sp_TimKiemSach
    @TuKhoa     NVARCHAR(300) = NULL,
    @MaTheLoai  INT = NULL,
    @MaTacGia   INT = NULL,
    @MaNXB      INT = NULL,
    @ChiConTon  BIT = 0,        -- 1 = chỉ lấy sách còn tồn kho
    @PageIndex  INT = 1,
    @PageSize   INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    WITH CTE AS (
        SELECT
            s.MaSach, s.TenSach, tg.TenTacGia, tl.TenTheLoai,
            nxb.TenNXB, s.NamXuatBan, s.SoLuongTon, s.MaQR, s.ViTri,
            ROW_NUMBER() OVER (ORDER BY s.TenSach) AS RowNum,
            COUNT(*) OVER () AS TongSo
        FROM Sach s
        JOIN TheLoai tl   ON s.MaTheLoai = tl.MaTheLoai
        JOIN TacGia tg    ON s.MaTacGia  = tg.MaTacGia
        JOIN NhaXuatBan nxb ON s.MaNXB   = nxb.MaNXB
        WHERE s.TrangThai = 1
          AND (@TuKhoa    IS NULL OR s.TenSach LIKE N'%' + @TuKhoa + '%'
                                  OR tg.TenTacGia LIKE N'%' + @TuKhoa + '%'
                                  OR s.MaQR = @TuKhoa)
          AND (@MaTheLoai IS NULL OR s.MaTheLoai = @MaTheLoai)
          AND (@MaTacGia  IS NULL OR s.MaTacGia  = @MaTacGia)
          AND (@MaNXB     IS NULL OR s.MaNXB     = @MaNXB)
          AND (@ChiConTon = 0     OR s.SoLuongTon > 0)
    )
    SELECT * FROM CTE
    WHERE RowNum BETWEEN ((@PageIndex - 1) * @PageSize + 1) AND (@PageIndex * @PageSize);
END;
GO

-- ============================================================
-- SP 5: Thống kê sách mượn nhiều nhất trong tháng
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ThongKeSachMuonNhieu
    @Thang  INT = NULL,
    @Nam    INT = NULL,
    @TopN   INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @Thang = ISNULL(@Thang, MONTH(GETDATE()));
    SELECT @Nam   = ISNULL(@Nam,   YEAR(GETDATE()));

    SELECT TOP (@TopN)
        s.MaSach, s.TenSach, tg.TenTacGia, tl.TenTheLoai,
        COUNT(ct.MaCT) AS SoLuotMuon
    FROM CTPhieuMuon ct
    JOIN PhieuMuon pm ON ct.MaPhieuMuon = pm.MaPhieuMuon
    JOIN Sach s        ON ct.MaSach      = s.MaSach
    JOIN TacGia tg     ON s.MaTacGia     = tg.MaTacGia
    JOIN TheLoai tl    ON s.MaTheLoai    = tl.MaTheLoai
    WHERE MONTH(pm.NgayMuon) = @Thang AND YEAR(pm.NgayMuon) = @Nam
    GROUP BY s.MaSach, s.TenSach, tg.TenTacGia, tl.TenTheLoai
    ORDER BY SoLuotMuon DESC;
END;
GO

-- ============================================================
-- SP 6: Thống kê độc giả mượn nhiều nhất trong tháng
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ThongKeDocGiaMuonNhieu
    @Thang  INT = NULL,
    @Nam    INT = NULL,
    @TopN   INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @Thang = ISNULL(@Thang, MONTH(GETDATE()));
    SELECT @Nam   = ISNULL(@Nam,   YEAR(GETDATE()));

    SELECT TOP (@TopN)
        dg.MaDocGia, dg.HoTen, dg.Lop, dg.Email,
        COUNT(pm.MaPhieuMuon) AS SoLanMuon,
        SUM(ct.SoLuongMuon)   AS TongSachMuon
    FROM PhieuMuon pm
    JOIN DocGia dg      ON pm.MaDocGia = dg.MaDocGia
    JOIN CTPhieuMuon ct ON pm.MaPhieuMuon = ct.MaPhieuMuon
    WHERE MONTH(pm.NgayMuon) = @Thang AND YEAR(pm.NgayMuon) = @Nam
    GROUP BY dg.MaDocGia, dg.HoTen, dg.Lop, dg.Email
    ORDER BY SoLanMuon DESC;
END;
GO

-- ============================================================
-- SP 7: Lấy sách sắp đến hạn trả (dùng cho email nhắc nhở)
-- ============================================================
CREATE OR ALTER PROCEDURE sp_LayPhieuMuonSapDenHan
    @SoNgayTruoc INT = 3    -- nhắc trước N ngày
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NgayNhac DATE = DATEADD(DAY, @SoNgayTruoc, CAST(GETDATE() AS DATE));
    SELECT
        pm.MaPhieuMuon, pm.NgayHanTra,
        dg.HoTen, dg.Email, dg.Lop,
        DATEDIFF(DAY, CAST(GETDATE() AS DATE), pm.NgayHanTra) AS SoNgayConLai,
        STRING_AGG(s.TenSach, N', ') WITHIN GROUP (ORDER BY s.TenSach) AS DanhSachSach
    FROM PhieuMuon pm
    JOIN DocGia dg      ON pm.MaDocGia    = dg.MaDocGia
    JOIN CTPhieuMuon ct ON pm.MaPhieuMuon = ct.MaPhieuMuon
    JOIN Sach s         ON ct.MaSach      = s.MaSach
    WHERE pm.TrangThai IN (1, 4)
      AND pm.NgayHanTra = @NgayNhac
      AND dg.Email IS NOT NULL
    GROUP BY pm.MaPhieuMuon, pm.NgayHanTra, dg.HoTen, dg.Email, dg.Lop;
END;
GO

-- ============================================================
-- SP 8: Dashboard — tổng quan hệ thống
-- ============================================================
CREATE OR ALTER PROCEDURE sp_Dashboard
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM Sach WHERE TrangThai = 1)                         AS TongDauSach,
        (SELECT SUM(SoLuongTon) FROM Sach WHERE TrangThai = 1)                  AS TongSachTon,
        (SELECT COUNT(*) FROM DocGia WHERE TrangThai = 1)                       AS TongDocGia,
        (SELECT COUNT(*) FROM PhieuMuon WHERE TrangThai IN (1,4))               AS DangMuon,
        (SELECT COUNT(*) FROM PhieuMuon WHERE TrangThai = 1
            AND NgayHanTra < CAST(GETDATE() AS DATE))                           AS QuaHan,
        (SELECT ISNULL(SUM(TienPhat),0) FROM PhieuTra WHERE DaThuPhat = 0)      AS TienPhatChuaThu,
        (SELECT COUNT(*) FROM PhieuMuon
            WHERE MONTH(NgayMuon) = MONTH(GETDATE())
              AND YEAR(NgayMuon) = YEAR(GETDATE()))                              AS MuonTrongThang;
END;
GO
