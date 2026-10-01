-- ============================================================
-- DATABASE: QuanLyThuVien_THCS_ThanhTuan
-- Encoding: UTF-8 | Collation: Vietnamese_CI_AS
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyThuVien')
    DROP DATABASE QuanLyThuVien;
GO

CREATE DATABASE QuanLyThuVien
    COLLATE Vietnamese_CI_AS;
GO

USE QuanLyThuVien;
GO

-- ============================================================
-- LOOKUP TABLES
-- ============================================================

CREATE TABLE TheLoai (
    MaTheLoai   INT             IDENTITY(1,1) PRIMARY KEY,
    TenTheLoai  NVARCHAR(100)   NOT NULL,
    MoTa        NVARCHAR(500)   NULL,
    NgayTao     DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_TheLoai_Ten UNIQUE (TenTheLoai)
);

CREATE TABLE TacGia (
    MaTacGia    INT             IDENTITY(1,1) PRIMARY KEY,
    TenTacGia   NVARCHAR(150)   NOT NULL,
    QuocTich    NVARCHAR(100)   NULL,
    GhiChu      NVARCHAR(500)   NULL,
    CONSTRAINT UQ_TacGia_Ten UNIQUE (TenTacGia)
);

CREATE TABLE NhaXuatBan (
    MaNXB       INT             IDENTITY(1,1) PRIMARY KEY,
    TenNXB      NVARCHAR(150)   NOT NULL,
    DiaChi      NVARCHAR(300)   NULL,
    DienThoai   VARCHAR(20)     NULL,
    CONSTRAINT UQ_NXB_Ten UNIQUE (TenNXB)
);

-- ============================================================
-- CORE TABLES
-- ============================================================

CREATE TABLE Sach (
    MaSach          INT             IDENTITY(1,1) PRIMARY KEY,
    MaTheLoai       INT             NOT NULL,
    MaTacGia        INT             NOT NULL,
    MaNXB           INT             NOT NULL,
    TenSach         NVARCHAR(300)   NOT NULL,
    NamXuatBan      SMALLINT        NULL,
    SoTrang         SMALLINT        NULL,
    SoLuongNhap     INT             NOT NULL DEFAULT 0,
    SoLuongTon      INT             NOT NULL DEFAULT 0,
    ViTri           NVARCHAR(50)    NULL,   -- kệ sách
    MaQR            VARCHAR(100)    NULL,   -- mã QR unique
    MoTa            NVARCHAR(1000)  NULL,
    NgayNhap        DATE            NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    TrangThai       TINYINT         NOT NULL DEFAULT 1, -- 1=Có sẵn, 0=Ngừng lưu hành
    CONSTRAINT FK_Sach_TheLoai  FOREIGN KEY (MaTheLoai)  REFERENCES TheLoai(MaTheLoai),
    CONSTRAINT FK_Sach_TacGia   FOREIGN KEY (MaTacGia)   REFERENCES TacGia(MaTacGia),
    CONSTRAINT FK_Sach_NXB      FOREIGN KEY (MaNXB)      REFERENCES NhaXuatBan(MaNXB),
    CONSTRAINT CHK_Sach_SoLuong CHECK (SoLuongTon >= 0 AND SoLuongNhap >= 0),
    CONSTRAINT UQ_Sach_MaQR     UNIQUE (MaQR)
);

CREATE TABLE DocGia (
    MaDocGia    INT             IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(150)   NOT NULL,
    Lop         NVARCHAR(20)    NULL,
    NgaySinh    DATE            NULL,
    GioiTinh    BIT             NULL,   -- 1=Nam, 0=Nữ
    DiaChi      NVARCHAR(300)   NULL,
    Email       VARCHAR(150)    NULL,
    SoDienThoai VARCHAR(15)     NULL,
    NgayDangKy  DATE            NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    TrangThai   TINYINT         NOT NULL DEFAULT 1 -- 1=Hoạt động, 0=Khóa
);

-- UNIQUE constraint chỉ cho phép một giá trị NULL, nên dùng filtered index
CREATE UNIQUE INDEX UQ_DocGia_Email ON DocGia(Email) WHERE Email IS NOT NULL;

