using ProjectTemplate.Data;
using ProjectTemplate.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ── Load .env ──────────────────────────────────────────────────────────────────
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ── Localisation ───────────────────────────────────────────────────────────────
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews().AddViewLocalization();
builder.Services.Configure<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(options =>
{
    var supported = new[] { "sq", "en" };
    options.SetDefaultCulture("sq");
    options.AddSupportedCultures(supported);
    options.AddSupportedUICultures(supported);
});

// ── Caching / Compression / Session ───────────────────────────────────────────
builder.Services.AddOutputCache();
builder.Services.AddResponseCompression(o => o.EnableForHttps = true);
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".EnisiCenter.Session";
});

// ── Database ───────────────────────────────────────────────────────────────────
var rawConn = builder.Configuration["SUPABASE_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=app.db";

var connectionString = rawConn;
if (rawConn.StartsWith("postgres://") || rawConn.StartsWith("postgresql://"))
{
    var uri = new Uri(rawConn);
    var userInfo = uri.UserInfo.Split(':');
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    connectionString = $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={uri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={password};Ssl Mode=Require;Trust Server Certificate=true;Pooling=true;";
}

builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    if (connectionString.Contains("app.db"))
        options.UseSqlite(connectionString);
    else
        options.UseNpgsql(connectionString);
});

// ── Application services ───────────────────────────────────────────────────────
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Ecommerce services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// ── Port binding for Render ────────────────────────────────────────────────────
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    app.Urls.Add($"http://*:{port}");

// ── Middleware ─────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization();
app.UseOutputCache();
app.UseSession();
app.UseAuthorization();

// ── Database initialisation ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (db.Database.ProviderName != null && db.Database.ProviderName.Contains("Npgsql"))
    {
        try
        {
            var creator = db.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
            try { if (!creator.Exists()) creator.Create(); } catch { }
            try { db.Database.Migrate(); } catch { }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB Init] {ex.Message}");
        }
    }
    else
    {
        db.Database.EnsureCreated();
    }

    // Seed quiz/game data (existing seeder — unchanged)
    try { DbSeeder.Seed(db); }
    catch (Exception ex) { Console.WriteLine($"[Seeder] {ex.Message}"); }

    // Seed demo products for Enisi Center if catalog is empty
    try { EcommerceSeeder.Seed(db); }
    catch (Exception ex) { Console.WriteLine($"[EcommerceSeeder] {ex.Message}"); }
}

// ── Routes ─────────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "shop_product",
    pattern: "shop/product/{id:int}/{slug?}",
    defaults: new { controller = "Shop", action = "Detail" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
