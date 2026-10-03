using System.Globalization;
using System.Net;
using System.Text;
using LibraryManagement.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace LibraryManagement.Infrastructure.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private sealed record DongThongTin(string Nhan, string GiaTri, bool NoiBat = false);

    private sealed record NoiDungEmail(
        string Subject,
        string TieuDe,
        string NhanTrangThai,
        string MauChuDao,
        string MauNen,
        string LoiMo,
        IReadOnlyList<DongThongTin> ThongTin,
        IReadOnlyList<string> DanhSachSach,
        string LoiNhan);

    public Task SendNhacNhoHanTraAsync(
        string toEmail,
        string tenDocGia,
        IEnumerable<string> danhSachSach,
        DateOnly ngayHanTra,
        int soNgayConLai)
    {
        var noiDung = new NoiDungEmail(
            Subject      : $"[Nhắc nhở] Sách mượn sắp đến hạn trả — còn {soNgayConLai} ngày",
            TieuDe       : "Sách mượn sắp đến hạn trả",
            NhanTrangThai: $"Còn {soNgayConLai} ngày",
            MauChuDao    : "#1d4ed8",
            MauNen       : "#eff6ff",
            LoiMo        : $"Bạn đang mượn những cuốn sách dưới đây và sẽ đến hạn trả vào ngày {ngayHanTra:dd/MM/yyyy}.",
            ThongTin     :
            [
                new("Hạn trả", ngayHanTra.ToString("dd/MM/yyyy")),
                new("Số ngày còn lại", $"{soNgayConLai} ngày", NoiBat: true)
            ],
            DanhSachSach : danhSachSach.ToList(),
            LoiNhan      : "Vui lòng trả sách đúng hạn để tránh phát sinh tiền phạt. "
                         + "Nếu cần thêm thời gian, bạn có thể liên hệ thư viện để được gia hạn.");

        return GuiAsync(toEmail, tenDocGia, noiDung);
    }

    public Task SendNhacNhoQuaHanAsync(
        string toEmail,
        string tenDocGia,
        IEnumerable<string> danhSachSach,
        DateOnly ngayHanTra,
        int soNgayTre,
        decimal tienPhat)
    {
        List<DongThongTin> thongTin =
        [
            new("Hạn trả", ngayHanTra.ToString("dd/MM/yyyy")),
            new("Số ngày trễ hạn", $"{soNgayTre} ngày", NoiBat: true)
        ];
        if (tienPhat > 0)
            thongTin.Add(new("Tiền phạt ước tính", $"{tienPhat.ToString("N0", Vi)} đ", NoiBat: true));

        var noiDung = new NoiDungEmail(
            Subject      : $"[KHẨN] Sách mượn đã quá hạn {soNgayTre} ngày — vui lòng trả ngay",
            TieuDe       : "Sách mượn đã quá hạn",
            NhanTrangThai: $"Quá hạn {soNgayTre} ngày",
            MauChuDao    : "#dc2626",
            MauNen       : "#fef2f2",
            LoiMo        : $"Bạn đang có sách mượn đã quá hạn trả {soNgayTre} ngày (hạn trả: {ngayHanTra:dd/MM/yyyy}).",
            ThongTin     : thongTin,
            DanhSachSach : danhSachSach.ToList(),
            LoiNhan      : "Vui lòng mang sách đến thư viện để trả ngay hôm nay để tránh phát sinh thêm tiền phạt. "
                         + "Nếu có thắc mắc, liên hệ thư viện trường để được hỗ trợ.");

        return GuiAsync(toEmail, tenDocGia, noiDung);
    }

    private async Task GuiAsync(string toEmail, string tenDocGia, NoiDungEmail noiDung)
    {
        var settings    = config.GetSection("EmailSettings");
        var host        = settings["Host"]        ?? "smtp.gmail.com";
        var port        = int.Parse(settings["Port"] ?? "587");
        var enableSsl   = bool.Parse(settings["EnableSsl"] ?? "true");
        var userName    = settings["UserName"]    ?? "";
        var password    = settings["Password"]    ?? "";
        var displayName = settings["DisplayName"] ?? "Thư viện";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(displayName, userName));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = noiDung.Subject;
        message.Body = new BodyBuilder
        {
            HtmlBody = TaoHtml(tenDocGia, displayName, noiDung),
            TextBody = TaoText(tenDocGia, displayName, noiDung)
        }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            var secureOption = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(host, port, secureOption);
            await client.AuthenticateAsync(userName, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            logger.LogInformation("Email \"{Subject}\" gửi thành công tới {Email}", noiDung.Subject, toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gửi email \"{Subject}\" thất bại tới {Email}", noiDung.Subject, toEmail);
            throw;
        }
    }

    private static string E(string? s) => WebUtility.HtmlEncode(s ?? "");

    private static string TaoHtml(string tenDocGia, string displayName, NoiDungEmail nd)
    {
        var mau = nd.MauChuDao;

        var thongTin = string.Concat(nd.ThongTin.Select((t, i) =>
        {
            var vien   = i < nd.ThongTin.Count - 1 ? "border-bottom:1px solid #e5e7eb;" : "";
            var mauChu = t.NoiBat ? mau : "#111827";
            return $"""
                <tr>
                  <td style="padding:12px 16px;font-size:14px;color:#6b7280;{vien}">{E(t.Nhan)}</td>
                  <td style="padding:12px 16px;font-size:15px;font-weight:700;color:{mauChu};text-align:right;{vien}">{E(t.GiaTri)}</td>
                </tr>
                """;
        }));

        var sach = string.Concat(nd.DanhSachSach.Select((ten, i) => $"""
            <tr>
              <td width="40" style="padding:10px 0 10px 4px;vertical-align:middle;">
                <div style="width:26px;height:26px;line-height:26px;border-radius:13px;background:{nd.MauNen};color:{mau};font-size:13px;font-weight:700;text-align:center;">{i + 1}</div>
              </td>
              <td style="padding:10px 8px 10px 0;font-size:15px;color:#111827;border-bottom:1px solid #f3f4f6;">{E(ten)}</td>
            </tr>
            """));

        return $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>{E(nd.Subject)}</title>
            </head>
            <body style="margin:0;padding:0;background:#f3f4f6;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f3f4f6;padding:32px 12px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(17,24,39,0.08);">

                      <tr>
                        <td style="background:{mau};padding:28px 32px;color:#ffffff;">
                          <div style="font-size:12px;letter-spacing:1.5px;text-transform:uppercase;opacity:0.85;">{E(displayName)}</div>
                          <div style="font-size:24px;font-weight:700;margin-top:6px;">{E(nd.TieuDe)}</div>
                          <div style="margin-top:16px;">
                            <span style="display:inline-block;background:#ffffff;color:{mau};font-size:13px;font-weight:700;padding:6px 14px;border-radius:999px;">{E(nd.NhanTrangThai)}</span>
                          </div>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:28px 32px 8px;">
                          <p style="margin:0 0 12px;font-size:16px;color:#111827;">Xin chào <strong>{E(tenDocGia)}</strong>,</p>
                          <p style="margin:0;font-size:15px;line-height:1.6;color:#374151;">{E(nd.LoiMo)}</p>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:16px 32px;">
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border:1px solid #e5e7eb;border-radius:8px;border-collapse:separate;">
                            {thongTin}
                          </table>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:8px 32px 0;">
                          <div style="font-size:12px;font-weight:700;letter-spacing:1px;text-transform:uppercase;color:#6b7280;margin-bottom:4px;">Danh sách sách đang mượn ({nd.DanhSachSach.Count})</div>
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                            {sach}
                          </table>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:24px 32px;">
                          <div style="background:{nd.MauNen};border-left:4px solid {mau};border-radius:6px;padding:14px 16px;font-size:14px;line-height:1.6;color:#374151;">{E(nd.LoiNhan)}</div>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:0 32px 28px;font-size:15px;line-height:1.6;color:#374151;">
                          Trân trọng,<br><strong style="color:#111827;">{E(displayName)}</strong>
                        </td>
                      </tr>

                      <tr>
                        <td style="background:#f9fafb;border-top:1px solid #e5e7eb;padding:16px 32px;font-size:12px;line-height:1.5;color:#9ca3af;text-align:center;">
                          Đây là email tự động từ hệ thống quản lý thư viện.<br>Nếu bạn đã trả sách, vui lòng bỏ qua email này.
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string TaoText(string tenDocGia, string displayName, NoiDungEmail nd)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Xin chào {tenDocGia},").AppendLine();
        sb.AppendLine(nd.LoiMo).AppendLine();
        foreach (var t in nd.ThongTin)
            sb.AppendLine($"{t.Nhan}: {t.GiaTri}");
        sb.AppendLine().AppendLine($"Danh sách sách đang mượn ({nd.DanhSachSach.Count}):");
        for (var i = 0; i < nd.DanhSachSach.Count; i++)
            sb.AppendLine($"  {i + 1}. {nd.DanhSachSach[i]}");
        sb.AppendLine().AppendLine(nd.LoiNhan).AppendLine();
        sb.AppendLine("Trân trọng,").AppendLine(displayName);
        return sb.ToString();
    }
}
