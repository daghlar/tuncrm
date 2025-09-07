using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TunCRM.Services;
using TunCRM.Models;
using System.ComponentModel.DataAnnotations;

namespace TunCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FirsatlarController : ControllerBase
    {
        private readonly CrmService _crmService;
        private readonly ILogger<FirsatlarController> _logger;

        public FirsatlarController(CrmService crmService, ILogger<FirsatlarController> logger)
        {
            _crmService = crmService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm fırsatları getirir
        /// </summary>
        /// <param name="search">Arama terimi</param>
        /// <param name="asama">Aşama filtresi</param>
        /// <param name="minTutar">Minimum tutar</param>
        /// <param name="maxTutar">Maksimum tutar</param>
        /// <param name="page">Sayfa numarası</param>
        /// <param name="pageSize">Sayfa boyutu</param>
        /// <returns>Fırsat listesi</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<FirsatDto>>>> GetFirsatlar(
            [FromQuery] string? search = null,
            [FromQuery] FirsatAsamasi? asama = null,
            [FromQuery] decimal? minTutar = null,
            [FromQuery] decimal? maxTutar = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var firsatlar = await _crmService.GetFirsatlarAsync();
                
                // Filtreleme
                if (!string.IsNullOrEmpty(search))
                {
                    firsatlar = firsatlar.Where(f => 
                        f.Ad.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        f.Aciklama.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (asama.HasValue)
                {
                    firsatlar = firsatlar.Where(f => f.Asama == asama.Value).ToList();
                }

                if (minTutar.HasValue)
                {
                    firsatlar = firsatlar.Where(f => f.Tutar >= minTutar.Value).ToList();
                }

                if (maxTutar.HasValue)
                {
                    firsatlar = firsatlar.Where(f => f.Tutar <= maxTutar.Value).ToList();
                }

                // Sayfalama
                var totalCount = firsatlar.Count;
                var pagedFirsatlar = firsatlar
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FirsatDto
                    {
                        Id = f.Id,
                        Ad = f.Ad,
                        Aciklama = f.Aciklama,
                        Asama = f.Asama,
                        AsamaAdi = GetAsamaAdi(f.Asama),
                        Tutar = f.Tutar,
                        FirmaId = f.FirmaId,
                        FirmaAdi = f.Firma?.Ad,
                        KullaniciId = f.KullaniciId,
                        KullaniciAdi = f.Kullanici?.TamAd,
                        OlusturmaTarihi = f.OlusturmaTarihi,
                        KapanisTarihi = f.KapanisTarihi
                    })
                    .ToList();

                return Ok(new ApiResponse<List<FirsatDto>>
                {
                    Success = true,
                    Data = pagedFirsatlar,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fırsatlar getirilirken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fırsatlar getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// ID'ye göre fırsat getirir
        /// </summary>
        /// <param name="id">Fırsat ID</param>
        /// <returns>Fırsat detayı</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FirsatDto>>> GetFirsat(int id)
        {
            try
            {
                var firsat = await _crmService.GetFirsatByIdAsync(id);
                if (firsat == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Fırsat bulunamadı"
                    });
                }

                var firsatDto = new FirsatDto
                {
                    Id = firsat.Id,
                    Ad = firsat.Ad,
                    Aciklama = firsat.Aciklama,
                    Asama = firsat.Asama,
                    AsamaAdi = GetAsamaAdi(firsat.Asama),
                    Tutar = firsat.Tutar,
                    FirmaId = firsat.FirmaId,
                    FirmaAdi = firsat.Firma?.Ad,
                    KullaniciId = firsat.KullaniciId,
                    KullaniciAdi = firsat.Kullanici?.TamAd,
                    OlusturmaTarihi = firsat.OlusturmaTarihi,
                    KapanisTarihi = firsat.KapanisTarihi
                };

                return Ok(new ApiResponse<FirsatDto>
                {
                    Success = true,
                    Data = firsatDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fırsat getirilirken hata oluştu: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fırsat getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Yeni fırsat oluşturur
        /// </summary>
        /// <param name="request">Fırsat oluşturma isteği</param>
        /// <returns>Oluşturulan fırsat</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<FirsatDto>>> CreateFirsat([FromBody] CreateFirsatRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Geçersiz veri",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var firsat = new Firsat
                {
                    Ad = request.Ad,
                    Aciklama = request.Aciklama,
                    Asama = request.Asama,
                    Tutar = request.Tutar,
                    FirmaId = request.FirmaId,
                    KullaniciId = request.KullaniciId,
                    OlusturmaTarihi = DateTime.Now
                };

                var createdFirsat = await _crmService.CreateFirsatAsync(firsat);
                
                var firsatDto = new FirsatDto
                {
                    Id = createdFirsat.Id,
                    Ad = createdFirsat.Ad,
                    Aciklama = createdFirsat.Aciklama,
                    Asama = createdFirsat.Asama,
                    AsamaAdi = GetAsamaAdi(createdFirsat.Asama),
                    Tutar = createdFirsat.Tutar,
                    FirmaId = createdFirsat.FirmaId,
                    FirmaAdi = createdFirsat.Firma?.Ad,
                    KullaniciId = createdFirsat.KullaniciId,
                    KullaniciAdi = createdFirsat.Kullanici?.TamAd,
                    OlusturmaTarihi = createdFirsat.OlusturmaTarihi,
                    KapanisTarihi = createdFirsat.KapanisTarihi
                };

                return CreatedAtAction(nameof(GetFirsat), new { id = createdFirsat.Id }, 
                    new ApiResponse<FirsatDto>
                    {
                        Success = true,
                        Data = firsatDto,
                        Message = "Fırsat başarıyla oluşturuldu"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fırsat oluşturulurken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fırsat oluşturulurken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Fırsat günceller
        /// </summary>
        /// <param name="id">Fırsat ID</param>
        /// <param name="request">Fırsat güncelleme isteği</param>
        /// <returns>Güncellenen fırsat</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<FirsatDto>>> UpdateFirsat(int id, [FromBody] UpdateFirsatRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Geçersiz veri",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var existingFirsat = await _crmService.GetFirsatByIdAsync(id);
                if (existingFirsat == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Fırsat bulunamadı"
                    });
                }

                existingFirsat.Ad = request.Ad;
                existingFirsat.Aciklama = request.Aciklama;
                existingFirsat.Asama = request.Asama;
                existingFirsat.Tutar = request.Tutar;
                existingFirsat.FirmaId = request.FirmaId;
                existingFirsat.KullaniciId = request.KullaniciId;
                existingFirsat.GuncellemeTarihi = DateTime.Now;

                if (request.Asama == FirsatAsamasi.KapatildiKazanildi || request.Asama == FirsatAsamasi.KapatildiKaybedildi)
                {
                    existingFirsat.KapanisTarihi = DateTime.Now;
                }

                await _crmService.UpdateFirsatAsync(existingFirsat);

                var firsatDto = new FirsatDto
                {
                    Id = existingFirsat.Id,
                    Ad = existingFirsat.Ad,
                    Aciklama = existingFirsat.Aciklama,
                    Asama = existingFirsat.Asama,
                    AsamaAdi = GetAsamaAdi(existingFirsat.Asama),
                    Tutar = existingFirsat.Tutar,
                    FirmaId = existingFirsat.FirmaId,
                    FirmaAdi = existingFirsat.Firma?.Ad,
                    KullaniciId = existingFirsat.KullaniciId,
                    KullaniciAdi = existingFirsat.Kullanici?.TamAd,
                    OlusturmaTarihi = existingFirsat.OlusturmaTarihi,
                    KapanisTarihi = existingFirsat.KapanisTarihi
                };

                return Ok(new ApiResponse<FirsatDto>
                {
                    Success = true,
                    Data = firsatDto,
                    Message = "Fırsat başarıyla güncellendi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fırsat güncellenirken hata oluştu: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fırsat güncellenirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Fırsat siler
        /// </summary>
        /// <param name="id">Fırsat ID</param>
        /// <returns>Silme sonucu</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteFirsat(int id)
        {
            try
            {
                var existingFirsat = await _crmService.GetFirsatByIdAsync(id);
                if (existingFirsat == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Fırsat bulunamadı"
                    });
                }

                await _crmService.DeleteFirsatAsync(id);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Fırsat başarıyla silindi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fırsat silinirken hata oluştu: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fırsat silinirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Fırsat aşamasını günceller
        /// </summary>
        /// <param name="id">Fırsat ID</param>
        /// <param name="request">Aşama güncelleme isteği</param>
        /// <returns>Güncellenen fırsat</returns>
        [HttpPatch("{id}/asama")]
        public async Task<ActionResult<ApiResponse<FirsatDto>>> UpdateAsama(int id, [FromBody] UpdateAsamaRequest request)
        {
            try
            {
                var existingFirsat = await _crmService.GetFirsatByIdAsync(id);
                if (existingFirsat == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Fırsat bulunamadı"
                    });
                }

                existingFirsat.Asama = request.Asama;
                existingFirsat.GuncellemeTarihi = DateTime.Now;

                if (request.Asama == FirsatAsamasi.KapatildiKazanildi || request.Asama == FirsatAsamasi.KapatildiKaybedildi)
                {
                    existingFirsat.KapanisTarihi = DateTime.Now;
                }

                await _crmService.UpdateFirsatAsync(existingFirsat);

                var firsatDto = new FirsatDto
                {
                    Id = existingFirsat.Id,
                    Ad = existingFirsat.Ad,
                    Aciklama = existingFirsat.Aciklama,
                    Asama = existingFirsat.Asama,
                    AsamaAdi = GetAsamaAdi(existingFirsat.Asama),
                    Tutar = existingFirsat.Tutar,
                    FirmaId = existingFirsat.FirmaId,
                    FirmaAdi = existingFirsat.Firma?.Ad,
                    KullaniciId = existingFirsat.KullaniciId,
                    KullaniciAdi = existingFirsat.Kullanici?.TamAd,
                    OlusturmaTarihi = existingFirsat.OlusturmaTarihi,
                    KapanisTarihi = existingFirsat.KapanisTarihi
                };

                return Ok(new ApiResponse<FirsatDto>
                {
                    Success = true,
                    Data = firsatDto,
                    Message = "Fırsat aşaması başarıyla güncellendi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fırsat aşaması güncellenirken hata oluştu: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fırsat aşaması güncellenirken hata oluştu"
                });
            }
        }

        private string GetAsamaAdi(FirsatAsamasi asama)
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
    }

    // DTOs
    public class FirsatDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public FirsatAsamasi Asama { get; set; }
        public string AsamaAdi { get; set; } = string.Empty;
        public decimal? Tutar { get; set; }
        public int FirmaId { get; set; }
        public string? FirmaAdi { get; set; }
        public int KullaniciId { get; set; }
        public string? KullaniciAdi { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public DateTime? KapanisTarihi { get; set; }
    }

    public class CreateFirsatRequest
    {
        [Required(ErrorMessage = "Fırsat adı gereklidir")]
        [StringLength(200, ErrorMessage = "Fırsat adı en fazla 200 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Aşama gereklidir")]
        public FirsatAsamasi Asama { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
        public decimal? Tutar { get; set; }

        [Required(ErrorMessage = "Firma ID gereklidir")]
        public int FirmaId { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID gereklidir")]
        public int KullaniciId { get; set; }
    }

    public class UpdateFirsatRequest
    {
        [Required(ErrorMessage = "Fırsat adı gereklidir")]
        [StringLength(200, ErrorMessage = "Fırsat adı en fazla 200 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Aşama gereklidir")]
        public FirsatAsamasi Asama { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
        public decimal? Tutar { get; set; }

        [Required(ErrorMessage = "Firma ID gereklidir")]
        public int FirmaId { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID gereklidir")]
        public int KullaniciId { get; set; }
    }

    public class UpdateAsamaRequest
    {
        [Required(ErrorMessage = "Aşama gereklidir")]
        public FirsatAsamasi Asama { get; set; }
    }
}
