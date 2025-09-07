using System.ComponentModel.DataAnnotations;

namespace TunCRM.Models
{
    public enum KullaniciRolu
    {
        [Display(Name = "Admin")]
        Admin = 1,
        
        [Display(Name = "Yönetici")]
        Manager = 2,
        
        [Display(Name = "Satış Temsilcisi")]
        Sales = 3,
        
        [Display(Name = "Kullanıcı")]
        User = 4
    }

    public class Kullanici
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string Soyad { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email gereklidir")]
        [StringLength(100, ErrorMessage = "Email en fazla 100 karakter olabilir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir")]
        public string? Telefon { get; set; }
        
        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, ErrorMessage = "Şifre en fazla 100 karakter olabilir")]
        public string Sifre { get; set; } = string.Empty;
        
        public KullaniciRolu Rol { get; set; } = KullaniciRolu.User;
        
        public bool Aktif { get; set; } = true;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? SonGirisTarihi { get; set; }
        
        // Navigation properties
        public virtual ICollection<Firsat> Firsatlar { get; set; } = new List<Firsat>();
        public virtual ICollection<Aktivite> Aktiviteler { get; set; } = new List<Aktivite>();
        
        // Computed property
        public string TamAd => $"{Ad} {Soyad}";
    }
}
