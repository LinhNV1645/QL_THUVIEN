# Contract: Domain Entities

> **BẮT BUỘC**: Tất cả agents dùng đúng tên field này. KHÔNG tự đặt lại.

---

## Sach
```csharp
public class Sach {
    public int MaSach { get; set; }
    public int MaTheLoai { get; set; }
    public int MaTacGia { get; set; }
    public int MaNXB { get; set; }
    public string TenSach { get; set; } = "";
    public short? NamXuatBan { get; set; }
    public short? SoTrang { get; set; }
    public int SoLuongNhap { get; set; }
    public int SoLuongTon { get; set; }
    public string? ViTri { get; set; }
    public string? MaQR { get; set; }
    public string? MoTa { get; set; }
    public DateOnly NgayNhap { get; set; }
    public byte TrangThai { get; set; } = 1; // 1=Có sẵn, 0=Ngừng
    // Navigation
    public TheLoai TheLoai { get; set; } = null!;
    public TacGia TacGia { get; set; } = null!;
    public NhaXuatBan NhaXuatBan { get; set; } = null!;
    public ICollection<CTPhieuMuon> CTPhieuMuons { get; set; } = [];
}
```

## TheLoai
```csharp
public class TheLoai {
    public int MaTheLoai { get; set; }
    public string TenTheLoai { get; set; } = "";
    public string? MoTa { get; set; }
    public DateTime NgayTao { get; set; }
    public ICollection<Sach> Sachs { get; set; } = [];
}
```

## TacGia
```csharp
public class TacGia {
    public int MaTacGia { get; set; }
    public string TenTacGia { get; set; } = "";
    public string? QuocTich { get; set; }
    public string? GhiChu { get; set; }
    public ICollection<Sach> Sachs { get; set; } = [];
}
```

## NhaXuatBan
```csharp
public class NhaXuatBan {
    public int MaNXB { get; set; }
    public string TenNXB { get; set; } = "";
    public string? DiaChi { get; set; }
    public string? DienThoai { get; set; }
    public ICollection<Sach> Sachs { get; set; } = [];
}
```

## DocGia
```csharp
public class DocGia {
    public int MaDocGia { get; set; }
    public string HoTen { get; set; } = "";
    public string? Lop { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public bool? GioiTinh { get; set; }
    public string? DiaChi { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public DateOnly NgayDangKy { get; set; }
    public byte TrangThai { get; set; } = 1;
    public ICollection<PhieuMuon> PhieuMuons { get; set; } = [];
}
```

## PhieuMuon
```csharp
public class PhieuMuon {
    public int MaPhieuMuon { get; set; }
    public int MaDocGia { get; set; }
    public DateOnly NgayMuon { get; set; }
    public DateOnly NgayHanTra { get; set; }
    public byte TrangThai { get; set; } = 1;
    // 1=Đang mượn, 2=Đã trả, 3=Quá hạn, 4=Đã gia hạn
    public string? GhiChu { get; set; }
    public int? NhanVienLap { get; set; }
    public DocGia DocGia { get; set; } = null!;
    public ICollection<CTPhieuMuon> CTPhieuMuons { get; set; } = [];
    public ICollection<GiaHan> GiaHans { get; set; } = [];
    public PhieuTra? PhieuTra { get; set; }
}
```

## CTPhieuMuon
```csharp
public class CTPhieuMuon {
    public int MaCT { get; set; }
    public int MaPhieuMuon { get; set; }
    public int MaSach { get; set; }
    public int SoLuongMuon { get; set; } = 1;
    public byte TrangThaiCT { get; set; } = 1; // 1=Đang mượn, 2=Đã trả
    public PhieuMuon PhieuMuon { get; set; } = null!;
    public Sach Sach { get; set; } = null!;
}
```

## GiaHan
```csharp
public class GiaHan {
    public int MaGiaHan { get; set; }
    public int MaPhieuMuon { get; set; }
    public DateOnly NgayGiaHan { get; set; }
    public DateOnly HanTraCu { get; set; }
    public DateOnly HanTraMoi { get; set; }
    public byte LanGiaHan { get; set; }
    public int? NhanVienDuyet { get; set; }
    public string? GhiChu { get; set; }
    public PhieuMuon PhieuMuon { get; set; } = null!;
}
```

## PhieuTra
```csharp
public class PhieuTra {
    public int MaPhieuTra { get; set; }
    public int MaPhieuMuon { get; set; }
    public DateOnly NgayTra { get; set; }
    public int SoNgayMuon { get; set; }
    public int SoNgayTreHan { get; set; }
    public decimal TienPhat { get; set; }
    public bool DaThuPhat { get; set; }
    public byte TrangThaiSach { get; set; } = 1; // 1=Bình thường, 2=Hư, 3=Mất
    public string? GhiChu { get; set; }
    public int? NhanVienThu { get; set; }
    public PhieuMuon PhieuMuon { get; set; } = null!;
}
```

## VaiTro
```csharp
public class VaiTro {
    public int MaVaiTro { get; set; }
    public string TenVaiTro { get; set; } = ""; // "Admin"|"NhanVien"|"DocGia"
    public string? MoTa { get; set; }
    public ICollection<TaiKhoan> TaiKhoans { get; set; } = [];
}
```

## TaiKhoan
```csharp
public class TaiKhoan {
    public int MaTaiKhoan { get; set; }
    public int MaVaiTro { get; set; }
    public int? MaDocGia { get; set; }
    public string TenDangNhap { get; set; } = "";
    public string MatKhau { get; set; } = ""; // BCrypt hash
    public string HoTen { get; set; } = "";
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? LanDangNhapCuoi { get; set; }
    public byte TrangThai { get; set; } = 1;
    public VaiTro VaiTro { get; set; } = null!;
    public DocGia? DocGia { get; set; }
}
```

## CauHinhHeThong
```csharp
public class CauHinhHeThong {
    public int MaCauHinh { get; set; }
    public string TenCauHinh { get; set; } = "";
    public string GiaTri { get; set; } = "";
    public string? GhiChu { get; set; }
}
// Keys: "SoNgayMuonMacDinh", "MucPhatNgayTreHan",
//       "SoSachMuonToiDa", "SoLanGiaHanToiDa", "SoNgayGiaHanMoiLan"
```
