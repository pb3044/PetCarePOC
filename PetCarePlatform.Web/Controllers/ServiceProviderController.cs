using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using WebModels = PetCarePlatform.Web.Models;
using PetCarePlatform.Core.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;

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
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return RedirectToAction("Login", "Account");
                }

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
                var dashboard = new WebModels.ServiceProviderDashboardViewModel
                {
                    ProviderInfo = new WebModels.ServiceProviderInfo
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

                    dashboard.RecentRequests.Add(new WebModels.RecentBookingRequestViewModel
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
                    dashboard.TodaySchedule.Add(new WebModels.TodayScheduleViewModel
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
                return View(new WebModels.ServiceProviderDashboardViewModel());
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
        public async Task<IActionResult> Schedule(DateTime? date = null, string viewType = "day")
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get service provider profile
                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProvider == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }

                var currentDate = date ?? DateTime.Now;
                var startOfDay = currentDate.Date;
                var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);
                var startOfWeek = startOfDay.AddDays(-(int)startOfDay.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

                // Get all bookings for this provider
                var allBookings = await _bookingService.GetBookingsByProviderIdAsync(serviceProvider.Id);

                // Get availability schedule
                var availabilitySchedules = await _serviceProviderService.GetAvailabilityScheduleAsync(serviceProvider.Id);

                var viewModel = new WebModels.ServiceProviderScheduleViewModel
                {
                    CurrentDate = currentDate,
                    CurrentView = viewType
                };

                // Populate appointments based on view type
                var filteredBookings = viewType switch
                {
                    "day" => allBookings.Where(b => b.StartTime.Date == currentDate.Date),
                    "week" => allBookings.Where(b => b.StartTime >= startOfWeek && b.StartTime <= endOfWeek),
                    "month" => allBookings.Where(b => b.StartTime.Month == currentDate.Month && b.StartTime.Year == currentDate.Year),
                    _ => allBookings.Where(b => b.StartTime.Date == currentDate.Date)
                };

                foreach (var booking in filteredBookings.OrderBy(b => b.StartTime))
                {
                    var duration = booking.EndTime - booking.StartTime;
                    var durationText = duration.TotalHours >= 1 
                        ? $"{duration.TotalHours:F1} hours" 
                        : $"{duration.TotalMinutes} minutes";

                    viewModel.Appointments.Add(new WebModels.ScheduleAppointment
                    {
                        Id = booking.Id,
                        ServiceName = booking.Service?.Title ?? "",
                        PetOwnerName = $"{booking.Owner?.User?.FirstName} {booking.Owner?.User?.LastName}",
                        PetName = booking.Pet?.Name ?? "",
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime,
                        Status = booking.Status.ToString(),
                        TotalPrice = booking.TotalPrice,
                        SpecialInstructions = booking.SpecialInstructions ?? "",
                        Duration = durationText,
                        TimeSlot = $"{booking.StartTime:HH:mm} - {booking.EndTime:HH:mm}"
                    });
                }

                // Populate availability schedule
                foreach (var availability in availabilitySchedules)
                {
                    viewModel.Availability.Add(new WebModels.AvailabilitySchedule
                    {
                        Id = availability.Id,
                        DayOfWeek = availability.DayOfWeek,
                        IsAvailable = availability.IsAvailable,
                        StartTime = availability.StartTime,
                        EndTime = availability.EndTime,
                        DayName = availability.DayOfWeek.ToString()
                    });
                }

                // Calculate statistics
                var todayBookings = allBookings.Where(b => b.StartTime.Date == DateTime.Now.Date).ToList();
                var weekBookings = allBookings.Where(b => b.StartTime >= DateTime.Now.Date.AddDays(-7)).ToList();

                viewModel.Statistics = new WebModels.ScheduleStatistics
                {
                    TodayAppointments = todayBookings.Count,
                    WeekAppointments = weekBookings.Count,
                    AvailableSlots = availabilitySchedules.Count(a => a.IsAvailable),
                    BlockedHours = 0, // TODO: Calculate blocked hours
                    TodayEarnings = todayBookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice),
                    WeekEarnings = weekBookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalPrice)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Schedule: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while loading your schedule. Please try again later.";
                return View(new WebModels.ServiceProviderScheduleViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetScheduleData(DateTime date, string viewType)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProvider == null)
                {
                    return Json(new { success = false, message = "Service provider not found" });
                }

                var allBookings = await _bookingService.GetBookingsByProviderIdAsync(serviceProvider.Id);
                var filteredBookings = viewType switch
                {
                    "day" => allBookings.Where(b => b.StartTime.Date == date.Date),
                    "week" => allBookings.Where(b => b.StartTime >= date.Date && b.StartTime <= date.Date.AddDays(7)),
                    "month" => allBookings.Where(b => b.StartTime.Month == date.Month && b.StartTime.Year == date.Year),
                    _ => allBookings.Where(b => b.StartTime.Date == date.Date)
                };

                var appointments = filteredBookings.Select(b => new
                {
                    id = b.Id,
                    title = b.Service?.Title ?? "",
                    start = b.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = b.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    status = b.Status.ToString(),
                    petOwnerName = $"{b.Owner?.User?.FirstName} {b.Owner?.User?.LastName}",
                    petName = b.Pet?.Name ?? "",
                    totalPrice = b.TotalPrice
                }).ToList();

                return Json(new { success = true, appointments });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.GetScheduleData: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while loading schedule data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAvailability(PetCarePlatform.Core.Models.AvailabilitySchedule model)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProvider == null)
                {
                    return Json(new { success = false, message = "Service provider not found" });
                }

                // Update availability schedule
                var availabilitySchedules = await _serviceProviderService.GetAvailabilityScheduleAsync(serviceProvider.Id);
                var existingSchedule = availabilitySchedules.FirstOrDefault(a => a.DayOfWeek == model.DayOfWeek);

                if (existingSchedule != null)
                {
                    existingSchedule.IsAvailable = model.IsAvailable;
                    existingSchedule.StartTime = model.StartTime;
                    existingSchedule.EndTime = model.EndTime;
                    // TODO: Update in database
                }

                return Json(new { success = true, message = "Availability updated successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.UpdateAvailability: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating availability" });
            }
        }

        public async Task<IActionResult> MyServices()
        {
            try
            {
                // Get current user ID
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get service provider profile
                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProvider == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }

                // Get all services for this provider
                var services = await _serviceService.GetServicesByProviderIdAsync(serviceProvider.Id);

                var viewModel = new WebModels.ServiceProviderMyServicesViewModel
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

                    viewModel.Services.Add(new WebModels.ServiceItem
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
                        Photos = service.Photos?.Select(p => new WebModels.ServicePhotoItem
                        {
                            Id = p.Id,
                            Url = p.Url,
                            Caption = p.Caption,
                            IsPrimary = p.IsPrimary
                        }).ToList() ?? new List<WebModels.ServicePhotoItem>()
                    });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.MyServices: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while loading your services. Please try again later.";
                return View(new WebModels.ServiceProviderMyServicesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateService(WebModels.ServiceFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Please check your input and try again." });
                }

                // Get current user ID
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

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
                    Type = Enum.Parse<ServiceType>(model.Type),
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
        public async Task<IActionResult> UpdateService(WebModels.ServiceFormModel model)
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
                existingService.Type = Enum.Parse<ServiceType>(model.Type);
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

                // Update booking in database
                await _bookingService.UpdateBookingAsync(booking);

                return Json(new { success = true, message = "Booking declined successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.RejectBooking: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while declining the booking" });
            }
        }

        public async Task<IActionResult> BookingRequest(WebModels.BookingRequestFilters filters = null)
        {
            try
            {
                // Get current user ID
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get service provider profile
                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProvider == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }

                // Initialize filters if null
                filters ??= new WebModels.BookingRequestFilters();

                // Get all bookings for this provider
                var allBookings = await _bookingService.GetBookingsByProviderIdAsync(serviceProvider.Id);

                // Apply filters
                var filteredBookings = allBookings.AsQueryable();

                if (!string.IsNullOrEmpty(filters.Status))
                {
                    if (Enum.TryParse<BookingStatus>(filters.Status, out var status))
                    {
                        filteredBookings = filteredBookings.Where(b => b.Status == status);
                    }
                }

                if (filters.FromDate.HasValue)
                {
                    filteredBookings = filteredBookings.Where(b => b.StartTime.Date >= filters.FromDate.Value.Date);
                }

                if (filters.ToDate.HasValue)
                {
                    filteredBookings = filteredBookings.Where(b => b.StartTime.Date <= filters.ToDate.Value.Date);
                }

                if (!string.IsNullOrEmpty(filters.ServiceType))
                {
                    if (Enum.TryParse<ServiceType>(filters.ServiceType, out var serviceType))
                    {
                        filteredBookings = filteredBookings.Where(b => b.Service.Type == serviceType);
                    }
                }

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    var searchTerm = filters.SearchTerm.ToLower();
                    filteredBookings = filteredBookings.Where(b =>
                        b.Owner.User.FirstName.ToLower().Contains(searchTerm) ||
                        b.Owner.User.LastName.ToLower().Contains(searchTerm) ||
                        b.Service.Title.ToLower().Contains(searchTerm) ||
                        b.Pet.Name.ToLower().Contains(searchTerm)
                    );
                }

                if (filters.ShowOnlyAvailable == true)
                {
                    // Filter to show only bookings where provider is available
                    var availableBookings = new List<Booking>();
                    foreach (var booking in filteredBookings)
                    {
                        var isAvailable = await CheckAvailabilityForBooking(serviceProvider.Id, booking);
                        if (isAvailable)
                        {
                            availableBookings.Add(booking);
                        }
                    }
                    filteredBookings = availableBookings.AsQueryable();
                }

                // Separate pending and recent requests
                var pendingRequests = filteredBookings
                    .Where(b => b.Status == BookingStatus.Pending)
                    .OrderBy(b => b.CreatedAt)
                    .ToList();

                var recentRequests = filteredBookings
                    .Where(b => b.Status != BookingStatus.Pending && b.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(20)
                    .ToList();

                // Create view model
                var viewModel = new WebModels.ServiceProviderBookingRequestViewModel
                {
                    Filters = filters,
                    TotalPendingCount = pendingRequests.Count,
                    TotalRecentCount = recentRequests.Count
                };

                // Populate pending requests
                foreach (var booking in pendingRequests)
                {
                    var isAvailable = await CheckAvailabilityForBooking(serviceProvider.Id, booking);
                    var duration = booking.EndTime - booking.StartTime;
                    var durationText = duration.TotalHours >= 1 
                        ? $"{duration.TotalHours:F1} hours" 
                        : $"{duration.TotalMinutes} minutes";

                    viewModel.PendingRequests.Add(new WebModels.BookingRequestItem
                    {
                        Id = booking.Id,
                        PetOwnerName = $"{booking.Owner.User.FirstName} {booking.Owner.User.LastName}",
                        PetOwnerEmail = booking.Owner.User.Email,
                        PetOwnerPhone = booking.Owner.User.PhoneNumber ?? "",
                        ServiceName = booking.Service.Title,
                        PetName = booking.Pet.Name,
                        PetType = booking.Pet.Type.ToString(),
                        PetBreed = booking.Pet.Breed ?? "",
                        RequestDate = booking.CreatedAt,
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime,
                        Duration = durationText,
                        TotalPrice = booking.TotalPrice,
                        Status = booking.Status.ToString(),
                        SpecialInstructions = booking.SpecialInstructions ?? "",
                        Notes = booking.Notes ?? "",
                        IsAvailable = isAvailable,
                        AvailabilityMessage = GetAvailabilityMessage(isAvailable, booking),
                        PetOwnerAddress = $"{booking.Owner.User.Address}, {booking.Owner.User.City}, {booking.Owner.User.Province}",
                        Distance = 0, // TODO: Calculate distance
                        EstimatedTravelTime = "15 min" // TODO: Calculate travel time
                    });
                }

                // Populate recent requests
                foreach (var booking in recentRequests)
                {
                    var duration = booking.EndTime - booking.StartTime;
                    var durationText = duration.TotalHours >= 1 
                        ? $"{duration.TotalHours:F1} hours" 
                        : $"{duration.TotalMinutes} minutes";

                    viewModel.RecentRequests.Add(new WebModels.BookingRequestItem
                    {
                        Id = booking.Id,
                        PetOwnerName = $"{booking.Owner.User.FirstName} {booking.Owner.User.LastName}",
                        PetOwnerEmail = booking.Owner.User.Email,
                        PetOwnerPhone = booking.Owner.User.PhoneNumber ?? "",
                        ServiceName = booking.Service.Title,
                        PetName = booking.Pet.Name,
                        PetType = booking.Pet.Type.ToString(),
                        PetBreed = booking.Pet.Breed ?? "",
                        RequestDate = booking.CreatedAt,
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime,
                        Duration = durationText,
                        TotalPrice = booking.TotalPrice,
                        Status = booking.Status.ToString(),
                        SpecialInstructions = booking.SpecialInstructions ?? "",
                        Notes = booking.Notes ?? "",
                        IsAvailable = false, // Not relevant for completed/declined bookings
                        AvailabilityMessage = "",
                        PetOwnerAddress = $"{booking.Owner.User.Address}, {booking.Owner.User.City}, {booking.Owner.User.Province}",
                        Distance = 0,
                        EstimatedTravelTime = "15 min"
                    });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.BookingRequest: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while loading booking requests. Please try again later.";
                return View(new WebModels.ServiceProviderBookingRequestViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AcceptBooking(int bookingId)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                // Check if provider is available for this booking
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }
                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                
                var isAvailable = await CheckAvailabilityForBooking(serviceProvider.Id, booking);
                if (!isAvailable)
                {
                    return Json(new { success = false, message = "You are not available for this booking time" });
                }

                // Update booking status to Confirmed
                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;

                await _bookingService.UpdateBookingAsync(booking);

                return Json(new { success = true, message = "Booking accepted successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.AcceptBooking: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while accepting the booking" });
            }
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get user and service provider
                var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProvider == null || serviceProvider.User == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }
                var user = serviceProvider.User;

                var reviews = await _serviceProviderService.GetProviderReviewsAsync(serviceProvider.Id);
                var reviewViewModels = reviews.Select(r => new WebModels.ReviewViewModel
                {
                    Id = r.Id,
                    BookingId = r.BookingId,
                    ServiceName = r.Service?.Title ?? string.Empty,
                    ReviewerName = r.Reviewer != null ? (r.Reviewer.FirstName + " " + r.Reviewer.LastName) : string.Empty,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList();

                var viewModel = new WebModels.ServiceProviderProfileViewModel
                {
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Email = user.Email ?? "",
                    Phone = user.PhoneNumber ?? "",
                    Address = user.Address ?? "",
                    City = user.City ?? "",
                    Province = user.Province ?? "",
                    PostalCode = user.PostalCode ?? "",
                    Bio = user.Bio ?? "",
                    BusinessName = serviceProvider.BusinessName ?? "",
                    Description = serviceProvider.Description ?? "",
                    TaxInfo = serviceProvider.TaxInfo ?? "",
                    TaxNumber = serviceProvider.TaxInfo ?? "",
                    BusinessDescription = serviceProvider.Description ?? "",
                    SpecialNotes = "",
                    AverageRating = serviceProvider.AverageRating,
                    TotalReviews = serviceProvider.TotalReviews,
                    Credentials = serviceProvider.Credentials ?? "",
                    Certifications = serviceProvider.Certifications ?? "",
                    InsuranceInfo = serviceProvider.InsuranceInfo ?? "",
                    LicenseInfo = serviceProvider.LicenseInfo ?? "",
                    BackgroundCheckVerified = serviceProvider.BackgroundCheckVerified,
                    BackgroundCheckDate = serviceProvider.BackgroundCheckDate,
                    IdentityVerified = serviceProvider.IdentityVerified,
                    ServiceArea = serviceProvider.ServiceArea ?? "",
                    ServiceRadius = serviceProvider.ServiceRadius,
                    BankingInfo = serviceProvider.BankingInfo ?? "",
                    Reviews = reviewViewModels
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Profile: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while loading your profile. Please try again later.";
                return View(new WebModels.ServiceProviderProfileViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetService(int id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                {
                    return Json(new { success = false, message = "Service not found." });
                }

                // Return service data for editing
                var serviceData = new
                {
                    success = true,
                    service = new
                    {
                        Id = service.Id,
                        Title = service.Title,
                        Description = service.Description,
                        Type = service.Type.ToString(), // Fix: Convert ServiceType enum to string for serialization
                        BasePrice = service.BasePrice,
                        PriceUnit = service.PriceUnit,
                        Location = service.Location,
                        IsActive = service.IsActive,
                        AcceptedPetTypes = service.AcceptedPetTypes,
                        AcceptedPetSizes = service.AcceptedPetSizes,
                        MaxPetsPerBooking = service.MaxPetsPerBooking
                    }
                };

                return Json(serviceData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.GetService: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while retrieving the service." });
            }
        }
    }
}

