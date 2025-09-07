using System.Net.Http;
using HtmlAgilityPack;
using TunCRM.Models;
using System.Text.RegularExpressions;

namespace TunCRM.Services
{
    public class GoogleMapsScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleMapsScraperService> _logger;

        public GoogleMapsScraperService(HttpClient httpClient, ILogger<GoogleMapsScraperService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ScrapedCompany>> ScrapeCompaniesAsync(string searchQuery, string location, int maxResults = 50)
        {
            try
            {
                _logger.LogInformation($"Google Maps scraping başlatıldı: {searchQuery} in {location}");

                var companies = new List<ScrapedCompany>();
                var searchUrl = BuildSearchUrl(searchQuery, location);
                
                // User-Agent header ekle
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                var html = await _httpClient.GetStringAsync(searchUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Google Maps sonuçlarını parse et
                var resultElements = doc.DocumentNode
                    .SelectNodes("//div[contains(@class, 'Nv2PK') or contains(@class, 'THOPZb')]")
                    ?.Take(maxResults);

                if (resultElements != null)
                {
                    foreach (var element in resultElements)
                    {
                        try
                        {
                            var company = ParseCompanyElement(element);
                            if (company != null)
                            {
                                companies.Add(company);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Şirket parse edilirken hata: {ex.Message}");
                        }
                    }
                }

                _logger.LogInformation($"{companies.Count} şirket bulundu");
                return companies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Maps scraping hatası");
                throw;
            }
        }

        private string BuildSearchUrl(string query, string location)
        {
            var encodedQuery = Uri.EscapeDataString($"{query} {location}");
            return $"https://www.google.com/maps/search/{encodedQuery}";
        }

        private ScrapedCompany? ParseCompanyElement(HtmlNode element)
        {
            try
            {
                var company = new ScrapedCompany();

                // Şirket adı
                var nameElement = element.SelectSingleNode(".//div[contains(@class, 'qBF1Pd')]") ??
                                 element.SelectSingleNode(".//h3[contains(@class, 'fontHeadlineSmall')]");
                company.Name = nameElement?.InnerText?.Trim() ?? "Bilinmeyen Şirket";

                // Adres
                var addressElement = element.SelectSingleNode(".//div[contains(@class, 'W4Efsd')]") ??
                                   element.SelectSingleNode(".//div[contains(@class, 'fontBodyMedium')]");
                company.Address = addressElement?.InnerText?.Trim() ?? "";

                // Telefon
                var phoneElement = element.SelectSingleNode(".//span[contains(@class, 'UsdlK')]") ??
                                 element.SelectSingleNode(".//span[contains(text(), '+')]");
                company.Phone = phoneElement?.InnerText?.Trim() ?? "";

                // Website
                var websiteElement = element.SelectSingleNode(".//a[contains(@href, 'http')]");
                company.Website = websiteElement?.GetAttributeValue("href", "") ?? "";

                // Rating
                var ratingElement = element.SelectSingleNode(".//span[contains(@class, 'MW4etd')]");
                if (ratingElement != null && decimal.TryParse(ratingElement.InnerText?.Trim(), out var rating))
                {
                    company.Rating = rating;
                }

                // Açıklama
                var descriptionElement = element.SelectSingleNode(".//div[contains(@class, 'W4Efsd')]");
                company.Description = descriptionElement?.InnerText?.Trim() ?? "";

                // Şehir bilgisini adresten çıkar
                company.City = ExtractCityFromAddress(company.Address);

                // Email'i website'den türet
                company.Email = GenerateEmailFromWebsite(company.Website);

                return company;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Şirket element parse hatası: {ex.Message}");
                return null;
            }
        }

        private string ExtractCityFromAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                return "";

            // Türkiye şehirleri
            var cities = new[] { "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Adana", "Konya", "Gaziantep", 
                               "Mersin", "Diyarbakır", "Kayseri", "Eskişehir", "Urfa", "Malatya", "Erzurum" };

            foreach (var city in cities)
            {
                if (address.Contains(city, StringComparison.OrdinalIgnoreCase))
                    return city;
            }

            return "";
        }

        private string GenerateEmailFromWebsite(string website)
        {
            if (string.IsNullOrEmpty(website))
                return "";

            try
            {
                var uri = new Uri(website);
                var domain = uri.Host.Replace("www.", "");
                return $"info@{domain}";
            }
            catch
            {
                return "";
            }
        }

        public async Task<List<ScrapedCompany>> ScrapeByCategoryAsync(string category, string location, int maxResults = 30)
        {
            var searchQueries = new[]
            {
                $"{category} {location}",
                $"{category} şirketleri {location}",
                $"{category} firmaları {location}",
                $"{category} işletmeleri {location}"
            };

            var allCompanies = new List<ScrapedCompany>();
            var companyNames = new HashSet<string>();

            foreach (var query in searchQueries)
            {
                try
                {
                    var companies = await ScrapeCompaniesAsync(query, location, maxResults / searchQueries.Length);
                    
                    foreach (var company in companies)
                    {
                        if (!companyNames.Contains(company.Name))
                        {
                            companyNames.Add(company.Name);
                            allCompanies.Add(company);
                        }
                    }

                    // Rate limiting
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Kategori scraping hatası ({query}): {ex.Message}");
                }
            }

            return allCompanies.Take(maxResults).ToList();
        }
    }

    public class ScrapedCompany
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Rating { get; set; }
        public DateTime ScrapedAt { get; set; } = DateTime.Now;
    }
}