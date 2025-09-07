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
    public class FirmalarController : ControllerBase
    {
        private readonly CrmService _crmService;
        private readonly ILogger<FirmalarController> _logger;

        public FirmalarController(CrmService crmService, ILogger<FirmalarController> logger)
        {
            _crmService = crmService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm firmaları getirir
        /// </summary>
        /// <param name="search">Arama terimi</param>
        /// <param name="sehir">Şehir filtresi</param>
        /// <param name="page">Sayfa numarası</param>
        /// <param name="pageSize">Sayfa boyutu</param>
        /// <returns>Firma listesi</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<FirmaDto>>>> GetFirmalar(
            [FromQuery] string? search = null,
            [FromQuery] string? sehir = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var firmalar = await _crmService.GetFirmalarAsync();
                
                // Filtreleme
                if (!string.IsNullOrEmpty(search))
                {
                    firmalar = firmalar.Where(f => 
                        f.Ad.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        f.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        f.Telefon.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(sehir))
                {
                    firmalar = firmalar.Where(f => 
                        f.Sehir.Contains(sehir, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Sayfalama
                var totalCount = firmalar.Count;
                var pagedFirmalar = firmalar
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FirmaDto
                    {
                        Id = f.Id,
                        Ad = f.Ad,
                        Email = f.Email,
                        Telefon = f.Telefon,
                        Sehir = f.Sehir,
                        Adres = f.Adres,
                        WebSitesi = f.WebSitesi,
                        Notlar = f.Notlar,
                        OlusturmaTarihi = f.OlusturmaTarihi
                    })
                    .ToList();

                return Ok(new ApiResponse<List<FirmaDto>>
                {
                    Success = true,
                    Data = pagedFirmalar,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firmalar getirilirken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Firmalar getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// ID'ye göre firma getirir
        /// </summary>
        /// <param name="id">Firma ID</param>
        /// <returns>Firma detayı</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FirmaDto>>> GetFirma(int id)
        {
            try
            {
                var firma = await _crmService.GetFirmaByIdAsync(id);
                if (firma == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Firma bulunamadı"
                    });
                }

                var firmaDto = new FirmaDto
                {
                    Id = firma.Id,
                    Ad = firma.Ad,
                    Email = firma.Email,
                    Telefon = firma.Telefon,
                    Sehir = firma.Sehir,
                    Adres = firma.Adres,
                    WebSitesi = firma.WebSitesi,
                    Notlar = firma.Notlar,
                    OlusturmaTarihi = firma.OlusturmaTarihi
                };

                return Ok(new ApiResponse<FirmaDto>
                {
                    Success = true,
                    Data = firmaDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firma getirilirken hata oluştu: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Firma getirilirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Yeni firma oluşturur
        /// </summary>
        /// <param name="request">Firma oluşturma isteği</param>
        /// <returns>Oluşturulan firma</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<FirmaDto>>> CreateFirma([FromBody] CreateFirmaRequest request)
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

                var firma = new Firma
                {
                    Ad = request.Ad,
                    Email = request.Email,
                    Telefon = request.Telefon,
                    Sehir = request.Sehir,
                    Adres = request.Adres,
                    WebSitesi = request.WebSitesi,
                    Notlar = request.Notlar,
                    OlusturmaTarihi = DateTime.Now
                };

                var createdFirma = await _crmService.CreateFirmaAsync(firma);
                
                var firmaDto = new FirmaDto
                {
                    Id = createdFirma.Id,
                    Ad = createdFirma.Ad,
                    Email = createdFirma.Email,
                    Telefon = createdFirma.Telefon,
                    Sehir = createdFirma.Sehir,
                    Adres = createdFirma.Adres,
                    WebSitesi = createdFirma.WebSitesi,
                    Notlar = createdFirma.Notlar,
                    OlusturmaTarihi = createdFirma.OlusturmaTarihi
                };

                return CreatedAtAction(nameof(GetFirma), new { id = createdFirma.Id }, 
                    new ApiResponse<FirmaDto>
                    {
                        Success = true,
                        Data = firmaDto,
                        Message = "Firma başarıyla oluşturuldu"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firma oluşturulurken hata oluştu");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Firma oluşturulurken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Firma günceller
        /// </summary>
        /// <param name="id">Firma ID</param>
        /// <param name="request">Firma güncelleme isteği</param>
        /// <returns>Güncellenen firma</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<FirmaDto>>> UpdateFirma(int id, [FromBody] UpdateFirmaRequest request)
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

                var existingFirma = await _crmService.GetFirmaByIdAsync(id);
                if (existingFirma == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Firma bulunamadı"
                    });
                }

                existingFirma.Ad = request.Ad;
                existingFirma.Email = request.Email;
                existingFirma.Telefon = request.Telefon;
                existingFirma.Sehir = request.Sehir;
                existingFirma.Adres = request.Adres;
                existingFirma.WebSitesi = request.WebSitesi;
                existingFirma.Notlar = request.Notlar;
                existingFirma.GuncellemeTarihi = DateTime.Now;

                await _crmService.UpdateFirmaAsync(existingFirma);

                var firmaDto = new FirmaDto
                {
                    Id = existingFirma.Id,
                    Ad = existingFirma.Ad,
                    Email = existingFirma.Email,
                    Telefon = existingFirma.Telefon,
                    Sehir = existingFirma.Sehir,
                    Adres = existingFirma.Adres,
                    WebSitesi = existingFirma.WebSitesi,
                    Notlar = existingFirma.Notlar,
                    OlusturmaTarihi = existingFirma.OlusturmaTarihi
                };

                return Ok(new ApiResponse<FirmaDto>
                {
                    Success = true,
                    Data = firmaDto,
                    Message = "Firma başarıyla güncellendi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firma güncellenirken hata oluştu: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Firma güncellenirken hata oluştu"
                });
            }
        }

        /// <summary>
        /// Firma siler
        /// </summary>
        /// <param name="id">Firma ID</param>
        /// <returns>Silme sonucu</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteFirma(int id)
        {
            try
            {
                var existingFirma = await _crmService.GetFirmaByIdAsync(id);
                if (existingFirma == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Firma bulunamadı"
                    });
                }

                await _crmService.DeleteFirmaAsync(id);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Firma başarıyla silindi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firma silinirken hata oluştu: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Firma silinirken hata oluştu"
                });
            }
        }
    }

    // DTOs
    public class FirmaDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefon { get; set; }
        public string? Sehir { get; set; }
        public string? Adres { get; set; }
        public string? WebSitesi { get; set; }
        public string? Notlar { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
    }

    public class CreateFirmaRequest
    {
        [Required(ErrorMessage = "Firma adı gereklidir")]
        [StringLength(200, ErrorMessage = "Firma adı en fazla 200 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? Telefon { get; set; }

        [StringLength(100, ErrorMessage = "Şehir en fazla 100 karakter olabilir")]
        public string? Sehir { get; set; }

        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string? Adres { get; set; }

        [Url(ErrorMessage = "Geçerli bir web sitesi URL'si giriniz")]
        public string? WebSitesi { get; set; }

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notlar { get; set; }
    }

    public class UpdateFirmaRequest
    {
        [Required(ErrorMessage = "Firma adı gereklidir")]
        [StringLength(200, ErrorMessage = "Firma adı en fazla 200 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? Telefon { get; set; }

        [StringLength(100, ErrorMessage = "Şehir en fazla 100 karakter olabilir")]
        public string? Sehir { get; set; }

        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string? Adres { get; set; }

        [Url(ErrorMessage = "Geçerli bir web sitesi URL'si giriniz")]
        public string? WebSitesi { get; set; }

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notlar { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public int? TotalCount { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
