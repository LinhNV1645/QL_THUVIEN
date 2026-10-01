using LibraryManagement.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace LibraryManagement.Infrastructure.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendNhacNhoHanTraAsync(
        string toEmail,
        string tenDocGia,
        string danhSachSach,
        DateOnly ngayHanTra,
        int soNgayConLai)
    {
        var settings = config.GetSection("EmailSettings");
        var host        = settings["Host"]        ?? "smtp.gmail.com";
        var port        = int.Parse(settings["Port"] ?? "587");
        var enableSsl   = bool.Parse(settings["EnableSsl"] ?? "true");
        var userName    = settings["UserName"]    ?? "";
        var password    = settings["Password"]    ?? "";
        var displayName = settings["DisplayName"] ?? "Thư viện";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(displayName, userName));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"[Nhắc nhở] Sách mượn sắp đến hạn trả — còn {soNgayConLai} ngày";

        var body = $"""
            Xin chào {tenDocGia},

            Bạn đang mượn những cuốn sách sau và sẽ đến hạn trả vào ngày {ngayHanTra:dd/MM/yyyy}
            (còn {soNgayConLai} ngày):

            {danhSachSach}

            Vui lòng trả sách đúng hạn để tránh phát sinh tiền phạt.

            Trân trọng,
            {displayName}
            """;

        message.Body = new TextPart("plain") { Text = body };

        try
        {
            using var client = new SmtpClient();
            var secureOption = enableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;
            await client.ConnectAsync(host, port, secureOption);
            await client.AuthenticateAsync(userName, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            logger.LogInformation("Email nhắc nhở gửi thành công tới {Email}", toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gửi email nhắc nhở thất bại tới {Email}", toEmail);
            throw;
        }
    }
}
