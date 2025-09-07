using Microsoft.EntityFrameworkCore;
using TunCRM.Data;
using TunCRM.Models;

namespace TunCRM.Services
{
    public class CrmService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CrmService> _logger;
        private readonly CacheService _cacheService;
        private readonly ErrorHandlingService _errorHandling;

        public CrmService(AppDbContext context, ILogger<CrmService> logger, CacheService cacheService, ErrorHandlingService errorHandling)
        {
            _context = context;
            _logger = logger;
            _cacheService = cacheService;
            _errorHandling = errorHandling;
        }

        #region Firma İşlemleri

        public async Task<List<Firma>> GetFirmalarAsync(string? filter = null, string? sehir = null)
        {
            var cacheKey = $"firmalar_{filter}_{sehir}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var query = _context.Firmalar.AsQueryable();

                if (!string.IsNullOrEmpty(filter))
                {
                    query = query.Where(f => f.Ad.Contains(filter) || 
                                           f.Telefon!.Contains(filter) || 
                                           f.Email!.Contains(filter));
                }

                if (!string.IsNullOrEmpty(sehir))
                {
                    query = query.Where(f => f.Sehir!.Contains(sehir));
                }

                return await query.OrderByDescending(f => f.OlusturmaTarihi).ToListAsync();
            });
        }

        public async Task<Firma?> GetFirmaByIdAsync(int id)
        {
            return await _context.Firmalar
                .Include(f => f.Firsatlar)
                .Include(f => f.Aktiviteler)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Firsat?> GetFirsatByIdAsync(int id)
        {
            return await _context.Firsatlar
                .Include(f => f.Firma)
                .Include(f => f.Kullanici)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Firma> CreateFirmaAsync(Firma firma)
        {
            // Çift kayıt kontrolü
            var existingFirma = await _context.Firmalar
                .FirstOrDefaultAsync(f => f.Ad.ToLower() == firma.Ad.ToLower() && 
                                        f.Sehir == firma.Sehir);

            if (existingFirma != null)
            {
                throw new InvalidOperationException("Bu firma zaten kayıtlı!");
            }

            firma.OlusturmaTarihi = DateTime.Now;
            _context.Firmalar.Add(firma);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Yeni firma eklendi: {firma.Ad}");
            return firma;
        }

        public async Task<Firma> UpdateFirmaAsync(Firma firma)
        {
            var existingFirma = await _context.Firmalar.FindAsync(firma.Id);
            if (existingFirma == null)
            {
                throw new ArgumentException("Firma bulunamadı!");
            }

            existingFirma.Ad = firma.Ad;
            existingFirma.Adres = firma.Adres;
            existingFirma.Telefon = firma.Telefon;
            existingFirma.Email = firma.Email;
            existingFirma.WebSitesi = firma.WebSitesi;
            existingFirma.Sehir = firma.Sehir;
            existingFirma.Ilce = firma.Ilce;
            existingFirma.PostaKodu = firma.PostaKodu;
            existingFirma.Notlar = firma.Notlar;
            existingFirma.GuncellemeTarihi = DateTime.Now;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Firma güncellendi: {firma.Ad}");
            return existingFirma;
        }

        public async Task DeleteFirmaAsync(int id)
        {
            var firma = await _context.Firmalar.FindAsync(id);
            if (firma == null)
            {
                throw new ArgumentException("Firma bulunamadı!");
            }

            _context.Firmalar.Remove(firma);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Firma silindi: {firma.Ad}");
        }

        #endregion

        #region Fırsat İşlemleri

        public async Task<List<Firsat>> GetFirsatlarAsync(int? firmaId = null, FirsatAsamasi? asama = null)
        {
            var query = _context.Firsatlar
                .Include(f => f.Firma)
                .Include(f => f.Kullanici)
                .AsQueryable();

            if (firmaId.HasValue)
            {
                query = query.Where(f => f.FirmaId == firmaId.Value);
            }

            if (asama.HasValue)
            {
                query = query.Where(f => f.Asama == asama.Value);
            }

            return await query.OrderByDescending(f => f.OlusturmaTarihi).ToListAsync();
        }

        public async Task<Firsat> CreateFirsatAsync(Firsat firsat)
        {
            firsat.OlusturmaTarihi = DateTime.Now;
            _context.Firsatlar.Add(firsat);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Yeni fırsat eklendi: {firsat.Ad}");
            return firsat;
        }

        public async Task<Firsat> UpdateFirsatAsync(Firsat firsat)
        {
            var existingFirsat = await _context.Firsatlar.FindAsync(firsat.Id);
            if (existingFirsat == null)
            {
                throw new ArgumentException("Fırsat bulunamadı!");
            }

            existingFirsat.Ad = firsat.Ad;
            existingFirsat.Aciklama = firsat.Aciklama;
            existingFirsat.Tutar = firsat.Tutar;
            existingFirsat.Asama = firsat.Asama;
            existingFirsat.KapanisTarihi = firsat.KapanisTarihi;
            existingFirsat.GuncellemeTarihi = DateTime.Now;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Fırsat güncellendi: {firsat.Ad}");
            return existingFirsat;
        }

        public async Task DeleteFirsatAsync(int id)
        {
            var firsat = await _context.Firsatlar.FindAsync(id);
            if (firsat == null)
            {
                throw new ArgumentException("Fırsat bulunamadı!");
            }

            _context.Firsatlar.Remove(firsat);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Fırsat silindi: {firsat.Ad}");
        }

        // Görev Metodları
        public async Task<List<Gorev>> GetGorevlerAsync()
        {
            return await _context.Gorevler
                .Include(g => g.AtananKullanici)
                .Include(g => g.OlusturanKullanici)
                .Include(g => g.Firma)
                .Include(g => g.Firsat)
                .Where(g => g.Aktif)
                .OrderByDescending(g => g.OlusturmaTarihi)
                .ToListAsync();
        }

        public async Task<Gorev?> GetGorevByIdAsync(int id)
        {
            return await _context.Gorevler
                .Include(g => g.AtananKullanici)
                .Include(g => g.OlusturanKullanici)
                .Include(g => g.Firma)
                .Include(g => g.Firsat)
                .FirstOrDefaultAsync(g => g.Id == id && g.Aktif);
        }

        public async Task<Gorev> CreateGorevAsync(Gorev gorev)
        {
            gorev.OlusturmaTarihi = DateTime.Now;
            gorev.Aktif = true;
            
            _context.Gorevler.Add(gorev);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Yeni görev oluşturuldu: {gorev.Baslik}");
            return gorev;
        }

        public async Task<Gorev> UpdateGorevAsync(Gorev gorev)
        {
            var existingGorev = await _context.Gorevler.FindAsync(gorev.Id);
            if (existingGorev == null)
            {
                throw new ArgumentException("Görev bulunamadı!");
            }

            existingGorev.Baslik = gorev.Baslik;
            existingGorev.Aciklama = gorev.Aciklama;
            existingGorev.Durum = gorev.Durum;
            existingGorev.Oncelik = gorev.Oncelik;
            existingGorev.BaslangicTarihi = gorev.BaslangicTarihi;
            existingGorev.BitisTarihi = gorev.BitisTarihi;
            existingGorev.AtananKullaniciId = gorev.AtananKullaniciId;
            existingGorev.FirmaId = gorev.FirmaId;
            existingGorev.FirsatId = gorev.FirsatId;
            existingGorev.GuncellemeTarihi = DateTime.Now;

            // Eğer görev tamamlandıysa tamamlanma tarihini set et
            if (gorev.Durum == GorevDurumu.Tamamlandi && existingGorev.Durum != GorevDurumu.Tamamlandi)
            {
                existingGorev.TamamlanmaTarihi = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Görev güncellendi: {gorev.Baslik}");
            return existingGorev;
        }

        public async Task DeleteGorevAsync(int id)
        {
            var gorev = await _context.Gorevler.FindAsync(id);
            if (gorev == null)
            {
                throw new ArgumentException("Görev bulunamadı!");
            }

            gorev.Aktif = false; // Soft delete
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Görev silindi: {gorev.Baslik}");
        }

        public async Task<List<Gorev>> GetGorevlerByKullaniciAsync(int kullaniciId)
        {
            return await _context.Gorevler
                .Include(g => g.AtananKullanici)
                .Include(g => g.OlusturanKullanici)
                .Include(g => g.Firma)
                .Include(g => g.Firsat)
                .Where(g => g.Aktif && g.AtananKullaniciId == kullaniciId)
                .OrderByDescending(g => g.Oncelik)
                .ThenBy(g => g.BitisTarihi)
                .ToListAsync();
        }

        public async Task<List<Gorev>> GetGecikmisGorevlerAsync()
        {
            return await _context.Gorevler
                .Include(g => g.AtananKullanici)
                .Include(g => g.Firma)
                .Include(g => g.Firsat)
                .Where(g => g.Aktif && 
                           g.BitisTarihi.HasValue && 
                           g.BitisTarihi.Value < DateTime.Now && 
                           g.Durum != GorevDurumu.Tamamlandi)
                .OrderBy(g => g.BitisTarihi)
                .ToListAsync();
        }

        public async Task<List<Gorev>> GetBugunBitecekGorevlerAsync()
        {
            var bugun = DateTime.Today;
            return await _context.Gorevler
                .Include(g => g.AtananKullanici)
                .Include(g => g.Firma)
                .Include(g => g.Firsat)
                .Where(g => g.Aktif && 
                           g.BitisTarihi.HasValue && 
                           g.BitisTarihi.Value.Date == bugun && 
                           g.Durum != GorevDurumu.Tamamlandi)
                .OrderBy(g => g.BitisTarihi)
                .ToListAsync();
        }

        #endregion

        #region Kullanıcı İşlemleri
        public async Task<List<Kullanici>> GetKullanicilarAsync()
        {
            return await _context.Kullanicilar
                .Where(k => k.Aktif)
                .OrderBy(k => k.Ad)
                .ToListAsync();
        }

        public async Task<Kullanici?> GetKullaniciByIdAsync(int id)
        {
            return await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.Id == id && k.Aktif);
        }

        public async Task<Kullanici> CreateKullaniciAsync(Kullanici kullanici)
        {
            // Email kontrolü
            var existingUser = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.Email == kullanici.Email);
            
            if (existingUser != null)
            {
                throw new ArgumentException("Bu email adresi zaten kullanılıyor!");
            }

            kullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(kullanici.Sifre);
            kullanici.OlusturmaTarihi = DateTime.Now;
            kullanici.Aktif = true;

            _context.Kullanicilar.Add(kullanici);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Yeni kullanıcı eklendi: {kullanici.Email}");
            return kullanici;
        }

        public async Task<Kullanici> UpdateKullaniciAsync(Kullanici kullanici)
        {
            var existingKullanici = await _context.Kullanicilar.FindAsync(kullanici.Id);
            if (existingKullanici == null)
            {
                throw new ArgumentException("Kullanıcı bulunamadı!");
            }

            // Email kontrolü (kendisi hariç)
            var emailExists = await _context.Kullanicilar
                .AnyAsync(k => k.Email == kullanici.Email && k.Id != kullanici.Id);
            
            if (emailExists)
            {
                throw new ArgumentException("Bu email adresi zaten kullanılıyor!");
            }

            existingKullanici.Ad = kullanici.Ad;
            existingKullanici.Soyad = kullanici.Soyad;
            existingKullanici.Email = kullanici.Email;
            existingKullanici.Telefon = kullanici.Telefon;
            existingKullanici.Rol = kullanici.Rol;
            existingKullanici.Aktif = kullanici.Aktif;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Kullanıcı güncellendi: {kullanici.Email}");
            return existingKullanici;
        }

        public async Task DeleteKullaniciAsync(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                throw new ArgumentException("Kullanıcı bulunamadı!");
            }

            kullanici.Aktif = false; // Soft delete
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Kullanıcı silindi: {kullanici.Email}");
        }
        #endregion

        #region Dashboard Raporları
        public async Task<List<FirsatAsamaRaporu>> GetFirsatAsamalariRaporuAsync()
        {
            var firsatlar = await _context.Firsatlar.ToListAsync();
            var toplamFirsat = firsatlar.Count;

            return firsatlar
                .GroupBy(f => f.Asama)
                .Select(g => new FirsatAsamaRaporu
                {
                    Asama = (int)g.Key,
                    AsamaAdi = GetFirsatAsamaAdi((int)g.Key),
                    Sayi = g.Count(),
                    ToplamTutar = g.Sum(f => f.Tutar ?? 0),
                    OrtalamaTutar = g.Average(f => g.Count() > 0 ? f.Tutar ?? 0 : 0),
                    Yuzde = toplamFirsat > 0 ? (double)g.Count() / toplamFirsat * 100 : 0
                })
                .OrderBy(r => r.Asama)
                .ToList();
        }

        public async Task<List<AktiviteTrendRaporu>> GetAktiviteTrendRaporuAsync()
        {
            var son6Ay = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var aktiviteler = await _context.Aktiviteler
                .Where(a => a.Tarih >= son6Ay.First() && a.Tarih <= DateTime.Now)
                .ToListAsync();

            return son6Ay.Select(ay => new AktiviteTrendRaporu
            {
                Ay = ay.ToString("MMM yyyy"),
                Sayi = aktiviteler.Count(a => a.Tarih.Month == ay.Month && a.Tarih.Year == ay.Year)
            }).ToList();
        }

        public async Task<List<FirmaSehirRaporu>> GetFirmaSehirRaporuAsync()
        {
            var firmalar = await _context.Firmalar.ToListAsync();

            return firmalar
                .Where(f => !string.IsNullOrEmpty(f.Sehir))
                .GroupBy(f => f.Sehir)
                .Select(g => new FirmaSehirRaporu
                {
                    Sehir = g.Key!,
                    Sayi = g.Count()
                })
                .OrderByDescending(r => r.Sayi)
                .Take(10)
                .ToList();
        }

        public async Task<List<GorevDurumRaporu>> GetGorevDurumRaporuAsync()
        {
            var gorevler = await _context.Gorevler.Where(g => g.Aktif).ToListAsync();

            return gorevler
                .GroupBy(g => g.Durum)
                .Select(g => new GorevDurumRaporu
                {
                    Durum = (int)g.Key,
                    DurumAdi = GetGorevDurumAdi((int)g.Key),
                    Sayi = g.Count()
                })
                .OrderBy(r => r.Durum)
                .ToList();
        }

        public async Task<List<GelirTrendRaporu>> GetGelirTrendRaporuAsync()
        {
            var son6Ay = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var kazanilanFirsatlar = await _context.Firsatlar
                .Where(f => f.Asama == FirsatAsamasi.KapatildiKazanildi && 
                           f.KapanisTarihi.HasValue &&
                           f.KapanisTarihi.Value >= son6Ay.First())
                .ToListAsync();

            return son6Ay.Select(ay => new GelirTrendRaporu
            {
                Ay = ay.ToString("MMM yyyy"),
                Tutar = kazanilanFirsatlar
                    .Where(f => f.KapanisTarihi!.Value.Month == ay.Month && 
                               f.KapanisTarihi.Value.Year == ay.Year)
                    .Sum(f => f.Tutar ?? 0)
            }).ToList();
        }

        public async Task<List<Aktivite>> GetSonAktivitelerAsync(int count = 10)
        {
            return await _context.Aktiviteler
                .Include(a => a.Firma)
                .Include(a => a.Firsat)
                .OrderByDescending(a => a.Tarih)
                .Take(count)
                .ToListAsync();
        }

        private string GetFirsatAsamaAdi(int asama)
        {
            return asama switch
            {
                1 => "İlk İletişim",
                2 => "Teklif Hazırlama",
                3 => "Teklif Gönderildi",
                4 => "Müzakere",
                5 => "Kazanıldı",
                6 => "Kaybedildi",
                _ => "Bilinmiyor"
            };
        }

        private string GetGorevDurumAdi(int durum)
        {
            return durum switch
            {
                1 => "Beklemede",
                2 => "Devam Ediyor",
                3 => "Tamamlandı",
                4 => "İptal Edildi",
                _ => "Bilinmiyor"
            };
        }

        // Dashboard istatistik metodları
        public async Task<int> GetFirmaSayisiAsync()
        {
            return await _context.Firmalar.CountAsync();
        }

        public async Task<int> GetAktifFirsatSayisiAsync()
        {
            return await _context.Firsatlar
                .Where(f => f.Asama != FirsatAsamasi.KapatildiKazanildi && f.Asama != FirsatAsamasi.KapatildiKaybedildi)
                .CountAsync();
        }

        public async Task<int> GetBuAyAktiviteSayisiAsync()
        {
            var bugun = DateTime.Now;
            return await _context.Aktiviteler
                .Where(a => a.Tarih.Month == bugun.Month && a.Tarih.Year == bugun.Year)
                .CountAsync();
        }

        public async Task<int> GetKazanilanFirsatSayisiAsync()
        {
            return await _context.Firsatlar
                .Where(f => f.Asama == FirsatAsamasi.KapatildiKazanildi)
                .CountAsync();
        }
        #endregion

        #region Aktivite İşlemleri

        public async Task<List<Aktivite>> GetAktivitelerAsync(int? firmaId = null, int? firsatId = null, AktiviteTipi? tip = null)
        {
            var query = _context.Aktiviteler
                .Include(a => a.Firma)
                .Include(a => a.Firsat)
                .Include(a => a.Kullanici)
                .AsQueryable();

            if (firmaId.HasValue)
            {
                query = query.Where(a => a.FirmaId == firmaId.Value);
            }

            if (firsatId.HasValue)
            {
                query = query.Where(a => a.FirsatId == firsatId.Value);
            }

            if (tip.HasValue)
            {
                query = query.Where(a => a.Tip == tip.Value);
            }

            return await query.OrderByDescending(a => a.Tarih).ToListAsync();
        }

        public async Task<Aktivite> CreateAktiviteAsync(Aktivite aktivite)
        {
            aktivite.OlusturmaTarihi = DateTime.Now;
            _context.Aktiviteler.Add(aktivite);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Yeni aktivite eklendi: {aktivite.Baslik}");
            return aktivite;
        }

        public async Task<Aktivite> UpdateAktiviteAsync(Aktivite aktivite)
        {
            var existingAktivite = await _context.Aktiviteler.FindAsync(aktivite.Id);
            if (existingAktivite == null)
            {
                throw new ArgumentException("Aktivite bulunamadı!");
            }

            existingAktivite.Baslik = aktivite.Baslik;
            existingAktivite.Aciklama = aktivite.Aciklama;
            existingAktivite.Tip = aktivite.Tip;
            existingAktivite.Tarih = aktivite.Tarih;
            existingAktivite.FirmaId = aktivite.FirmaId;
            existingAktivite.FirsatId = aktivite.FirsatId;
            existingAktivite.GuncellemeTarihi = DateTime.Now;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Aktivite güncellendi: {aktivite.Baslik}");
            return existingAktivite;
        }

        public async Task DeleteAktiviteAsync(int id)
        {
            var aktivite = await _context.Aktiviteler.FindAsync(id);
            if (aktivite == null)
            {
                throw new ArgumentException("Aktivite bulunamadı!");
            }

            _context.Aktiviteler.Remove(aktivite);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Aktivite silindi: {aktivite.Baslik}");
        }

        #endregion

        #region İstatistikler

        public async Task<Dictionary<string, int>> GetDashboardStatsAsync()
        {
            var stats = new Dictionary<string, int>
            {
                ["ToplamFirma"] = await _context.Firmalar.CountAsync(),
                ["AktifFirsat"] = await _context.Firsatlar
                    .Where(f => f.Asama != FirsatAsamasi.KapatildiKazanildi && 
                               f.Asama != FirsatAsamasi.KapatildiKaybedildi)
                    .CountAsync(),
                ["BuAyAktivite"] = await _context.Aktiviteler
                    .Where(a => a.Tarih.Month == DateTime.Now.Month && 
                               a.Tarih.Year == DateTime.Now.Year)
                    .CountAsync(),
                ["KazanilanFirsat"] = await _context.Firsatlar
                    .Where(f => f.Asama == FirsatAsamasi.KapatildiKazanildi)
                    .CountAsync()
            };

            return stats;
        }

        #endregion
    }
}
