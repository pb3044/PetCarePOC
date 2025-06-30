using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Web.Models;
using PetCarePlatform.Core.Interfaces;
using System.Linq;
using System.Security.Claims;

namespace PetCarePlatform.Web.Controllers
{
    public class ServiceProviderController : Controller
    {
        private readonly IServiceProviderService _serviceProviderService;
        private readonly IBookingService _bookingService;
        private readonly IServiceService _serviceService;
        private readonly IPaymentService _paymentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceProviderController(
            IServiceProviderService serviceProviderService,
            IBookingService bookingService,
            IServiceService serviceService,
            IPaymentService paymentService,
            UserManager<ApplicationUser> userManager)
        {
            _serviceProviderService = serviceProviderService;
            _bookingService = bookingService;
            _serviceService = serviceService;
            _paymentService = paymentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var userIdInt = int.Parse(userId);

                // Get service provider profile
                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProvider == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }

                // Get current date and time
                var now = DateTime.UtcNow;
                var today = now.Date;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                // Fetch dashboard data
                var dashboard = new ServiceProviderDashboardViewModel
                {
                    ProviderInfo = new ServiceProviderInfo
                    {
                        Id = serviceProvider.Id,
                        BusinessName = serviceProvider.BusinessName,
                        UserName = serviceProvider.User?.UserName ?? "",
                        Email = serviceProvider.User?.Email ?? "",
                        AverageRating = serviceProvider.AverageRating,
                        TotalReviews = serviceProvider.TotalReviews
                    }
                };

                // Get all bookings for this provider
                var allBookings = await _bookingService.GetBookingsByProviderIdAsync(serviceProvider.Id);
                
                // Calculate total bookings
                dashboard.TotalBookings = allBookings.Count();

                // Calculate pending requests (bookings with Pending status)
                var pendingBookings = allBookings.Where(b => b.Status == BookingStatus.Pending).ToList();
                dashboard.PendingRequests = pendingBookings.Count;

                // Get active services
                var activeServices = await _serviceService.GetServicesByProviderIdAsync(serviceProvider.Id);
                dashboard.ActiveServices = activeServices.Count(s => s.IsActive);

                // Calculate monthly earnings
                var monthlyBookings = allBookings.Where(b => 
                    b.Status == BookingStatus.Completed && 
                    b.CreatedAt >= startOfMonth).ToList();
                dashboard.MonthlyEarnings = monthlyBookings.Sum(b => b.TotalPrice);

                // Get recent requests (last 30 days, excluding pending)
                var recentBookings = allBookings
                    .Where(b => b.Status != BookingStatus.Pending && b.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(20)
                    .ToList();

                foreach (var booking in recentBookings)
                {
                    var duration = booking.EndTime - booking.StartTime;
                    var durationText = duration.TotalHours >= 1 
                        ? $"{duration.TotalHours:F1} hours" 
                        : $"{duration.TotalMinutes} minutes";

                    dashboard.RecentRequests.Add(new RecentBookingRequestViewModel
                    {
                        Id = booking.Id,
                        PetOwnerName = $"{booking.Owner?.User?.FirstName} {booking.Owner?.User?.LastName}",
                        ServiceName = booking.Service?.Title ?? "",
                        RequestDate = booking.CreatedAt,
                        Status = booking.Status.ToString(),
                        IsAvailable = false, // Not relevant for completed/declined bookings
                        AvailabilityMessage = ""
                    });
                }

                // Get today's schedule
                var todayBookings = allBookings
                    .Where(b => b.StartTime.Date == today && 
                               (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress))
                    .OrderBy(b => b.StartTime)
                    .ToList();

                foreach (var booking in todayBookings)
                {
                    dashboard.TodaySchedule.Add(new TodayScheduleViewModel
                    {
                        Id = booking.Id,
                        ServiceName = booking.Service?.Title ?? "",
                        PetOwnerName = $"{booking.Owner?.User?.FirstName} {booking.Owner?.User?.LastName}",
                        PetName = booking.Pet?.Name ?? "",
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime,
                        Time = $"{booking.StartTime:HH:mm} - {booking.EndTime:HH:mm}",
                        Status = booking.Status.ToString(),
                        TotalPrice = booking.TotalPrice,
                        SpecialInstructions = booking.SpecialInstructions ?? ""
                    });
                }

                // Check if provider is available today
                dashboard.ProviderInfo.IsAvailableToday = await CheckProviderAvailabilityToday(serviceProvider.Id);
                dashboard.ProviderInfo.CurrentAvailabilityStatus = GetCurrentAvailabilityStatus(dashboard.ProviderInfo.IsAvailableToday);

                return View(dashboard);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Dashboard: {ex.Message}");
                
                // Return a view with error information
                ViewBag.ErrorMessage = "An error occurred while loading the dashboard. Please try again later.";
                return View(new ServiceProviderDashboardViewModel());
            }
        }

