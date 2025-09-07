using TunCRM.Models;
using TunCRM.Data;
using Microsoft.EntityFrameworkCore;

namespace TunCRM.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(30); // Her 30 dakikada bir çalış

        public NotificationService(IServiceProvider serviceProvider, ILogger<NotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendNotifications();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bildirim kontrolü sırasında hata oluştu");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }

        private async Task CheckAndSendNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            // Gecikmiş görevleri kontrol et
            await CheckOverdueTasks(context, emailService);

            // Bugün bitecek görevleri kontrol et
            await CheckTasksDueToday(context, emailService);

            // Uzun süre güncellenmeyen fırsatları kontrol et
            await CheckStaleOpportunities(context, emailService);

            // Yaklaşan aktiviteleri kontrol et
            await CheckUpcomingActivities(context, emailService);
        }

        private async Task CheckOverdueTasks(AppDbContext context, EmailService emailService)
        {
            var overdueTasks = await context.Gorevler
                .Include(g => g.AtananKullanici)
                .Include(g => g.Firma)
                .Include(g => g.Firsat)
                .Where(g => g.Aktif && 
                           g.Durum != GorevDurumu.Tamamlandi &&
                           g.BitisTarihi.HasValue &&
                           g.BitisTarihi.Value < DateTime.Now)
                .ToListAsync();

            foreach (var task in overdueTasks)
            {
                if (task.AtananKullanici != null)
                {
                    await emailService.SendGorevHatirlatmaAsync(task, task.AtananKullanici);
                    _logger.LogInformation($"Gecikmiş görev hatırlatması gönderildi: {task.Baslik} - {task.AtananKullanici.Email}");
                }
            }
        }

        private async Task CheckTasksDueToday(AppDbContext context, EmailService emailService)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var tasksDueToday = await context.Gorevler
                .Include(g => g.AtananKullanici)
                .Include(g => g.Firma)
                .Include(g => g.Firsat)
                .Where(g => g.Aktif && 
                           g.Durum != GorevDurumu.Tamamlandi &&
                           g.BitisTarihi.HasValue &&
                           g.BitisTarihi.Value >= today &&
                           g.BitisTarihi.Value < tomorrow)
                .ToListAsync();

            foreach (var task in tasksDueToday)
            {
                if (task.AtananKullanici != null)
                {
                    await emailService.SendGorevHatirlatmaAsync(task, task.AtananKullanici);
                    _logger.LogInformation($"Bugün bitecek görev hatırlatması gönderildi: {task.Baslik} - {task.AtananKullanici.Email}");
                }
            }
        }

        private async Task CheckStaleOpportunities(AppDbContext context, EmailService emailService)
        {
            var staleDate = DateTime.Now.AddDays(-7); // 7 gün önce güncellenmiş

            var staleOpportunities = await context.Firsatlar
                .Include(f => f.Kullanici)
                .Include(f => f.Firma)
                .Where(f => f.Asama != FirsatAsamasi.KapatildiKazanildi &&
                           f.Asama != FirsatAsamasi.KapatildiKaybedildi &&
                           f.GuncellemeTarihi.HasValue &&
                           f.GuncellemeTarihi.Value < staleDate)
                .ToListAsync();

            foreach (var opportunity in staleOpportunities)
            {
                await emailService.SendFirsatHatirlatmaAsync(opportunity, opportunity.Kullanici);
                _logger.LogInformation($"Eski fırsat hatırlatması gönderildi: {opportunity.Ad} - {opportunity.Kullanici.Email}");
            }
        }

        private async Task CheckUpcomingActivities(AppDbContext context, EmailService emailService)
        {
            var tomorrow = DateTime.Today.AddDays(1);
            var dayAfterTomorrow = DateTime.Today.AddDays(2);

            var upcomingActivities = await context.Aktiviteler
                .Include(a => a.Kullanici)
                .Include(a => a.Firma)
                .Include(a => a.Firsat)
                .Where(a => a.Tarih >= tomorrow && a.Tarih < dayAfterTomorrow)
                .ToListAsync();

            foreach (var activity in upcomingActivities)
            {
                await emailService.SendAktiviteHatirlatmaAsync(activity, activity.Kullanici);
                _logger.LogInformation($"Yaklaşan aktivite hatırlatması gönderildi: {activity.Baslik} - {activity.Kullanici.Email}");
            }
        }
    }
}
