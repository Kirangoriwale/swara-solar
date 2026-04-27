using Microsoft.EntityFrameworkCore;
using SolarBilling.Data;
using SolarBilling.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("PostgreSQL connection string is missing. Set ConnectionStrings:DefaultConnection.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<PdfService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var runMigrations = app.Environment.IsDevelopment()
    || string.Equals(
        builder.Configuration["RunMigrationsOnStartup"],
        "true",
        StringComparison.OrdinalIgnoreCase)
    || string.Equals(
        Environment.GetEnvironmentVariable("RUN_MIGRATIONS_ON_STARTUP"),
        "true",
        StringComparison.OrdinalIgnoreCase);

if (runMigrations)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations executed on startup.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration on startup failed.");
        throw;
    }
}

app.Run();
