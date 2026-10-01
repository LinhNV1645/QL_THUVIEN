using LibraryManagement.Infrastructure.Persistence;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Infrastructure.Repositories;

using LibraryManagement.Infrastructure.Services;
using LibraryManagement.Infrastructure.Jobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddControllersWithViews().AddNewtonsoftJson();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(connectionString));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt => {
        opt.LoginPath = "/Login";
        opt.LogoutPath = "/Login/Logout";
        opt.AccessDeniedPath = "/Error/403";
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
        opt.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(opt => {
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opt.AddPolicy("Staff",     p => p.RequireRole("Admin", "NhanVien"));
});

builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions {
        CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
        QueuePollInterval            = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks           = true
    }));
builder.Services.AddHangfireServer();

// Infrastructure Repositories
builder.Services.AddScoped<ISachRepository, SachRepository>();
builder.Services.AddScoped<IDocGiaRepository, DocGiaRepository>();
builder.Services.AddScoped<IPhieuMuonRepository, PhieuMuonRepository>();
builder.Services.AddScoped<IGiaHanRepository, GiaHanRepository>();
builder.Services.AddScoped<IPhieuTraRepository, PhieuTraRepository>();
builder.Services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
builder.Services.AddScoped<ICauHinhRepository, CauHinhRepository>();
// Infrastructure Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<IExportService, ExportService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions {
    Authorization = [new HangfireAuthFilter()]
});

RecurringJob.AddOrUpdate<NhacNhoHanTraJob>("nhac-nho-han-tra",
    x => x.Execute(), "0 7 * * *"); // 7AM every day

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class HangfireAuthFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext ctx)
    {
        var httpCtx = (ctx as Hangfire.Dashboard.AspNetCoreDashboardContext)?.HttpContext;
        return httpCtx?.User.IsInRole("Admin") ?? false;
    }
}
