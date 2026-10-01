using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace LibraryManagement.Infrastructure.Services;

public class QrService(IWebHostEnvironment env, ILogger<QrService> logger) : IQrService
{
    private const int PixelsPerModule = 10;

    // Trả về đường dẫn relative "/qr/{maQR}.png", lưu file vào wwwroot/qr/
    public string GenerateQrImagePath(string maQR)
    {
        var dir = Path.Combine(env.WebRootPath, "qr");
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, $"{maQR}.png");
        if (!File.Exists(filePath))
            File.WriteAllBytes(filePath, GenerateQrBytes(maQR));

        logger.LogDebug("QR image saved: {Path}", filePath);
        return $"/qr/{maQR}.png";
    }

    // Trả về PNG bytes của mã QR
    public byte[] GenerateQrBytes(string maQR)
    {
        using var qrGenerator  = new QRCodeGenerator();
        using var qrCodeData   = qrGenerator.CreateQrCode(maQR, QRCodeGenerator.ECCLevel.Q);
        using var qrCode       = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(PixelsPerModule);
    }
}
