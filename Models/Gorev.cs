using System.ComponentModel.DataAnnotations;

namespace TunCRM.Models
{
    public enum GorevDurumu
    {
        [Display(Name = "Beklemede")]
        Beklemede = 1,
        
        [Display(Name = "Devam Ediyor")]
        DevamEdiyor = 2,
        
        [Display(Name = "Tamamlandı")]
        Tamamlandi = 3,
        
        [Display(Name = "İptal Edildi")]
        IptalEdildi = 4
    }

    public enum GorevOnceligi
    {
        [Display(Name = "Düşük")]
        Dusuk = 1,
        
        [Display(Name = "Normal")]
        Normal = 2,
        
        [Display(Name = "Yüksek")]
        Yuksek = 3,
        
        [Display(Name = "Kritik")]
        Kritik = 4
    }

    public class Gorev
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Baslik { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Aciklama { get; set; }
        
        public GorevDurumu Durum { get; set; } = GorevDurumu.Beklemede;
        
        public GorevOnceligi Oncelik { get; set; } = GorevOnceligi.Normal;
        
        public DateTime? BaslangicTarihi { get; set; }
        
        public DateTime? BitisTarihi { get; set; }
        
        public DateTime? TamamlanmaTarihi { get; set; }
        
        public int? AtananKullaniciId { get; set; }
        public virtual Kullanici? AtananKullanici { get; set; }
        
        public int? OlusturanKullaniciId { get; set; }
        public virtual Kullanici? OlusturanKullanici { get; set; }
        
        public int? FirmaId { get; set; }
        public virtual Firma? Firma { get; set; }
        
        public int? FirsatId { get; set; }
        public virtual Firsat? Firsat { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Computed properties
        public bool Gecikmis => BitisTarihi.HasValue && BitisTarihi.Value < DateTime.Now && Durum != GorevDurumu.Tamamlandi;
        
        public bool BugunBiter => BitisTarihi.HasValue && BitisTarihi.Value.Date == DateTime.Today && Durum != GorevDurumu.Tamamlandi;
        
        public bool YakindaBiter => BitisTarihi.HasValue && BitisTarihi.Value.Date <= DateTime.Today.AddDays(3) && Durum != GorevDurumu.Tamamlandi;
    }
}