CREATE TABLE CauHinhHeThong (
    MaCauHinh       INT             IDENTITY(1,1) PRIMARY KEY,
    TenCauHinh      NVARCHAR(100)   NOT NULL,
    GiaTri          NVARCHAR(200)   NOT NULL,
    GhiChu          NVARCHAR(300)   NULL,
    CONSTRAINT UQ_CauHinh_Ten UNIQUE (TenCauHinh)
);

-- Seed cấu hình mặc định
INSERT INTO CauHinhHeThong (TenCauHinh, GiaTri, GhiChu) VALUES
    (N'SoNgayMuonMacDinh',  '14',    N'Số ngày mượn tối đa'),
    (N'MucPhatNgayTreHan',  '2000',  N'Tiền phạt mỗi ngày trễ (VND)'),
    (N'SoSachMuonToiDa',    '3',     N'Số sách một độc giả được mượn đồng thời'),
    (N'SoLanGiaHanToiDa',   '2',     N'Số lần gia hạn tối đa mỗi phiếu mượn'),
    (N'SoNgayGiaHanMoiLan', '7',     N'Số ngày gia hạn thêm mỗi lần');
GO

-- ============================================================
-- BORROWING / RETURNING
-- ============================================================

CREATE TABLE PhieuMuon (
    MaPhieuMuon     INT             IDENTITY(1,1) PRIMARY KEY,
    MaDocGia        INT             NOT NULL,
    NgayMuon        DATE            NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    NgayHanTra      DATE            NOT NULL,
    TrangThai       TINYINT         NOT NULL DEFAULT 1,
    -- 1=Đang mượn, 2=Đã trả hết, 3=Quá hạn, 4=Đã gia hạn
    GhiChu          NVARCHAR(500)   NULL,
    NhanVienLap     INT             NULL,   -- FK TaiKhoan
    CONSTRAINT FK_PhieuMuon_DocGia FOREIGN KEY (MaDocGia) REFERENCES DocGia(MaDocGia),
    CONSTRAINT CHK_PhieuMuon_Han   CHECK (NgayHanTra > NgayMuon)
);

CREATE TABLE CTPhieuMuon (
    MaCT            INT             IDENTITY(1,1) PRIMARY KEY,
    MaPhieuMuon     INT             NOT NULL,
    MaSach          INT             NOT NULL,
    SoLuongMuon     INT             NOT NULL DEFAULT 1,
    TrangThaiCT     TINYINT         NOT NULL DEFAULT 1, -- 1=Đang mượn, 2=Đã trả
    CONSTRAINT FK_CT_PhieuMuon  FOREIGN KEY (MaPhieuMuon) REFERENCES PhieuMuon(MaPhieuMuon),
    CONSTRAINT FK_CT_Sach       FOREIGN KEY (MaSach)      REFERENCES Sach(MaSach),
    CONSTRAINT CHK_CT_SoLuong   CHECK (SoLuongMuon > 0),
    CONSTRAINT UQ_CT_Unique     UNIQUE (MaPhieuMuon, MaSach)
);

CREATE TABLE GiaHan (
    MaGiaHan        INT             IDENTITY(1,1) PRIMARY KEY,
    MaPhieuMuon     INT             NOT NULL,
    NgayGiaHan      DATE            NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    HanTraCu        DATE            NOT NULL,
    HanTraMoi       DATE            NOT NULL,
    LanGiaHan       TINYINT         NOT NULL DEFAULT 1,
    NhanVienDuyet   INT             NULL,
    GhiChu          NVARCHAR(300)   NULL,
    CONSTRAINT FK_GiaHan_PhieuMuon FOREIGN KEY (MaPhieuMuon) REFERENCES PhieuMuon(MaPhieuMuon),
    CONSTRAINT CHK_GiaHan_HanTra   CHECK (HanTraMoi > HanTraCu)
);

