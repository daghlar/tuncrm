# TunCRM - Professional CRM Application

TunCRM, modern ve kapsamlı bir müşteri ilişkileri yönetimi (CRM) uygulamasıdır. .NET 9.0 ve Blazor Server teknolojileri kullanılarak geliştirilmiştir.

## 🚀 Özellikler

### 📊 Dashboard
- İnteraktif grafikler ve istatistikler
- Gerçek zamanlı veri görselleştirme
- Chart.js entegrasyonu
- KPI kartları ve özet bilgiler

### 🏢 Firma Yönetimi
- Firma bilgileri CRUD işlemleri
- Excel export/import
- Gelişmiş filtreleme ve arama
- Duplicate kontrolü
- Toplu işlemler

### 💼 Fırsat Yönetimi
- Kanban board ile fırsat takibi
- Aşama bazlı yönetim (Potansiyel, Görüşme, Teklif, Kapatıldı)
- Drag & drop desteği
- Fırsat durumu güncellemeleri
- Fırsat değeri takibi

### 📋 Görev Yönetimi
- Görev oluşturma ve atama
- Öncelik seviyeleri (Kritik, Yüksek, Normal, Düşük)
- Tarih takibi ve hatırlatmalar
- Durum yönetimi
- Görev kategorileri

### 📞 Aktivite Takibi
- Telefon, email, toplantı kayıtları
- Firma ve fırsat bağlantıları
- Tarih bazlı filtreleme
- Aktivite türleri ve notlar

### 📧 Email Sistemi
- SMTP entegrasyonu
- HTML email şablonları
- Toplu email gönderimi
- Otomatik hatırlatmalar
- Email geçmişi

### 📊 Raporlama
- PDF rapor oluşturma
- Excel export
- Özelleştirilebilir raporlar
- Grafik ve tablo görünümleri
- Dashboard raporları

### 🔍 Google Maps Scraper
- Potansiyel müşteri toplama
- Otomatik veri çekme
- Toplu import işlemleri
- Veri doğrulama

### 🔐 Güvenlik
- JWT Authentication
- Role-based Authorization
- BCrypt password hashing
- Secure API endpoints
- Session yönetimi

### 🎨 Modern UI/UX
- Bootstrap 5 tasarım
- Dark/Light tema desteği
- Responsive tasarım
- Global arama
- Klavye kısayolları
- Drag & drop arayüzü

## 🛠️ Teknolojiler

- **Backend**: .NET 9.0, Blazor Server
- **Database**: SQLite, Entity Framework Core
- **Frontend**: Bootstrap 5, Chart.js, Font Awesome
- **Authentication**: JWT, BCrypt
- **Email**: MailKit, MimeKit
- **Export**: ClosedXML, iTextSharp
- **API**: RESTful API, Swagger
- **Caching**: Memory Cache
- **Scraping**: HtmlAgilityPack

## 📦 Kurulum

### Gereksinimler
- .NET 9.0 SDK
- Visual Studio 2022 veya VS Code
- Git

### Adımlar
1. Repository'yi klonlayın:
```bash
git clone https://github.com/daghlar/tuncrm.git
cd tuncrm
```

2. NuGet paketlerini restore edin:
```bash
dotnet restore
```

3. Veritabanını oluşturun:
```bash
dotnet ef database update
```

4. Uygulamayı çalıştırın:
```bash
dotnet run
```

5. Tarayıcıda `https://localhost:5001` adresine gidin

## 🔧 Konfigürasyon

### Email Ayarları
`appsettings.json` dosyasında email ayarlarını yapılandırın:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@tuncrm.com",
    "FromName": "TunCRM",
    "EnableSsl": true
  }
}
```

### Veritabanı
SQLite veritabanı otomatik olarak oluşturulur. Migration'lar otomatik çalışır.

## 📚 API Dokümantasyonu

Swagger UI: `https://localhost:5001/api-docs`

### API Endpoints
- `GET /api/firmalar` - Firma listesi
- `POST /api/firmalar` - Yeni firma oluştur
- `PUT /api/firmalar/{id}` - Firma güncelle
- `DELETE /api/firmalar/{id}` - Firma sil
- `GET /api/firsatlar` - Fırsat listesi
- `GET /api/dashboard` - Dashboard verileri

## 🎯 Kullanım

### İlk Giriş
Admin kullanıcısı otomatik oluşturulur:
- **Email**: admin@tuncrm.com
- **Şifre**: Admin123!

### Temel İşlemler
1. **Firma Ekleme**: Firmalar sayfasından yeni firmalar ekleyebilirsiniz
2. **Fırsat Yönetimi**: Kanban board ile fırsatları takip edin
3. **Görev Atama**: Görevler sayfasından görevler oluşturun ve atayın
4. **Raporlama**: Raporlar sayfasından PDF/Excel raporları oluşturun
5. **Email Gönderimi**: Email sayfasından toplu email gönderin

### Tema Değiştirme
Sağ üst köşedeki tema butonu ile dark/light tema arasında geçiş yapabilirsiniz.

### Global Arama
Üst menüdeki arama kutusu ile tüm verilerde arama yapabilirsiniz.

## 🏗️ Proje Yapısı

```
TunCRM/
├── Components/           # Blazor bileşenleri
│   ├── Layout/          # Layout bileşenleri
│   └── Pages/           # Sayfa bileşenleri
├── Controllers/          # API Controller'ları
├── Data/                # Veritabanı context
├── Models/              # Veri modelleri
├── Services/            # İş mantığı servisleri
├── Migrations/          # EF Core migration'ları
├── wwwroot/             # Statik dosyalar
└── Program.cs           # Uygulama başlangıcı
```

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Commit yapın (`git commit -m 'Add some AmazingFeature'`)
4. Push yapın (`git push origin feature/AmazingFeature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 👨‍💻 Geliştirici

TunCRM Development Team

## 📞 İletişim

- **GitHub**: [@daghlar](https://github.com/daghlar)
- **Email**: info@tuncrm.com
- **Website**: https://github.com/daghlar/tuncrm

## 🎉 Teşekkürler

Bu projeyi beğendiyseniz yıldız vermeyi unutmayın! ⭐

---

**Not**: Bu proje eğitim ve geliştirme amaçlıdır. Üretim ortamında kullanmadan önce güvenlik ayarlarını gözden geçirin.