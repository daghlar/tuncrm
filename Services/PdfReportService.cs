using iTextSharp.text;
using iTextSharp.text.pdf;
using TunCRM.Models;
using TunCRM.Data;
using Microsoft.EntityFrameworkCore;

namespace TunCRM.Services
{
    public class PdfReportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PdfReportService> _logger;

        public PdfReportService(AppDbContext context, ILogger<PdfReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<byte[]> GenerateFirsatRaporuAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 25, 25);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Başlık
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.DARK_GRAY);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            var title = new Paragraph("Fırsat Raporu", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);

            // Tarih aralığı
            var tarihAraligi = baslangicTarihi.HasValue && bitisTarihi.HasValue
                ? $"{baslangicTarihi.Value:dd.MM.yyyy} - {bitisTarihi.Value:dd.MM.yyyy}"
                : "Tüm Zamanlar";

            var tarihParagraph = new Paragraph($"Tarih Aralığı: {tarihAraligi}", normalFont)
            {
                SpacingAfter = 20
            };
            document.Add(tarihParagraph);

            // Fırsat verilerini al
            var firsatlar = await GetFirsatlarAsync(baslangicTarihi, bitisTarihi);

            // İstatistikler
            var toplamFirsat = firsatlar.Count;
            var kazanilanFirsat = firsatlar.Count(f => f.Asama == FirsatAsamasi.KapatildiKazanildi);
            var kaybedilenFirsat = firsatlar.Count(f => f.Asama == FirsatAsamasi.KapatildiKaybedildi);
            var toplamTutar = firsatlar.Where(f => f.Tutar.HasValue).Sum(f => f.Tutar!.Value);

            // İstatistik tablosu
            var statsTable = new PdfPTable(2)
            {
                WidthPercentage = 50,
                SpacingAfter = 20
            };

            statsTable.AddCell(CreateCell("Toplam Fırsat", headerFont, true));
            statsTable.AddCell(CreateCell(toplamFirsat.ToString(), normalFont, false));
            statsTable.AddCell(CreateCell("Kazanılan Fırsat", headerFont, true));
            statsTable.AddCell(CreateCell(kazanilanFirsat.ToString(), normalFont, false));
            statsTable.AddCell(CreateCell("Kaybedilen Fırsat", headerFont, true));
            statsTable.AddCell(CreateCell(kaybedilenFirsat.ToString(), normalFont, false));
            statsTable.AddCell(CreateCell("Toplam Tutar", headerFont, true));
            statsTable.AddCell(CreateCell(toplamTutar.ToString("C0"), normalFont, false));

            document.Add(statsTable);

            // Fırsat detayları tablosu
            var firsatTable = new PdfPTable(6)
            {
                WidthPercentage = 100,
                SpacingAfter = 20
            };

            // Tablo başlıkları
            firsatTable.AddCell(CreateCell("Fırsat Adı", headerFont, true));
            firsatTable.AddCell(CreateCell("Firma", headerFont, true));
            firsatTable.AddCell(CreateCell("Aşama", headerFont, true));
            firsatTable.AddCell(CreateCell("Tutar", headerFont, true));
            firsatTable.AddCell(CreateCell("Oluşturma", headerFont, true));
            firsatTable.AddCell(CreateCell("Kapanış", headerFont, true));

            // Fırsat verileri
            foreach (var firsat in firsatlar)
            {
                firsatTable.AddCell(CreateCell(firsat.Ad, normalFont, false));
                firsatTable.AddCell(CreateCell(firsat.Firma?.Ad ?? "-", normalFont, false));
                firsatTable.AddCell(CreateCell(GetFirsatAsamaAdi((int)firsat.Asama), normalFont, false));
                firsatTable.AddCell(CreateCell(firsat.Tutar?.ToString("C0") ?? "-", normalFont, false));
                firsatTable.AddCell(CreateCell(firsat.OlusturmaTarihi.ToString("dd.MM.yyyy"), normalFont, false));
                firsatTable.AddCell(CreateCell(firsat.KapanisTarihi?.ToString("dd.MM.yyyy") ?? "-", normalFont, false));
            }

            document.Add(firsatTable);

            // Aşama dağılımı
            var asamaDagilimi = firsatlar
                .GroupBy(f => f.Asama)
                .Select(g => new { Asama = g.Key, Sayi = g.Count() })
                .OrderBy(x => x.Asama)
                .ToList();

            var asamaTable = new PdfPTable(2)
            {
                WidthPercentage = 50,
                SpacingAfter = 20
            };

            asamaTable.AddCell(CreateCell("Aşama", headerFont, true));
            asamaTable.AddCell(CreateCell("Sayı", headerFont, true));

            foreach (var asama in asamaDagilimi)
            {
                asamaTable.AddCell(CreateCell(GetFirsatAsamaAdi((int)asama.Asama), normalFont, false));
                asamaTable.AddCell(CreateCell(asama.Sayi.ToString(), normalFont, false));
            }

            document.Add(asamaTable);

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GenerateGorevRaporuAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 25, 25);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.DARK_GRAY);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            var title = new Paragraph("Görev Raporu", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);

            // Görev verilerini al
            var gorevler = await GetGorevlerAsync(baslangicTarihi, bitisTarihi);

            // İstatistikler
            var toplamGorev = gorevler.Count;
            var tamamlananGorev = gorevler.Count(g => g.Durum == GorevDurumu.Tamamlandi);
            var gecikmisGorev = gorevler.Count(g => g.Gecikmis);
            var bugunBitecekGorev = gorevler.Count(g => g.BugunBiter);

            // İstatistik tablosu
            var statsTable = new PdfPTable(2)
            {
                WidthPercentage = 50,
                SpacingAfter = 20
            };

            statsTable.AddCell(CreateCell("Toplam Görev", headerFont, true));
            statsTable.AddCell(CreateCell(toplamGorev.ToString(), normalFont, false));
            statsTable.AddCell(CreateCell("Tamamlanan", headerFont, true));
            statsTable.AddCell(CreateCell(tamamlananGorev.ToString(), normalFont, false));
            statsTable.AddCell(CreateCell("Gecikmiş", headerFont, true));
            statsTable.AddCell(CreateCell(gecikmisGorev.ToString(), normalFont, false));
            statsTable.AddCell(CreateCell("Bugün Bitecek", headerFont, true));
            statsTable.AddCell(CreateCell(bugunBitecekGorev.ToString(), normalFont, false));

            document.Add(statsTable);

            // Görev detayları tablosu
            var gorevTable = new PdfPTable(7)
            {
                WidthPercentage = 100,
                SpacingAfter = 20
            };

            gorevTable.AddCell(CreateCell("Görev", headerFont, true));
            gorevTable.AddCell(CreateCell("Durum", headerFont, true));
            gorevTable.AddCell(CreateCell("Öncelik", headerFont, true));
            gorevTable.AddCell(CreateCell("Atanan", headerFont, true));
            gorevTable.AddCell(CreateCell("Firma", headerFont, true));
            gorevTable.AddCell(CreateCell("Başlangıç", headerFont, true));
            gorevTable.AddCell(CreateCell("Bitiş", headerFont, true));

            foreach (var gorev in gorevler)
            {
                gorevTable.AddCell(CreateCell(gorev.Baslik, normalFont, false));
                gorevTable.AddCell(CreateCell(GetGorevDurumAdi((int)gorev.Durum), normalFont, false));
                gorevTable.AddCell(CreateCell(GetGorevOncelikAdi((int)gorev.Oncelik), normalFont, false));
                gorevTable.AddCell(CreateCell(gorev.AtananKullanici?.TamAd ?? "-", normalFont, false));
                gorevTable.AddCell(CreateCell(gorev.Firma?.Ad ?? "-", normalFont, false));
                gorevTable.AddCell(CreateCell(gorev.BaslangicTarihi?.ToString("dd.MM.yyyy") ?? "-", normalFont, false));
                gorevTable.AddCell(CreateCell(gorev.BitisTarihi?.ToString("dd.MM.yyyy") ?? "-", normalFont, false));
            }

            document.Add(gorevTable);

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GenerateFirmaRaporuAsync()
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 25, 25);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.DARK_GRAY);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            var title = new Paragraph("Firma Raporu", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);

            // Firma verilerini al
            var firmalar = await _context.Firmalar
                .Include(f => f.Firsatlar)
                .Include(f => f.Aktiviteler)
                .ToListAsync();

            // İstatistikler
            var toplamFirma = firmalar.Count;
            var aktifFirma = firmalar.Count(f => f.Firsatlar.Any());
            var toplamFirsat = firmalar.Sum(f => f.Firsatlar.Count);
            var toplamAktivite = firmalar.Sum(f => f.Aktiviteler.Count);

            // İstatistik tablosu
            var statsTable = new PdfPTable(2)
            {
                WidthPercentage = 50,
                SpacingAfter = 20
            };

            statsTable.AddCell(CreateCell("Toplam Firma", headerFont, true));
            statsTable.AddCell(CreateCell(toplamFirma.ToString(), normalFont, false));
            statsTable.AddCell(CreateCell("Aktif Firma", headerFont, true));
            statsTable.AddCell(CreateCell(aktifFirma.ToString(), normalFont, false));
            statsTable.AddCell(CreateCell("Toplam Fırsat", headerFont, true));
            statsTable.AddCell(CreateCell(toplamFirsat.ToString(), normalFont, false));
            statsTable.AddCell(CreateCell("Toplam Aktivite", headerFont, true));
            statsTable.AddCell(CreateCell(toplamAktivite.ToString(), normalFont, false));

            document.Add(statsTable);

            // Firma detayları tablosu
            var firmaTable = new PdfPTable(6)
            {
                WidthPercentage = 100,
                SpacingAfter = 20
            };

            firmaTable.AddCell(CreateCell("Firma Adı", headerFont, true));
            firmaTable.AddCell(CreateCell("Şehir", headerFont, true));
            firmaTable.AddCell(CreateCell("Email", headerFont, true));
            firmaTable.AddCell(CreateCell("Telefon", headerFont, true));
            firmaTable.AddCell(CreateCell("Fırsat Sayısı", headerFont, true));
            firmaTable.AddCell(CreateCell("Aktivite Sayısı", headerFont, true));

            foreach (var firma in firmalar)
            {
                firmaTable.AddCell(CreateCell(firma.Ad, normalFont, false));
                firmaTable.AddCell(CreateCell(firma.Sehir ?? "-", normalFont, false));
                firmaTable.AddCell(CreateCell(firma.Email ?? "-", normalFont, false));
                firmaTable.AddCell(CreateCell(firma.Telefon ?? "-", normalFont, false));
                firmaTable.AddCell(CreateCell(firma.Firsatlar.Count.ToString(), normalFont, false));
                firmaTable.AddCell(CreateCell(firma.Aktiviteler.Count.ToString(), normalFont, false));
            }

            document.Add(firmaTable);

            document.Close();
            return memoryStream.ToArray();
        }

        private async Task<List<Firsat>> GetFirsatlarAsync(DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            var query = _context.Firsatlar
                .Include(f => f.Firma)
                .Include(f => f.Kullanici)
                .AsQueryable();

            if (baslangicTarihi.HasValue)
                query = query.Where(f => f.OlusturmaTarihi >= baslangicTarihi.Value);

            if (bitisTarihi.HasValue)
                query = query.Where(f => f.OlusturmaTarihi <= bitisTarihi.Value);

            return await query.OrderByDescending(f => f.OlusturmaTarihi).ToListAsync();
        }

        private async Task<List<Gorev>> GetGorevlerAsync(DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            var query = _context.Gorevler
                .Include(g => g.AtananKullanici)
                .Include(g => g.Firma)
                .Include(g => g.Firsat)
                .Where(g => g.Aktif)
                .AsQueryable();

            if (baslangicTarihi.HasValue)
                query = query.Where(g => g.OlusturmaTarihi >= baslangicTarihi.Value);

            if (bitisTarihi.HasValue)
                query = query.Where(g => g.OlusturmaTarihi <= bitisTarihi.Value);

            return await query.OrderByDescending(g => g.OlusturmaTarihi).ToListAsync();
        }

        private PdfPCell CreateCell(string text, Font font, bool isHeader)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 8,
                BackgroundColor = isHeader ? BaseColor.LIGHT_GRAY : BaseColor.WHITE,
                Border = Rectangle.BOX,
                BorderWidth = 1
            };
            return cell;
        }

        private string GetFirsatAsamaAdi(int asama)
        {
            return asama switch
            {
                1 => "İlk İletişim",
                2 => "Teklif Hazırlama",
                3 => "Teklif Gönderildi",
                4 => "Müzakere",
                5 => "Kazanıldı",
                6 => "Kaybedildi",
                _ => "Bilinmiyor"
            };
        }

        private string GetGorevDurumAdi(int durum)
        {
            return durum switch
            {
                1 => "Beklemede",
                2 => "Devam Ediyor",
                3 => "Tamamlandı",
                4 => "İptal Edildi",
                _ => "Bilinmiyor"
            };
        }

        private string GetGorevOncelikAdi(int oncelik)
        {
            return oncelik switch
            {
                1 => "Düşük",
                2 => "Normal",
                3 => "Yüksek",
                4 => "Kritik",
                _ => "Bilinmiyor"
            };
        }
    }
}