        private async Task<bool> CheckAvailabilityForBooking(int providerId, Booking booking)
        {
            try
            {
                // Get provider's availability schedule
                var availabilitySchedules = await _serviceProviderService.GetAvailabilityScheduleAsync(providerId);
                
                // Check if the booking time falls within provider's availability
                var dayOfWeek = (int)booking.StartTime.DayOfWeek;
                var startTime = booking.StartTime.TimeOfDay;
                var endTime = booking.EndTime.TimeOfDay;

                var daySchedule = availabilitySchedules.FirstOrDefault(s => (int)s.DayOfWeek == dayOfWeek);
                
                if (daySchedule == null || !daySchedule.IsAvailable)
                {
                    return false;
                }

                // Check if booking time overlaps with availability
                return startTime >= daySchedule.StartTime && endTime <= daySchedule.EndTime;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CheckProviderAvailabilityToday(int providerId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var dayOfWeek = (int)today.DayOfWeek;
                var currentTime = DateTime.UtcNow.TimeOfDay;

                var availabilitySchedules = await _serviceProviderService.GetAvailabilityScheduleAsync(providerId);
                var todaySchedule = availabilitySchedules.FirstOrDefault(s => (int)s.DayOfWeek == dayOfWeek);

                if (todaySchedule == null || !todaySchedule.IsAvailable)
                {
                    return false;
                }

                // Check if current time is within availability window
                return currentTime >= todaySchedule.StartTime && currentTime <= todaySchedule.EndTime;
            }
            catch
            {
                return false;
            }
        }

        private string GetAvailabilityMessage(bool isAvailable, Booking booking)
        {
            if (isAvailable)
            {
                return "Available for this booking";
            }
            else
            {
                var dayOfWeek = booking.StartTime.DayOfWeek.ToString();
                return $"Not available on {dayOfWeek} at this time";
            }
        }

        private string GetCurrentAvailabilityStatus(bool isAvailableToday)
        {
            return isAvailableToday ? "Available Today" : "Not Available Today";
        }

        // You can add other actions for pet owners here
        public IActionResult Schedule()
        {
            return View();
        }

        public async Task<IActionResult> MyServices()
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var userIdInt = int.Parse(userId);

                // Get service provider profile
                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProvider == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }

                // Get all services for this provider
                var services = await _serviceService.GetServicesByProviderIdAsync(serviceProvider.Id);

                var viewModel = new ServiceProviderMyServicesViewModel
                {
                    TotalServices = services.Count(),
                    ActiveServices = services.Count(s => s.IsActive),
                    InactiveServices = services.Count(s => !s.IsActive)
                };