CREATE TABLE PhieuTra (
    MaPhieuTra      INT             IDENTITY(1,1) PRIMARY KEY,
    MaPhieuMuon     INT             NOT NULL,
    NgayTra         DATE            NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    SoNgayMuon      INT             NOT NULL DEFAULT 0,
    SoNgayTreHan    INT             NOT NULL DEFAULT 0,
    TienPhat        DECIMAL(12,0)   NOT NULL DEFAULT 0,
    DaThuPhat       BIT             NOT NULL DEFAULT 0,
    TrangThaiSach   TINYINT         NOT NULL DEFAULT 1, -- 1=Bình thường, 2=Hư hỏng, 3=Mất
    GhiChu          NVARCHAR(500)   NULL,
    NhanVienThu     INT             NULL,
    CONSTRAINT FK_PhieuTra_PhieuMuon FOREIGN KEY (MaPhieuMuon) REFERENCES PhieuMuon(MaPhieuMuon),
    CONSTRAINT CHK_PhieuTra_Tien     CHECK (TienPhat >= 0)
);

-- ============================================================
-- USER MANAGEMENT
-- ============================================================

CREATE TABLE VaiTro (
    MaVaiTro    INT             IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro   NVARCHAR(50)    NOT NULL,   -- Admin, NhanVien, DocGia
    MoTa        NVARCHAR(200)   NULL,
    CONSTRAINT UQ_VaiTro_Ten UNIQUE (TenVaiTro)
);

INSERT INTO VaiTro (TenVaiTro, MoTa) VALUES
    (N'Admin',      N'Quản trị hệ thống, toàn quyền'),
    (N'NhanVien',   N'Nhân viên thư viện'),
    (N'DocGia',     N'Độc giả tra cứu');

CREATE TABLE TaiKhoan (
    MaTaiKhoan      INT             IDENTITY(1,1) PRIMARY KEY,
    MaVaiTro        INT             NOT NULL,
    MaDocGia        INT             NULL,   -- liên kết nếu là độc giả
    TenDangNhap     VARCHAR(50)     NOT NULL,
    MatKhau         VARCHAR(255)    NOT NULL,   -- BCrypt hash
    HoTen           NVARCHAR(150)   NOT NULL,
    Email           VARCHAR(150)    NULL,
    SoDienThoai     VARCHAR(15)     NULL,
    NgayTao         DATETIME2       NOT NULL DEFAULT GETDATE(),
    LanDangNhapCuoi DATETIME2       NULL,
    TrangThai       TINYINT         NOT NULL DEFAULT 1,
    CONSTRAINT FK_TK_VaiTro     FOREIGN KEY (MaVaiTro)  REFERENCES VaiTro(MaVaiTro),
    CONSTRAINT FK_TK_DocGia     FOREIGN KEY (MaDocGia)  REFERENCES DocGia(MaDocGia),
    CONSTRAINT UQ_TK_TenDN      UNIQUE (TenDangNhap)
);

CREATE UNIQUE INDEX UQ_TK_Email ON TaiKhoan(Email) WHERE Email IS NOT NULL;

-- ============================================================
-- INDEXES — tối ưu tìm kiếm và join
-- ============================================================

-- Sách
CREATE INDEX IX_Sach_TenSach      ON Sach(TenSach);
CREATE INDEX IX_Sach_MaTheLoai    ON Sach(MaTheLoai);
CREATE INDEX IX_Sach_MaTacGia     ON Sach(MaTacGia);
CREATE INDEX IX_Sach_MaNXB        ON Sach(MaNXB);
CREATE INDEX IX_Sach_TrangThai    ON Sach(TrangThai);
CREATE INDEX IX_Sach_MaQR         ON Sach(MaQR);

-- DocGia
CREATE INDEX IX_DocGia_HoTen      ON DocGia(HoTen);
CREATE INDEX IX_DocGia_Lop        ON DocGia(Lop);

-- PhieuMuon
CREATE INDEX IX_PM_MaDocGia       ON PhieuMuon(MaDocGia);
CREATE INDEX IX_PM_NgayMuon       ON PhieuMuon(NgayMuon);
CREATE INDEX IX_PM_NgayHanTra     ON PhieuMuon(NgayHanTra);
CREATE INDEX IX_PM_TrangThai      ON PhieuMuon(TrangThai);

-- CTPhieuMuon
CREATE INDEX IX_CT_MaSach         ON CTPhieuMuon(MaSach);

-- PhieuTra
CREATE INDEX IX_PhieuTra_NgayTra  ON PhieuTra(NgayTra);

GO
