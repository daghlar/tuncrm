using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TunCRM.Data;
using TunCRM.Models;

namespace TunCRM.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var kullanici = await _context.Kullanicilar
                    .FirstOrDefaultAsync(k => k.Email == email && k.Aktif);

                if (kullanici == null)
                {
                    return new AuthResult { Success = false, Message = "Kullanıcı bulunamadı veya hesap aktif değil." };
                }

                if (!BCrypt.Net.BCrypt.Verify(password, kullanici.Sifre))
                {
                    return new AuthResult { Success = false, Message = "Geçersiz şifre." };
                }

                // Son giriş tarihini güncelle
                kullanici.SonGirisTarihi = DateTime.Now;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(kullanici);

                _logger.LogInformation($"Kullanıcı giriş yaptı: {email}");

                return new AuthResult
                {
                    Success = true,
                    Token = token,
                    User = new UserInfo
                    {
                        Id = kullanici.Id,
                        Ad = kullanici.Ad,
                        Soyad = kullanici.Soyad,
                        Email = kullanici.Email,
                        Rol = kullanici.Rol,
                        TamAd = kullanici.TamAd
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş sırasında hata oluştu: {Email}", email);
                return new AuthResult { Success = false, Message = "Giriş sırasında bir hata oluştu." };
            }
        }

        public async Task<AuthResult> RegisterAsync(string ad, string soyad, string email, string password, KullaniciRolu rol = KullaniciRolu.User)
        {
            try
            {
                // Email kontrolü
                var existingUser = await _context.Kullanicilar
                    .FirstOrDefaultAsync(k => k.Email == email);

                if (existingUser != null)
                {
                    return new AuthResult { Success = false, Message = "Bu email adresi zaten kullanılıyor." };
                }

                // Şifreyi hashle
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var kullanici = new Kullanici
                {
                    Ad = ad,
                    Soyad = soyad,
                    Email = email,
                    Sifre = hashedPassword,
                    Rol = rol,
                    Aktif = true,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.Kullanicilar.Add(kullanici);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(kullanici);

                _logger.LogInformation($"Yeni kullanıcı kaydoldu: {email}");

                return new AuthResult
                {
                    Success = true,
                    Token = token,
                    User = new UserInfo
                    {
                        Id = kullanici.Id,
                        Ad = kullanici.Ad,
                        Soyad = kullanici.Soyad,
                        Email = kullanici.Email,
                        Rol = kullanici.Rol,
                        TamAd = kullanici.TamAd
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt sırasında hata oluştu: {Email}", email);
                return new AuthResult { Success = false, Message = "Kayıt sırasında bir hata oluştu." };
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var kullanici = await _context.Kullanicilar.FindAsync(userId);
                if (kullanici == null) return false;

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, kullanici.Sifre))
                {
                    return false;
                }

                kullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Kullanıcı şifresini değiştirdi: {kullanici.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirme sırasında hata oluştu: {UserId}", userId);
                return false;
            }
        }

        public async Task<UserInfo?> GetUserByIdAsync(int userId)
        {
            try
            {
                var kullanici = await _context.Kullanicilar
                    .FirstOrDefaultAsync(k => k.Id == userId && k.Aktif);

                if (kullanici == null) return null;

                return new UserInfo
                {
                    Id = kullanici.Id,
                    Ad = kullanici.Ad,
                    Soyad = kullanici.Soyad,
                    Email = kullanici.Email,
                    Rol = kullanici.Rol,
                    TamAd = kullanici.TamAd
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgisi alınırken hata oluştu: {UserId}", userId);
                return null;
            }
        }

        private string GenerateJwtToken(Kullanici kullanici)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "TunCRM_Super_Secret_Key_2024_At_Least_32_Characters";
            var issuer = jwtSettings["Issuer"] ?? "TunCRM";
            var audience = jwtSettings["Audience"] ?? "TunCRM_Users";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                new Claim(ClaimTypes.Name, kullanici.Email),
                new Claim(ClaimTypes.GivenName, kullanici.Ad),
                new Claim(ClaimTypes.Surname, kullanici.Soyad),
                new Claim(ClaimTypes.Role, kullanici.Rol.ToString()),
                new Claim("FullName", kullanici.TamAd)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public KullaniciRolu Rol { get; set; }
        public string TamAd { get; set; } = string.Empty;
    }
}
