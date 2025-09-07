using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TunCRM.Services;
using TunCRM.Models;

namespace TunCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly CrmService _crmService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(CrmService crmService, ILogger<DashboardController> logger)
        {
            _crmService = crmService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard istatistiklerini getirir
        /// </summary>
        /// <returns>Dashboard istatistikleri</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
        {
            try
            {
                var stats = new DashboardStatsDto
                {
                    ToplamFirma = await _crmService.GetFirmaSayisiAsync(),
                    ToplamFirsat = await _crmService.GetFirsatlarAsync().ContinueWith(t => t.Result.Count),
                    ToplamAktivite = await _crmService.GetAktivitelerAsync().ContinueWith(t => t.Result.Count),
                    ToplamGorev = await _crmService.GetGorevlerAsync().ContinueWith(t => t.Result.Count),
                    AktifFirsat = await _crmService.GetAktifFirsatSayisiAsync(),
                    BuAyAktivite = await _crmService.GetBuAyAktiviteSayisiAsync(),
                    KazanilanFirsat = await _crmService.GetKazanilanFirsatSayisiAsync()
                };

                return Ok(new ApiResponse<DashboardStatsDto>
                {
                    Success = true,
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard istatistikleri getirilirken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dashboard istatistikleri getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Fırsat aşamaları raporunu getirir
        /// </summary>
        /// <returns>Fırsat aşamaları raporu</returns>
        [HttpGet("firsat-asamalari")]
        public async Task<ActionResult<ApiResponse<List<FirsatAsamaRaporu>>>> GetFirsatAsamalari()
        {
            try
            {
                var rapor = await _crmService.GetFirsatAsamalariRaporuAsync();
                
                return Ok(new ApiResponse<List<FirsatAsamaRaporu>>
                {
                    Success = true,
                    Data = rapor
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fırsat aşamaları raporu getirilirken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fırsat aşamaları raporu getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Aktivite trend raporunu getirir
        /// </summary>
        /// <returns>Aktivite trend raporu</returns>
        [HttpGet("aktivite-trend")]
        public async Task<ActionResult<ApiResponse<List<AktiviteTrendRaporu>>>> GetAktiviteTrend()
        {
            try
            {
                var rapor = await _crmService.GetAktiviteTrendRaporuAsync();
                
                return Ok(new ApiResponse<List<AktiviteTrendRaporu>>
                {
                    Success = true,
                    Data = rapor
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktivite trend raporu getirilirken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Aktivite trend raporu getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Firma şehir dağılımı raporunu getirir
        /// </summary>
        /// <returns>Firma şehir dağılımı raporu</returns>
        [HttpGet("firma-sehir")]
        public async Task<ActionResult<ApiResponse<List<FirmaSehirRaporu>>>> GetFirmaSehir()
        {
            try
            {
                var rapor = await _crmService.GetFirmaSehirRaporuAsync();
                
                return Ok(new ApiResponse<List<FirmaSehirRaporu>>
                {
                    Success = true,
                    Data = rapor
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firma şehir dağılımı raporu getirilirken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Firma şehir dağılımı raporu getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Görev durumları raporunu getirir
        /// </summary>
        /// <returns>Görev durumları raporu</returns>
        [HttpGet("gorev-durumlari")]
        public async Task<ActionResult<ApiResponse<List<GorevDurumRaporu>>>> GetGorevDurumlari()
        {
            try
            {
                var rapor = await _crmService.GetGorevDurumRaporuAsync();
                
                return Ok(new ApiResponse<List<GorevDurumRaporu>>
                {
                    Success = true,
                    Data = rapor
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görev durumları raporu getirilirken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Görev durumları raporu getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Gelir trend raporunu getirir
        /// </summary>
        /// <returns>Gelir trend raporu</returns>
        [HttpGet("gelir-trend")]
        public async Task<ActionResult<ApiResponse<List<GelirTrendRaporu>>>> GetGelirTrend()
        {
            try
            {
                var rapor = await _crmService.GetGelirTrendRaporuAsync();
                
                return Ok(new ApiResponse<List<GelirTrendRaporu>>
                {
                    Success = true,
                    Data = rapor
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelir trend raporu getirilirken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Gelir trend raporu getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Son aktiviteleri getirir
        /// </summary>
        /// <param name="count">Getirilecek aktivite sayısı</param>
        /// <returns>Son aktiviteler</returns>
        [HttpGet("son-aktiviteler")]
        public async Task<ActionResult<ApiResponse<List<AktiviteDto>>>> GetSonAktiviteler([FromQuery] int count = 10)
        {
            try
            {
                var aktiviteler = await _crmService.GetSonAktivitelerAsync(count);
                var aktiviteDtos = aktiviteler.Select(a => new AktiviteDto
                {
                    Id = a.Id,
                    Baslik = a.Baslik,
                    Aciklama = a.Aciklama,
                    Tip = a.Tip,
                    TipAdi = GetAktiviteTipiAdi(a.Tip),
                    Tarih = a.Tarih,
                    FirmaId = a.FirmaId,
                    FirmaAdi = a.Firma?.Ad,
                    FirsatId = a.FirsatId,
                    FirsatAdi = a.Firsat?.Ad,
                    KullaniciId = a.KullaniciId,
                    KullaniciAdi = a.Kullanici?.TamAd
                }).ToList();
                
                return Ok(new ApiResponse<List<AktiviteDto>>
                {
                    Success = true,
                    Data = aktiviteDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Son aktiviteler getirilirken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Son aktiviteler getirilirken hata oluştu"
                });
            }
        }

        private string GetAktiviteTipiAdi(AktiviteTipi tip)
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

    // DTOs
    public class DashboardStatsDto
    {
        public int ToplamFirma { get; set; }
        public int ToplamFirsat { get; set; }
        public int ToplamAktivite { get; set; }
        public int ToplamGorev { get; set; }
        public int AktifFirsat { get; set; }
        public int BuAyAktivite { get; set; }
        public int KazanilanFirsat { get; set; }
    }

    public class AktiviteDto
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public AktiviteTipi Tip { get; set; }
        public string TipAdi { get; set; } = string.Empty;
        public DateTime Tarih { get; set; }
        public int? FirmaId { get; set; }
        public string? FirmaAdi { get; set; }
        public int? FirsatId { get; set; }
        public string? FirsatAdi { get; set; }
        public int KullaniciId { get; set; }
        public string? KullaniciAdi { get; set; }
    }
}
