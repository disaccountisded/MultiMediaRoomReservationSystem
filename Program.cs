using Microsoft.EntityFrameworkCore;
using MultiMediaRoomReservationSystem.Data;
using MultiMediaRoomReservationSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Local development reads appsettings.json.
// Railway first uses ConnectionStrings__DefaultConnection if supplied.
// If that variable is absent, build the connection string from Railway's MYSQL* variables.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!builder.Environment.IsDevelopment())
{
    var host = builder.Configuration["MYSQLHOST"];
    var port = builder.Configuration["MYSQLPORT"];
    var database = builder.Configuration["MYSQLDATABASE"];
    var user = builder.Configuration["MYSQLUSER"];
    var password = builder.Configuration["MYSQLPASSWORD"];

    var mysqlUrl = builder.Configuration["MYSQL_URL"];
    if (string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(mysqlUrl) &&
        Uri.TryCreate(mysqlUrl, UriKind.Absolute, out var uri))
    {
        var info = uri.UserInfo.Split(':', 2);
        connectionString =
            $"Server={uri.Host};Port={(uri.Port > 0 ? uri.Port : 3306)};" +
            $"Database={uri.AbsolutePath.TrimStart('/')};" +
            $"User={Uri.UnescapeDataString(info[0])};" +
            $"Password={(info.Length > 1 ? Uri.UnescapeDataString(info[1]) : "")};";
    }

    if (!string.IsNullOrWhiteSpace(host) &&
        !string.IsNullOrWhiteSpace(database) &&
        !string.IsNullOrWhiteSpace(user) &&
        !string.IsNullOrWhiteSpace(password))
    {
        connectionString =
            $"Server={host};Port={port ?? "3306"};Database={database};" +
            $"User={user};Password={password};";
    }
}

if (string.IsNullOrWhiteSpace(connectionString) ||
    connectionString.Contains("YOUR_LOCAL_MYSQL", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "MySQL connection string is not configured. For local development, edit " +
        "ConnectionStrings:DefaultConnection in appsettings.json. On Railway, set " +
        "ConnectionStrings__DefaultConnection or link MYSQLHOST, MYSQLPORT, " +
        "MYSQLDATABASE, MYSQLUSER and MYSQLPASSWORD.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure()
    ));

builder.Services.AddScoped<IReservationService, ReservationService>();

// Railway assigns PORT dynamically. Bind Kestrel to all interfaces.
var railwayPort = builder.Configuration["PORT"];
if (!string.IsNullOrWhiteSpace(railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
}

// Railway (and most hosts) terminate HTTPS at a proxy; honour X-Forwarded-* headers.
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Apply EF Core migrations on startup so a fresh Railway/hosted MySQL database
// gets its tables automatically. Disable with AUTO_MIGRATE=false.
if (!string.Equals(builder.Configuration["AUTO_MIGRATE"], "false", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
        logger.LogInformation("Database migrations applied.");
    }
    catch (Exception ex)
    {
        // Keep the site up so /health and logs stay reachable.
        logger.LogError(ex, "Database migration failed.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reservations}/{action=Index}/{id?}");

// Simple diagnostics: /health confirms the web process; /health/database checks MySQL.
app.MapGet("/Error", () => Results.Problem("An unexpected server error occurred."));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/health/database", async (ApplicationDbContext db) =>
{
    try
    {
        var connected = await db.Database.CanConnectAsync();
        return connected
            ? Results.Ok(new { status = "connected", database = "MySQL" })
            : Results.Problem("The application could not connect to the configured database.");
    }
    catch
    {
        return Results.Problem("The application could not connect to the configured database.");
    }
});

app.Run();
