using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Services;
using PetCarePlatform.Infrastructure;
using PetCarePlatform.Infrastructure.Data;
using PetCarePlatform.Infrastructure.Identity;
using PetCarePlatform.Infrastructure.Location;
using PetCarePlatform.Infrastructure.Payment;
using PetCarePlatform.Web;
using PetCarePlatform.Web.Middleware;
using PetCarePlatform.Web.HealthChecks;
using PetCarePlatform.Web.Mappings;
using PetCarePlatform.Infrastructure.Configuration;
using Serilog;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
// HealthChecks UI disabled to avoid KubernetesClient dependency
// using HealthChecks.UI.Client;
// using HealthChecks.UI;
using Microsoft.Extensions.DependencyInjection;

// Ensure logs directory exists
try
{
    var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    if (!Directory.Exists(logsDirectory))
    {
        Directory.CreateDirectory(logsDirectory);
    }
}
catch (Exception ex)
{
    // If we can't create logs directory, continue without file logging
    Console.WriteLine($"Warning: Could not create logs directory: {ex.Message}");
}

// Configure Serilog with error handling
try
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build())
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "PetCarePlatform")
        .CreateLogger();
}
catch (Exception ex)
{
    // Fallback to console-only logging if file logging fails
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger();
    Log.Warning(ex, "Failed to initialize file logging. Using console-only logging.");
}

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<PetCarePlatform.Core.Validators.CreateBookingRequestValidator>());

// Register global exception handler
builder.Services.AddExceptionHandler<PetCarePlatform.Web.Middleware.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        b => b.MigrationsAssembly("PetCarePlatform.Infrastructure")
              .EnableRetryOnFailure(
                  maxRetryCount: 5,
                  maxRetryDelay: TimeSpan.FromSeconds(30),
                  errorNumbersToAdd: null)));

// Add Identity Services
builder.Services.AddIdentityServices(builder.Configuration);

// Add Infrastructure Services (repositories)
builder.Services.AddInfrastructureServices();

// Add strongly-typed configuration with validation
builder.Services.AddValidatedConfiguration<EmailConfiguration>(
    builder.Configuration, EmailConfiguration.SectionName);
// Google Maps configuration removed - using OpenStreetMap (free, no API key needed)

// Register Core Services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// Stripe Payment Service - Commented out for manual payment system
// builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IPetOwnerService, PetOwnerService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IServiceProviderService, ServiceProviderService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IUserService, UserService>();

// Register Infrastructure Services
// Configure HttpClient for OpenStreetMapService (Nominatim API)
// Set User-Agent header as required by Nominatim usage policy
// Nominatim requires a descriptive User-Agent that identifies your application
builder.Services.AddHttpClient<OpenStreetMapService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    // Set User-Agent header - Nominatim requires this to identify your application
    // Format: ApplicationName/Version (contact information)
    client.DefaultRequestHeaders.Add("User-Agent", "PetCarePlatform/1.0 (contact@petcareplatform.com)");
    // Add Accept-Language for better geocoding results
    client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
})
.AddHttpMessageHandler(() => new NominatimUserAgentHandler("PetCarePlatform/1.0 (contact@petcareplatform.com)"));
builder.Services.AddScoped<ILocationService, OpenStreetMapService>();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfiles>());


// Configure HttpClient for external services
builder.Services.AddHttpClient();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
        name: "sqlserver",
        tags: new[] { "db", "sql", "sqlserver" });

// Health Checks UI disabled to avoid KubernetesClient dependency
// builder.Services.AddHealthChecksUI(setup =>
// {
//     setup.SetEvaluationTimeInSeconds(10);
//     setup.MaximumHistoryEntriesPerEndpoint(50);
//     setup.AddHealthCheckEndpoint("PetCare Platform API", "/health");
// })
// .AddInMemoryStorage();

var app = builder.Build();

// Validate configuration on startup
try
{
    app.Services.ValidateConfiguration();
    Log.Information("Configuration validated successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Configuration validation failed");
    throw;
}

// Seed database only in Development environment
if (app.Environment.IsDevelopment())
{
    try
    {
        //SeedData.InitializeAsync(app.Services).GetAwaiter().GetResult();
        Log.Information("Database seeded successfully (Development)");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database seeding failed or was skipped");
    }
}

// Configure the HTTP request pipeline.
// Use global exception handler (must be first)
app.UseExceptionHandler();

// Add security headers
//app.UseMiddleware<SecurityHeadersMiddleware>();

// Add rate limiting (simple implementation)
//app.UseMiddleware<RateLimitingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Health Check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    // Simple JSON response instead of UI response
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                exception = e.Value.Exception?.Message
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        });
        await context.Response.WriteAsync(result);
    }
});

// Health Checks UI disabled
// app.MapHealthChecksUI(options =>
// {
//     options.UIPath = "/health-ui";
//     options.ApiPath = "/health-api";
// });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    Log.Information("Starting PetCare Platform application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }

