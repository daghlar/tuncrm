using ClosedXML.Excel;
using TunCRM.Models;

namespace TunCRM.Services
{
    public class ExcelExportService
    {
        private readonly ILogger<ExcelExportService> _logger;

        public ExcelExportService(ILogger<ExcelExportService> logger)
        {
            _logger = logger;
        }

        public byte[] ExportFirmalarToExcel(List<Firma> firmalar)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Firmalar");

            // Başlık satırı
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Firma Adı";
            worksheet.Cell(1, 3).Value = "Adres";
            worksheet.Cell(1, 4).Value = "Telefon";
            worksheet.Cell(1, 5).Value = "Email";
            worksheet.Cell(1, 6).Value = "Web Sitesi";
            worksheet.Cell(1, 7).Value = "Şehir";
            worksheet.Cell(1, 8).Value = "İlçe";
            worksheet.Cell(1, 9).Value = "Posta Kodu";
            worksheet.Cell(1, 10).Value = "Notlar";
            worksheet.Cell(1, 11).Value = "Oluşturma Tarihi";

            // Veri satırları
            for (int i = 0; i < firmalar.Count; i++)
            {
                var firma = firmalar[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = firma.Id;
                worksheet.Cell(row, 2).Value = firma.Ad;
                worksheet.Cell(row, 3).Value = firma.Adres ?? "";
                worksheet.Cell(row, 4).Value = firma.Telefon ?? "";
                worksheet.Cell(row, 5).Value = firma.Email ?? "";
                worksheet.Cell(row, 6).Value = firma.WebSitesi ?? "";
                worksheet.Cell(row, 7).Value = firma.Sehir ?? "";
                worksheet.Cell(row, 8).Value = firma.Ilce ?? "";
                worksheet.Cell(row, 9).Value = firma.PostaKodu ?? "";
                worksheet.Cell(row, 10).Value = firma.Notlar ?? "";
                worksheet.Cell(row, 11).Value = firma.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm");
            }

            // Sütun genişliklerini ayarla
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportFirsatlarToExcel(List<Firsat> firsatlar)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Fırsatlar");

            // Başlık satırı
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGreen;

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Fırsat Adı";
            worksheet.Cell(1, 3).Value = "Firma";
            worksheet.Cell(1, 4).Value = "Açıklama";
            worksheet.Cell(1, 5).Value = "Tutar";
            worksheet.Cell(1, 6).Value = "Aşama";
            worksheet.Cell(1, 7).Value = "Kapanış Tarihi";
            worksheet.Cell(1, 8).Value = "Oluşturma Tarihi";

            // Veri satırları
            for (int i = 0; i < firsatlar.Count; i++)
            {
                var firsat = firsatlar[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = firsat.Id;
                worksheet.Cell(row, 2).Value = firsat.Ad;
                worksheet.Cell(row, 3).Value = firsat.Firma?.Ad ?? "";
                worksheet.Cell(row, 4).Value = firsat.Aciklama ?? "";
                worksheet.Cell(row, 5).Value = firsat.Tutar?.ToString("C") ?? "";
                worksheet.Cell(row, 6).Value = GetAsamaDisplayName(firsat.Asama);
                worksheet.Cell(row, 7).Value = firsat.KapanisTarihi?.ToString("dd.MM.yyyy") ?? "";
                worksheet.Cell(row, 8).Value = firsat.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm");
            }

            // Sütun genişliklerini ayarla
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportAktivitelerToExcel(List<Aktivite> aktiviteler)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Aktiviteler");

            // Başlık satırı
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightYellow;

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Başlık";
            worksheet.Cell(1, 3).Value = "Tip";
            worksheet.Cell(1, 4).Value = "Firma";
            worksheet.Cell(1, 5).Value = "Fırsat";
            worksheet.Cell(1, 6).Value = "Açıklama";
            worksheet.Cell(1, 7).Value = "Tarih";
            worksheet.Cell(1, 8).Value = "Kullanıcı";

            // Veri satırları
            for (int i = 0; i < aktiviteler.Count; i++)
            {
                var aktivite = aktiviteler[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = aktivite.Id;
                worksheet.Cell(row, 2).Value = aktivite.Baslik;
                worksheet.Cell(row, 3).Value = GetAktiviteTipiDisplayName(aktivite.Tip);
                worksheet.Cell(row, 4).Value = aktivite.Firma?.Ad ?? "";
                worksheet.Cell(row, 5).Value = aktivite.Firsat?.Ad ?? "";
                worksheet.Cell(row, 6).Value = aktivite.Aciklama ?? "";
                worksheet.Cell(row, 7).Value = aktivite.Tarih.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cell(row, 8).Value = aktivite.Kullanici?.TamAd ?? "";
            }

            // Sütun genişliklerini ayarla
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private string GetAsamaDisplayName(FirsatAsamasi asama)
        {
            return asama switch
            {
                FirsatAsamasi.IlkIletisim => "İlk İletişim",
                FirsatAsamasi.TeklifHazirlama => "Teklif Hazırlama",
                FirsatAsamasi.TeklifGonderildi => "Teklif Gönderildi",
                FirsatAsamasi.Muzakere => "Müzakere",
                FirsatAsamasi.KapatildiKazanildi => "Kapatıldı - Kazanıldı",
                FirsatAsamasi.KapatildiKaybedildi => "Kapatıldı - Kaybedildi",
                _ => asama.ToString()
            };
        }

        private string GetAktiviteTipiDisplayName(AktiviteTipi tip)
        {
            return tip switch
            {
                AktiviteTipi.Telefon => "Telefon",
                AktiviteTipi.Email => "Email",
                AktiviteTipi.Toplanti => "Toplantı",
                AktiviteTipi.Not => "Not",
                AktiviteTipi.Gorev => "Görev",
                _ => tip.ToString()
            };
        }
    }
}
