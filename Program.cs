using TunCRM.Components;
using TunCRM.Data;
using TunCRM.Services;
using TunCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "TunCRM API", 
        Version = "v1",
        Description = "TunCRM Müşteri İlişkileri Yönetimi API'si"
    });
    
    // JWT Authentication için Swagger konfigürasyonu
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=tuncrm.db"));

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "TunCRM_Super_Secret_Key_2024_At_Least_32_Characters";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "TunCRM",
            ValidAudience = jwtSettings["Audience"] ?? "TunCRM_Users",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// Rate Limiting - .NET 9'da built-in
// builder.Services.AddRateLimiter(options =>
// {
//     options.AddFixedWindowLimiter("ApiPolicy", opt =>
//     {
//         opt.PermitLimit = 100; // 100 istek
//         opt.Window = TimeSpan.FromMinutes(1); // 1 dakika içinde
//         opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
//         opt.QueueLimit = 10; // 10 istek kuyruğa alınabilir
//     });
// });

// Services
builder.Services.AddScoped<CrmService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<PdfReportService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<CacheService>();
builder.Services.AddScoped<ErrorHandlingService>();
builder.Services.AddHttpClient<GoogleMapsScraperService>();
builder.Services.AddScoped<GoogleMapsScraperService>();

// Memory Cache
builder.Services.AddMemoryCache();

// Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Background Services
// builder.Services.AddHostedService<NotificationService>(); // Geçici olarak devre dışı

// Auto-migrate database
builder.Services.AddHostedService<DatabaseMigrationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Rate Limiting - .NET 9'da built-in
// app.UseRateLimiter();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TunCRM API v1");
        c.RoutePrefix = "api-docs"; // /api-docs adresinde Swagger UI
    });
}

// API Controllers
app.MapControllers();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
