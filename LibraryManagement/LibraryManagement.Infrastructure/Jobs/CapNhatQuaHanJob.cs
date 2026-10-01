using LibraryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Infrastructure.Jobs;

/// <summary>
/// Hangfire job - chuyển các phiếu mượn đã quá hạn trả sang trạng thái 3 (Quá hạn).
/// </summary>
public class CapNhatQuaHanJob(AppDbContext db, ILogger<CapNhatQuaHanJob> logger)
{
    public async Task Execute()
    {
        await db.Database.ExecuteSqlRawAsync("EXEC sp_CapNhatQuaHan");
        logger.LogInformation("CapNhatQuaHanJob hoàn tất lúc {Time}", DateTime.Now);
    }
}
