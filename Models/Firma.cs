using System.ComponentModel.DataAnnotations;

namespace TunCRM.Models
{
    public class Firma
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Firma adı gereklidir")]
        [StringLength(200, ErrorMessage = "Firma adı en fazla 200 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string? Adres { get; set; }
        
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir")]
        public string? Telefon { get; set; }
        
        [StringLength(100, ErrorMessage = "Email en fazla 100 karakter olabilir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string? Email { get; set; }
        
        [StringLength(200, ErrorMessage = "Web sitesi en fazla 200 karakter olabilir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string? WebSitesi { get; set; }
        
        [StringLength(100, ErrorMessage = "Şehir en fazla 100 karakter olabilir")]
        public string? Sehir { get; set; }
        
        [StringLength(100, ErrorMessage = "İlçe en fazla 100 karakter olabilir")]
        public string? Ilce { get; set; }
        
        [StringLength(50, ErrorMessage = "Posta kodu en fazla 50 karakter olabilir")]
        public string? PostaKodu { get; set; }
        
        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notlar { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Navigation properties
        public virtual ICollection<Firsat> Firsatlar { get; set; } = new List<Firsat>();
        public virtual ICollection<Aktivite> Aktiviteler { get; set; } = new List<Aktivite>();
    }
}
