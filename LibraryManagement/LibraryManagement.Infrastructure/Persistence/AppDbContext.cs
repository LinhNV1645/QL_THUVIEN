using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Sach> Sachs => Set<Sach>();
    public DbSet<TheLoai> TheLoais => Set<TheLoai>();
    public DbSet<TacGia> TacGias => Set<TacGia>();
    public DbSet<NhaXuatBan> NhaXuatBans => Set<NhaXuatBan>();
    public DbSet<DocGia> DocGias => Set<DocGia>();
    public DbSet<PhieuMuon> PhieuMuons => Set<PhieuMuon>();
    public DbSet<CTPhieuMuon> CTPhieuMuons => Set<CTPhieuMuon>();
    public DbSet<GiaHan> GiaHans => Set<GiaHan>();
    public DbSet<PhieuTra> PhieuTras => Set<PhieuTra>();
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<CauHinhHeThong> CauHinhHeThongs => Set<CauHinhHeThong>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Sach
        mb.Entity<Sach>(e => {
            e.ToTable("Sach");
            e.HasKey(x => x.MaSach);
            e.Property(x => x.TenSach).HasMaxLength(300).IsRequired();
            e.Property(x => x.MaQR).HasMaxLength(100);
            e.Property(x => x.ViTri).HasMaxLength(50);
            e.Property(x => x.MoTa).HasMaxLength(1000);
            e.HasIndex(x => x.MaQR).IsUnique();
            e.HasIndex(x => x.TenSach);
            e.HasOne(x => x.TheLoai).WithMany(t => t.Sachs).HasForeignKey(x => x.MaTheLoai);
            e.HasOne(x => x.TacGia).WithMany(t => t.Sachs).HasForeignKey(x => x.MaTacGia);
            e.HasOne(x => x.NhaXuatBan).WithMany(n => n.Sachs).HasForeignKey(x => x.MaNXB);
        });

        // TheLoai
        mb.Entity<TheLoai>(e => {
            e.ToTable("TheLoai");
            e.HasKey(x => x.MaTheLoai);
            e.Property(x => x.TenTheLoai).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.TenTheLoai).IsUnique();
        });

        // TacGia
        mb.Entity<TacGia>(e => {
            e.ToTable("TacGia");
            e.HasKey(x => x.MaTacGia);
            e.Property(x => x.TenTacGia).HasMaxLength(150).IsRequired();
        });

        // NhaXuatBan
        mb.Entity<NhaXuatBan>(e => {
            e.ToTable("NhaXuatBan");
            e.HasKey(x => x.MaNXB);
            e.Property(x => x.TenNXB).HasMaxLength(150).IsRequired();
        });

        // DocGia
        mb.Entity<DocGia>(e => {
            e.ToTable("DocGia");
            e.HasKey(x => x.MaDocGia);
            e.Property(x => x.HoTen).HasMaxLength(150).IsRequired();
            e.Property(x => x.Lop).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.SoDienThoai).HasMaxLength(15);
            e.HasIndex(x => x.HoTen);
        });

        // PhieuMuon
        mb.Entity<PhieuMuon>(e => {
            e.ToTable("PhieuMuon");
            e.HasKey(x => x.MaPhieuMuon);
            e.HasOne(x => x.DocGia).WithMany(d => d.PhieuMuons).HasForeignKey(x => x.MaDocGia);
            e.HasIndex(x => x.MaDocGia);
            e.HasIndex(x => x.NgayHanTra);
            e.HasIndex(x => x.TrangThai);
        });

        // CTPhieuMuon
        mb.Entity<CTPhieuMuon>(e => {
            e.ToTable("CTPhieuMuon");
            e.HasKey(x => x.MaCT);
            e.HasOne(x => x.PhieuMuon).WithMany(p => p.CTPhieuMuons).HasForeignKey(x => x.MaPhieuMuon);
            e.HasOne(x => x.Sach).WithMany(s => s.CTPhieuMuons).HasForeignKey(x => x.MaSach);
            e.HasIndex(x => x.MaSach);
            e.HasIndex(x => new { x.MaPhieuMuon, x.MaSach }).IsUnique();
        });

        // GiaHan
        mb.Entity<GiaHan>(e => {
            e.ToTable("GiaHan");
            e.HasKey(x => x.MaGiaHan);
            e.HasOne(x => x.PhieuMuon).WithMany(p => p.GiaHans).HasForeignKey(x => x.MaPhieuMuon);
        });

        // PhieuTra
        mb.Entity<PhieuTra>(e => {
            e.ToTable("PhieuTra");
            e.HasKey(x => x.MaPhieuTra);
            e.Property(x => x.TienPhat).HasColumnType("decimal(12,0)");
            e.HasOne(x => x.PhieuMuon).WithOne(p => p.PhieuTra).HasForeignKey<PhieuTra>(x => x.MaPhieuMuon);
            e.HasIndex(x => x.NgayTra);
        });

        // VaiTro
        mb.Entity<VaiTro>(e => {
            e.ToTable("VaiTro");
            e.HasKey(x => x.MaVaiTro);
            e.Property(x => x.TenVaiTro).HasMaxLength(50).IsRequired();
        });

        // TaiKhoan
        mb.Entity<TaiKhoan>(e => {
            e.ToTable("TaiKhoan");
            e.HasKey(x => x.MaTaiKhoan);
            e.Property(x => x.TenDangNhap).HasMaxLength(50).IsRequired();
            e.Property(x => x.MatKhau).HasMaxLength(255).IsRequired();
            e.Property(x => x.HoTen).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150);
            e.HasIndex(x => x.TenDangNhap).IsUnique();
            e.HasOne(x => x.VaiTro).WithMany(v => v.TaiKhoans).HasForeignKey(x => x.MaVaiTro);
            e.HasOne(x => x.DocGia).WithMany().HasForeignKey(x => x.MaDocGia);
        });

        // CauHinhHeThong
        mb.Entity<CauHinhHeThong>(e => {
            e.ToTable("CauHinhHeThong");
            e.HasKey(x => x.MaCauHinh);
            e.Property(x => x.TenCauHinh).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.TenCauHinh).IsUnique();
        });
    }
}
