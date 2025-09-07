using MailKit.Net.Smtp;
using MimeKit;
using TunCRM.Models;
using Microsoft.Extensions.Options;

namespace TunCRM.Services
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl);
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email başarıyla gönderildi: {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Email gönderilirken hata oluştu: {toEmail}");
                return false;
            }
        }

        public async Task<bool> SendBulkEmailAsync(List<EmailRecipient> recipients, string subject, string body, bool isHtml = true)
        {
            var successCount = 0;
            var totalCount = recipients.Count;

            foreach (var recipient in recipients)
            {
                var success = await SendEmailAsync(recipient.Email, recipient.Name, subject, body, isHtml);
                if (success) successCount++;
            }

            _logger.LogInformation($"Toplu email gönderimi tamamlandı: {successCount}/{totalCount}");
            return successCount == totalCount;
        }

        public async Task<bool> SendGorevHatirlatmaAsync(Gorev gorev, Kullanici kullanici)
        {
            var subject = $"Görev Hatırlatması: {gorev.Baslik}";
            var body = GenerateGorevHatirlatmaTemplate(gorev, kullanici);
            
            return await SendEmailAsync(kullanici.Email, kullanici.TamAd, subject, body);
        }

        public async Task<bool> SendFirsatHatirlatmaAsync(Firsat firsat, Kullanici kullanici)
        {
            var subject = $"Fırsat Hatırlatması: {firsat.Ad}";
            var body = GenerateFirsatHatirlatmaTemplate(firsat, kullanici);
            
            return await SendEmailAsync(kullanici.Email, kullanici.TamAd, subject, body);
        }

        public async Task<bool> SendAktiviteHatirlatmaAsync(Aktivite aktivite, Kullanici kullanici)
        {
            var subject = $"Aktivite Hatırlatması: {aktivite.Baslik}";
            var body = GenerateAktiviteHatirlatmaTemplate(aktivite, kullanici);
            
            return await SendEmailAsync(kullanici.Email, kullanici.TamAd, subject, body);
        }

        public async Task<bool> SendWelcomeEmailAsync(Kullanici kullanici, string tempPassword)
        {
            var subject = "TunCRM'e Hoş Geldiniz!";
            var body = GenerateWelcomeEmailTemplate(kullanici, tempPassword);
            
            return await SendEmailAsync(kullanici.Email, kullanici.TamAd, subject, body);
        }

        public async Task<bool> SendPasswordResetEmailAsync(Kullanici kullanici, string resetToken)
        {
            var subject = "Şifre Sıfırlama - TunCRM";
            var body = GeneratePasswordResetTemplate(kullanici, resetToken);
            
            return await SendEmailAsync(kullanici.Email, kullanici.TamAd, subject, body);
        }

        private string GenerateGorevHatirlatmaTemplate(Gorev gorev, Kullanici kullanici)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Görev Hatırlatması</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
        .gorev-info {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .priority {{ padding: 5px 10px; border-radius: 3px; color: white; font-weight: bold; }}
        .priority-high {{ background: #dc3545; }}
        .priority-medium {{ background: #ffc107; color: #333; }}
        .priority-low {{ background: #28a745; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📋 Görev Hatırlatması</h1>
        </div>
        <div class='content'>
            <p>Merhaba {kullanici.TamAd},</p>
            <p>Aşağıdaki görev için hatırlatma:</p>
            
            <div class='gorev-info'>
                <h3>{gorev.Baslik}</h3>
                <p><strong>Açıklama:</strong> {gorev.Aciklama ?? "Açıklama yok"}</p>
                <p><strong>Öncelik:</strong> <span class='priority priority-{GetPriorityClass(gorev.Oncelik)}'>{GetPriorityName(gorev.Oncelik)}</span></p>
                <p><strong>Bitiş Tarihi:</strong> {gorev.BitisTarihi?.ToString("dd.MM.yyyy HH:mm") ?? "Belirtilmemiş"}</p>
                <p><strong>Firma:</strong> {gorev.Firma?.Ad ?? "Belirtilmemiş"}</p>
                <p><strong>Fırsat:</strong> {gorev.Firsat?.Ad ?? "Belirtilmemiş"}</p>
            </div>
            
            <p>Görevi tamamlamak için TunCRM sistemine giriş yapabilirsiniz.</p>
            
            <div class='footer'>
                <p>Bu email TunCRM sistemi tarafından otomatik olarak gönderilmiştir.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateFirsatHatirlatmaTemplate(Firsat firsat, Kullanici kullanici)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Fırsat Hatırlatması</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #28a745; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
        .firsat-info {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .stage {{ padding: 5px 10px; border-radius: 3px; color: white; font-weight: bold; }}
        .stage-1 {{ background: #6c757d; }}
        .stage-2 {{ background: #17a2b8; }}
        .stage-3 {{ background: #ffc107; color: #333; }}
        .stage-4 {{ background: #007bff; }}
        .stage-5 {{ background: #28a745; }}
        .stage-6 {{ background: #dc3545; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>💼 Fırsat Hatırlatması</h1>
        </div>
        <div class='content'>
            <p>Merhaba {kullanici.TamAd},</p>
            <p>Aşağıdaki fırsat için hatırlatma:</p>
            
            <div class='firsat-info'>
                <h3>{firsat.Ad}</h3>
                <p><strong>Açıklama:</strong> {firsat.Aciklama ?? "Açıklama yok"}</p>
                <p><strong>Aşama:</strong> <span class='stage stage-{(int)firsat.Asama}'>{GetStageName(firsat.Asama)}</span></p>
                <p><strong>Tutar:</strong> {firsat.Tutar?.ToString("C0") ?? "Belirtilmemiş"}</p>
                <p><strong>Firma:</strong> {firsat.Firma?.Ad ?? "Belirtilmemiş"}</p>
                <p><strong>Oluşturma Tarihi:</strong> {firsat.OlusturmaTarihi:dd.MM.yyyy HH:mm}</p>
            </div>
            
            <p>Fırsatı takip etmek için TunCRM sistemine giriş yapabilirsiniz.</p>
            
            <div class='footer'>
                <p>Bu email TunCRM sistemi tarafından otomatik olarak gönderilmiştir.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateAktiviteHatirlatmaTemplate(Aktivite aktivite, Kullanici kullanici)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Aktivite Hatırlatması</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #17a2b8; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
        .aktivite-info {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .type {{ padding: 5px 10px; border-radius: 3px; color: white; font-weight: bold; }}
        .type-1 {{ background: #17a2b8; }}
        .type-2 {{ background: #007bff; }}
        .type-3 {{ background: #ffc107; color: #333; }}
        .type-4 {{ background: #28a745; }}
        .type-5 {{ background: #6c757d; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📅 Aktivite Hatırlatması</h1>
        </div>
        <div class='content'>
            <p>Merhaba {kullanici.TamAd},</p>
            <p>Aşağıdaki aktivite için hatırlatma:</p>
            
            <div class='aktivite-info'>
                <h3>{aktivite.Baslik}</h3>
                <p><strong>Açıklama:</strong> {aktivite.Aciklama ?? "Açıklama yok"}</p>
                <p><strong>Tür:</strong> <span class='type type-{(int)aktivite.Tip}'>{GetActivityTypeName(aktivite.Tip)}</span></p>
                <p><strong>Tarih:</strong> {aktivite.Tarih:dd.MM.yyyy HH:mm}</p>
                <p><strong>Firma:</strong> {aktivite.Firma?.Ad ?? "Belirtilmemiş"}</p>
                <p><strong>Fırsat:</strong> {aktivite.Firsat?.Ad ?? "Belirtilmemiş"}</p>
            </div>
            
            <p>Aktiviteyi takip etmek için TunCRM sistemine giriş yapabilirsiniz.</p>
            
            <div class='footer'>
                <p>Bu email TunCRM sistemi tarafından otomatik olarak gönderilmiştir.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateWelcomeEmailTemplate(Kullanici kullanici, string tempPassword)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Hoş Geldiniz</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
        .welcome-info {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .password {{ background: #e9ecef; padding: 10px; border-radius: 3px; font-family: monospace; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 TunCRM'e Hoş Geldiniz!</h1>
        </div>
        <div class='content'>
            <p>Merhaba {kullanici.TamAd},</p>
            <p>TunCRM sistemine başarıyla kaydoldunuz. Hesabınızı kullanmaya başlayabilirsiniz.</p>
            
            <div class='welcome-info'>
                <h3>Hesap Bilgileriniz</h3>
                <p><strong>Email:</strong> {kullanici.Email}</p>
                <p><strong>Geçici Şifre:</strong></p>
                <div class='password'>{tempPassword}</div>
                <p><em>Güvenlik için ilk girişte şifrenizi değiştirmenizi öneririz.</em></p>
            </div>
            
            <p>TunCRM ile müşteri ilişkilerinizi daha etkili yönetebilirsiniz.</p>
            
            <div class='footer'>
                <p>Bu email TunCRM sistemi tarafından otomatik olarak gönderilmiştir.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordResetTemplate(Kullanici kullanici, string resetToken)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Şifre Sıfırlama</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc3545; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
        .reset-info {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .token {{ background: #e9ecef; padding: 10px; border-radius: 3px; font-family: monospace; word-break: break-all; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Şifre Sıfırlama</h1>
        </div>
        <div class='content'>
            <p>Merhaba {kullanici.TamAd},</p>
            <p>Hesabınız için şifre sıfırlama talebinde bulundunuz.</p>
            
            <div class='reset-info'>
                <h3>Sıfırlama Kodu</h3>
                <div class='token'>{resetToken}</div>
                <p><em>Bu kodu kullanarak yeni şifrenizi oluşturabilirsiniz.</em></p>
            </div>
            
            <p>Bu kodu kimseyle paylaşmayın. Eğer bu talebi siz yapmadıysanız, lütfen sistem yöneticisi ile iletişime geçin.</p>
            
            <div class='footer'>
                <p>Bu email TunCRM sistemi tarafından otomatik olarak gönderilmiştir.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPriorityClass(GorevOnceligi oncelik)
        {
            return oncelik switch
            {
                GorevOnceligi.Kritik => "high",
                GorevOnceligi.Yuksek => "high",
                GorevOnceligi.Normal => "medium",
                GorevOnceligi.Dusuk => "low",
                _ => "medium"
            };
        }

        private string GetPriorityName(GorevOnceligi oncelik)
        {
            return oncelik switch
            {
                GorevOnceligi.Kritik => "Kritik",
                GorevOnceligi.Yuksek => "Yüksek",
                GorevOnceligi.Normal => "Normal",
                GorevOnceligi.Dusuk => "Düşük",
                _ => "Normal"
            };
        }

        private string GetStageName(FirsatAsamasi asama)
        {
            return asama switch
            {
                FirsatAsamasi.IlkIletisim => "İlk İletişim",
                FirsatAsamasi.TeklifHazirlama => "Teklif Hazırlama",
                FirsatAsamasi.TeklifGonderildi => "Teklif Gönderildi",
                FirsatAsamasi.Muzakere => "Müzakere",
                FirsatAsamasi.KapatildiKazanildi => "Kazanıldı",
                FirsatAsamasi.KapatildiKaybedildi => "Kaybedildi",
                _ => "Bilinmiyor"
            };
        }

        private string GetActivityTypeName(AktiviteTipi tip)
        {
            return tip switch
            {
                AktiviteTipi.Telefon => "Telefon",
                AktiviteTipi.Email => "Email",
                AktiviteTipi.Toplanti => "Toplantı",
                AktiviteTipi.Not => "Not",
                AktiviteTipi.Gorev => "Görev",
                _ => "Bilinmiyor"
            };
        }
    }

    public class EmailRecipient
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
