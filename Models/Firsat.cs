using System.ComponentModel.DataAnnotations;

namespace TunCRM.Models
{
    public enum FirsatAsamasi
    {
        [Display(Name = "İlk İletişim")]
        IlkIletisim = 1,
        
        [Display(Name = "Teklif Hazırlama")]
        TeklifHazirlama = 2,
        
        [Display(Name = "Teklif Gönderildi")]
        TeklifGonderildi = 3,
        
        [Display(Name = "Müzakere")]
        Muzakere = 4,
        
        [Display(Name = "Kapatıldı - Kazanıldı")]
        KapatildiKazanildi = 5,
        
        [Display(Name = "Kapatıldı - Kaybedildi")]
        KapatildiKaybedildi = 6
    }

    public class Firsat
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Fırsat adı gereklidir")]
        [StringLength(200, ErrorMessage = "Fırsat adı en fazla 200 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Aciklama { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Tutar pozitif bir değer olmalıdır")]
        public decimal? Tutar { get; set; }
        
        public FirsatAsamasi Asama { get; set; } = FirsatAsamasi.IlkIletisim;
        
        public DateTime? KapanisTarihi { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Foreign Keys
        public int FirmaId { get; set; }
        public int KullaniciId { get; set; }
        
        // Navigation properties
        public virtual Firma Firma { get; set; } = null!;
        public virtual Kullanici Kullanici { get; set; } = null!;
        public virtual ICollection<Aktivite> Aktiviteler { get; set; } = new List<Aktivite>();
    }
}
