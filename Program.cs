using ProjectTemplate.Data;
using ProjectTemplate.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ---- Load .env file ---
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}
// -----------------------

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to config
builder.Configuration.AddEnvironmentVariables();

// â”€â”€ Services â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization();

builder.Services.Configure<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "sq" };
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});
builder.Services.AddOutputCache();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var rawConnString = builder.Configuration["SUPABASE_CONNECTION_STRING"] 
    ?? builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=app.db";

var connectionString = rawConnString;

if (rawConnString.StartsWith("postgres://") || rawConnString.StartsWith("postgresql://"))
{
    var uri = new Uri(rawConnString);
    var userInfo = uri.UserInfo.Split(':');
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    connectionString = $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={uri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={password};Ssl Mode=Require;Trust Server Certificate=true;Pooling=true;";
}

builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    if (connectionString.Contains("app.db"))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://*:{port}");
}

// â”€â”€ Middleware â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

// â”€â”€ Seed database â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.ProviderName != null && db.Database.ProviderName.Contains("Npgsql"))
     {
         try
         {
             var creator = db.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
             try
             {
                 if (!creator.Exists())
                 {
                     creator.Create();
                 }
             }
             catch { /* Ignore database creation failures */ }

             try {
                 // Apply any pending migrations to keep Supabase integration tables up to date
                 db.Database.Migrate();
             } 
             catch { /* Ignore migration errors on start if they fail */ }

             var connection = db.Database.GetDbConnection();
             bool hasTables = false;
             try
             {
                 connection.Open();
                 using var command = connection.CreateCommand();
                 command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Chapters';";
                 var count = Convert.ToInt64(command.ExecuteScalar());
                 hasTables = count > 0;
             }
             finally
             {
                 connection.Close();
             }

             if (!hasTables)
             {
                 // In case migration was skipped or failed and database is empty
                 try {
                     creator.CreateTables();
                 } catch { }
             }
             else
             {
                 // Add missing columns if database exists but needs sync
                 try {
                     var conn = db.Database.GetDbConnection();
                     if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                     using var cmd = conn.CreateCommand();
                     cmd.CommandText = @"
                         ALTER TABLE ""Chapters"" ADD COLUMN IF NOT EXISTS ""Language"" text DEFAULT 'en';
                         ALTER TABLE ""Chapters"" ADD COLUMN IF NOT EXISTS ""GameType"" text DEFAULT 'Coding';
                         ALTER TABLE ""GameSessions"" ADD COLUMN IF NOT EXISTS ""PlayerName"" text DEFAULT '';
                         ALTER TABLE ""GameSessions"" ADD COLUMN IF NOT EXISTS ""Language"" text DEFAULT 'en';
                         ALTER TABLE ""GameSessions"" ADD COLUMN IF NOT EXISTS ""GameType"" text DEFAULT 'Coding';
                     ";
                     cmd.ExecuteNonQuery();
                     conn.Close();
                 } catch { }
             }
         }
         catch (Exception ex)
         {
             Console.WriteLine($"Error initializing db: {ex.Message}");
         }
     }
    else
    {
        db.Database.EnsureCreated();
    }

    try
    {
        DbSeeder.Seed(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding db: {ex.Message}");
    }
}

// â”€â”€ Routes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

