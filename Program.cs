using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TransAbon.Data;
using TransAbon.Models;
using TransAbon.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Sesiune: folosită doar pentru indexul caruselului
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.Configure<AppInfoOptions>(builder.Configuration.GetSection(AppInfoOptions.SectionName));
builder.Services.AddScoped<ICarouselService, CarouselService>();

// Baza de date SQLite (fișier local). Calea relativă se rezolvă față de folderul proiectului.
var csb = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("Default") ?? "Data Source=App_Data/transabon.db");
if (!Path.IsPathRooted(csb.DataSource))
{
    csb.DataSource = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
}
Directory.CreateDirectory(Path.GetDirectoryName(csb.DataSource)!);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(csb.ConnectionString));

// Autentificare și roluri (ASP.NET Core Identity)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/autentificare";
    options.AccessDeniedPath = "/acces-interzis";
    options.Cookie.Name = "TransAbon.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // În dezvoltare se acceptă și http://localhost; în producție cookie-ul circulă doar prin HTTPS.
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Creează baza de date (dacă nu există) și conturile inițiale
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    await DbSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/eroare");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
