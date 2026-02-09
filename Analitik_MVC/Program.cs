using Analitik_MVC.Data;
using Analitik_MVC.Enums;
using Analitik_MVC.Services.Data;
using Analitik_MVC.Services.Database;
using Analitik_MVC.Services.Excel;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("Cookies").AddCookie
    (options =>
    {
        options.LoginPath = "/Usuario/Login";
        options.AccessDeniedPath = "/Usuario/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.SameSite = SameSiteMode.Lax; // o None + Secure si cross-site
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("ConnectionString DefaultConnection es NULL");
}

// 🔑 Construir DataSource UNA sola vez
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<SectorEmpresa>("tipo_sector_empresa");
dataSourceBuilder.MapEnum<TamanoEmpresa>("tipo_tamano_empresa");

var dataSource = dataSourceBuilder.Build();

// 🔑 Pasar el DataSource ya construido
builder.Services.AddDbContext<AnalitikDbContext>(options =>
{
    options.UseNpgsql(dataSource);
});

// ✅ REGISTRAR SERVICIOS DE IMPORTACIÓN ETL
builder.Services.AddScoped<ExcelValidationService>();
builder.Services.AddScoped<ExcelReaderService>();
builder.Services.AddScoped<DataTransformationService>();
builder.Services.AddScoped<DatabaseLoaderService>();
builder.Services.AddScoped<ImportLogService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
