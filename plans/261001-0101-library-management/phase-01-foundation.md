# Phase 01 — Foundation

**Priority**: CRITICAL — phải xong trước Phase 02/03/04  
**Status**: ⬜ Pending  
**Agent**: 1 agent duy nhất, sequential  
**Ước tính**: 2–3 giờ

---

## Mục tiêu

Tạo toàn bộ nền tảng mà 3 agent sau dùng chung:
- Solution structure (4 projects)
- Domain entities
- Application interfaces
- EF Core DbContext + Migrations
- Auth (Cookie + 3 role)
- Base layout Bootstrap 5

---

## Todo List

### 1. Tạo Solution & Projects
- [ ] `dotnet new sln -n LibraryManagement`
- [ ] `dotnet new classlib -n LibraryManagement.Domain`
- [ ] `dotnet new classlib -n LibraryManagement.Application`
- [ ] `dotnet new classlib -n LibraryManagement.Infrastructure`
- [ ] `dotnet new mvc -n LibraryManagement.Web`
- [ ] Add project references theo Clean Architecture
- [ ] Install NuGet packages (xem danh sách bên dưới)

### 2. Domain Entities (theo contracts/entities.md)
- [ ] `Sach.cs`
- [ ] `TheLoai.cs`, `TacGia.cs`, `NhaXuatBan.cs`
- [ ] `DocGia.cs`
- [ ] `PhieuMuon.cs`, `CTPhieuMuon.cs`
- [ ] `GiaHan.cs`
- [ ] `PhieuTra.cs`
- [ ] `TaiKhoan.cs`, `VaiTro.cs`
- [ ] `CauHinhHeThong.cs`
- [ ] Domain exceptions: `TonKhoKhongDuException`, `GioiHanMuonException`, `QuaHanGiaHanException`

### 3. Application Interfaces (theo contracts/interfaces.md)
- [ ] `ISachRepository.cs`
- [ ] `IDocGiaRepository.cs`
- [ ] `IPhieuMuonRepository.cs`
- [ ] `IGiaHanRepository.cs`
- [ ] `IPhieuTraRepository.cs`
- [ ] `ITaiKhoanRepository.cs`
- [ ] `ICauHinhRepository.cs`
- [ ] `IEmailService.cs`
- [ ] `IExportService.cs`
- [ ] `IQrService.cs`
- [ ] `IUnitOfWork.cs`

### 4. Application DTOs & Use Cases (shared)
- [ ] `PaginatedResult<T>.cs`
- [ ] `Result<T>.cs` (wrapper cho error handling)
- [ ] `DashboardDto.cs`

### 5. Infrastructure — EF Core
- [ ] `AppDbContext.cs` — cấu hình tất cả DbSet + Fluent API
- [ ] Fluent API: relationships, constraints, column types
- [ ] `DbInitializer.cs` — chạy SQL script seed
- [ ] Migration ban đầu: `dotnet ef migrations add InitialCreate`

### 6. Infrastructure — Repositories (base)
- [ ] `BaseRepository<T>.cs` — CRUD chung
- [ ] `UnitOfWork.cs`

### 7. Web — Auth
- [ ] Cấu hình Cookie Authentication trong `Program.cs`
- [ ] `[Authorize(Roles = "Admin")]` policy
- [ ] `LoginController.cs` + `Views/Login/Index.cshtml`
- [ ] `AccountController.cs` (đổi mật khẩu)
- [ ] BCrypt hash password khi tạo tài khoản

### 8. Web — Base Layout
- [ ] `_Layout.cshtml` — sidebar + navbar Bootstrap 5
- [ ] Sidebar: phân quyền hiển thị menu theo role
- [ ] `_ValidationScriptsPartial.cshtml`
- [ ] `wwwroot/css/site.css` — custom styles
- [ ] Error pages: 403, 404, 500

### 9. Chạy SQL Scripts lên SQL Server
- [ ] Kết nối SQL Server qua VPN (IP từ thầy/admin)
- [ ] Chạy `contracts/db-schema.sql`
- [ ] Chạy `contracts/stored-procedures.sql`
- [ ] Chạy `contracts/views-triggers.sql`
- [ ] Verify: đúng 12 bảng, 8 SP, 5 View, 2 Trigger

---

## NuGet Packages

```xml
<!-- Infrastructure project -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.*" />
<PackageReference Include="MailKit" Version="4.*" />
<PackageReference Include="QRCoder" Version="1.*" />
<PackageReference Include="ClosedXML" Version="0.102.*" />
<PackageReference Include="DinkToPdf" Version="1.*" />
<PackageReference Include="Hangfire.SqlServer" Version="1.*" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.*" />

<!-- Web project -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="2.*" />
<PackageReference Include="Newtonsoft.Json" Version="13.*" />
```

---

## Program.cs cấu hình cần có

```csharp
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt => {
        opt.LoginPath = "/Login";
        opt.AccessDeniedPath = "/Error/403";
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(opt => {
    opt.AddPolicy("AdminOnly",   p => p.RequireRole("Admin"));
    opt.AddPolicy("Staff",       p => p.RequireRole("Admin", "NhanVien"));
    opt.AddPolicy("AllUsers",    p => p.RequireRole("Admin", "NhanVien", "DocGia"));
});

builder.Services.AddHangfire(cfg => cfg.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

// DI registrations
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<IExportService, ExportService>();
```

---

## Success Criteria

- [ ] `dotnet build` không lỗi
- [ ] Migration chạy được, tạo đúng 12 bảng
- [ ] Login/logout hoạt động
- [ ] Sidebar hiển thị menu đúng theo role
- [ ] Dashboard trả về data từ `sp_Dashboard`
- [ ] 3 agent Phase 02/03/04 có thể checkout và implement ngay
