namespace TunCRM.Models
{
    public class FirsatAsamaRaporu
    {
        public int Asama { get; set; }
        public string AsamaAdi { get; set; } = string.Empty;
        public int Sayi { get; set; }
        public decimal ToplamTutar { get; set; }
        public decimal OrtalamaTutar { get; set; }
        public double Yuzde { get; set; }
    }

    public class AktiviteTrendRaporu
    {
        public string Ay { get; set; } = string.Empty;
        public int Sayi { get; set; }
    }

    public class FirmaSehirRaporu
    {
        public string Sehir { get; set; } = string.Empty;
        public int Sayi { get; set; }
    }

    public class GorevDurumRaporu
    {
        public int Durum { get; set; }
        public string DurumAdi { get; set; } = string.Empty;
        public int Sayi { get; set; }
    }

    public class GelirTrendRaporu
    {
        public string Ay { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
    }
}
