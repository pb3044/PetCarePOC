using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Services;
using PetCarePlatform.Infrastructure;
using PetCarePlatform.Infrastructure.Data;
using PetCarePlatform.Infrastructure.Identity;
using PetCarePlatform.Infrastructure.Location;
using PetCarePlatform.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        b => b.MigrationsAssembly("PetCarePlatform.Infrastructure")));

// Add Identity Services
builder.Services.AddIdentityServices(builder.Configuration);

// Add Infrastructure Services (repositories)
builder.Services.AddInfrastructureServices();

// Register Core Services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPetOwnerService, PetOwnerService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IServiceProviderService, ServiceProviderService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IUserService, UserService>();

// Register Infrastructure Services
builder.Services.AddScoped<ILocationService, GoogleMapsService>();

builder.Services.AddAutoMapper(typeof(MapperConfig));
// Configure HttpClient for external services
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

