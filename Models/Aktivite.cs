using System.ComponentModel.DataAnnotations;

namespace TunCRM.Models
{
    public enum AktiviteTipi
    {
        [Display(Name = "Telefon")]
        Telefon = 1,
        
        [Display(Name = "Email")]
        Email = 2,
        
        [Display(Name = "Toplantı")]
        Toplanti = 3,
        
        [Display(Name = "Not")]
        Not = 4,
        
        [Display(Name = "Görev")]
        Gorev = 5
    }

    public class Aktivite
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Aktivite başlığı gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Baslik { get; set; } = string.Empty;
        
        [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
        public string? Aciklama { get; set; }
        
        public AktiviteTipi Tip { get; set; } = AktiviteTipi.Not;
        
        public DateTime Tarih { get; set; } = DateTime.Now;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Foreign Keys
        public int? FirmaId { get; set; }
        public int? FirsatId { get; set; }
        public int KullaniciId { get; set; }
        
        // Navigation properties
        public virtual Firma? Firma { get; set; }
        public virtual Firsat? Firsat { get; set; }
        public virtual Kullanici Kullanici { get; set; } = null!;
    }
}