                foreach (var service in services)
                {
                    // Get booking count for this service
                    var serviceBookings = await _bookingService.GetBookingsByServiceIdAsync(service.Id);
                    
                    // Calculate average rating
                    var averageRating = await _serviceService.GetServiceRatingAsync(service.Id);

                    viewModel.Services.Add(new ServiceItem
                    {
                        Id = service.Id,
                        Title = service.Title,
                        Description = service.Description,
                        Type = service.Type,
                        BasePrice = service.BasePrice,
                        PriceUnit = service.PriceUnit,
                        Location = service.Location,
                        IsActive = service.IsActive,
                        CreatedAt = service.CreatedAt,
                        UpdatedAt = service.UpdatedAt,
                        TotalBookings = serviceBookings.Count(),
                        AverageRating = averageRating,
                        TotalReviews = service.Reviews?.Count ?? 0,
                        AcceptedPetTypes = service.AcceptedPetTypes,
                        AcceptedPetSizes = service.AcceptedPetSizes,
                        MaxPetsPerBooking = service.MaxPetsPerBooking,
                        PrimaryPhotoUrl = service.Photos?.FirstOrDefault(p => p.IsPrimary)?.Url ?? "/images/default-service.jpg",
                        Photos = service.Photos?.Select(p => new ServicePhotoItem
                        {
                            Id = p.Id,
                            Url = p.Url,
                            Caption = p.Caption,
                            IsPrimary = p.IsPrimary
                        }).ToList() ?? new List<ServicePhotoItem>()
                    });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.MyServices: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while loading your services. Please try again later.";
                return View(new ServiceProviderMyServicesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateService(ServiceFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Please check your input and try again." });
                }

                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var userIdInt = int.Parse(userId);

                // Get service provider profile
                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProvider == null)
                {
                    return Json(new { success = false, message = "Service provider profile not found." });
                }

                // Create new service
                var newService = new Service
                {
                    ProviderId = serviceProvider.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.Type,
                    BasePrice = model.BasePrice,
                    PriceUnit = model.PriceUnit,
                    Location = model.Location,
                    IsActive = model.IsActive,
                    AcceptedPetTypes = model.AcceptedPetTypes,
                    AcceptedPetSizes = model.AcceptedPetSizes,
                    MaxPetsPerBooking = model.MaxPetsPerBooking,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude
                };

                var createdService = await _serviceProviderService.AddServiceAsync(newService);

                return Json(new { success = true, message = "Service created successfully!", serviceId = createdService.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.CreateService: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while creating the service." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateService(ServiceFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Please check your input and try again." });
                }

                // Get existing service
                var existingService = await _serviceService.GetServiceByIdAsync(model.Id);
                if (existingService == null)
                {
                    return Json(new { success = false, message = "Service not found." });
                }

                // Update service properties
                existingService.Title = model.Title;
                existingService.Description = model.Description;
                existingService.Type = model.Type;
                existingService.BasePrice = model.BasePrice;
                existingService.PriceUnit = model.PriceUnit;
                existingService.Location = model.Location;
                existingService.IsActive = model.IsActive;
                existingService.AcceptedPetTypes = model.AcceptedPetTypes;
                existingService.AcceptedPetSizes = model.AcceptedPetSizes;
                existingService.MaxPetsPerBooking = model.MaxPetsPerBooking;
                existingService.Latitude = model.Latitude;
                existingService.Longitude = model.Longitude;

                await _serviceService.UpdateServiceAsync(existingService);

                return Json(new { success = true, message = "Service updated successfully!" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.UpdateService: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating the service." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteService(int serviceId)
        {
            try
            {
                // Check if service has any bookings
                var serviceBookings = await _bookingService.GetBookingsByServiceIdAsync(serviceId);
                if (serviceBookings.Any())
                {
                    return Json(new { success = false, message = "Cannot delete service with existing bookings." });
                }

                await _serviceService.DeleteServiceAsync(serviceId);

                return Json(new { success = true, message = "Service deleted successfully!" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.DeleteService: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while deleting the service." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleServiceStatus(int serviceId)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(serviceId);
                if (service == null)
                {
                    return Json(new { success = false, message = "Service not found." });
                }

                service.IsActive = !service.IsActive;
                await _serviceService.UpdateServiceAsync(service);

                var status = service.IsActive ? "activated" : "deactivated";
                return Json(new { success = true, message = $"Service {status} successfully!", isActive = service.IsActive });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.ToggleServiceStatus: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating the service status." });
            }
        }

        public IActionResult Reviews()
        {
            return View();
        }
        public IActionResult Earnings()
        {
            return View();
        }
        public IActionResult Reports()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RejectBooking(int bookingId, string reason)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                // Update booking status to Declined
                booking.Status = BookingStatus.Declined;
                booking.Notes = reason;
                booking.UpdatedAt = DateTime.UtcNow;

                // TODO: Update booking in database
                // await _bookingService.UpdateBookingAsync(booking);

                return Json(new { success = true, message = "Booking declined successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.RejectBooking: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while declining the booking" });
            }
        }
    }
}

