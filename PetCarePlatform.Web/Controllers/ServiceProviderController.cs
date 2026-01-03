using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using WebModels = PetCarePlatform.Web.Models;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.DTOs.Requests;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using System.IO;

namespace PetCarePlatform.Web.Controllers
{
    public class ServiceProviderController : Controller
    {
        private readonly IServiceProviderService _serviceProviderService;
        private readonly IBookingService _bookingService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceService _serviceService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IReviewService _reviewService;

        public ServiceProviderController(
            IServiceProviderService serviceProviderService,
            IBookingService bookingService,
            IBookingRepository bookingRepository,
            IServiceService serviceService,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IReviewService reviewService)
        {
            _serviceProviderService = serviceProviderService;
            _bookingService = bookingService;
            _bookingRepository = bookingRepository;
            _serviceService = serviceService;
            _userManager = userManager;
            _emailService = emailService;
            _reviewService = reviewService;
        }

        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Get current user ID
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if service provider profile already exists
                var existingProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (existingProvider != null)
                {
                    return RedirectToAction("Dashboard");
                }

                // Get user information
                var user = await _userManager.FindByIdAsync(userIdInt.ToString());
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var viewModel = new WebModels.ServiceProviderOnboardingViewModel
                {
                    UserId = userIdInt,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Email = user.Email ?? "",
                    Phone = user.PhoneNumber ?? "",
                    Address = user.Address ?? "",
                    City = user.City ?? "",
                    Province = user.Province ?? "",
                    PostalCode = user.PostalCode ?? "",
                    Bio = user.Bio ?? ""
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Create: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the onboarding form. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Create(WebModels.ServiceProviderOnboardingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Get current user ID
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if service provider profile already exists
                var existingProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (existingProvider != null)
                {
                    return RedirectToAction("Dashboard");
                }

                // Create service provider profile
                var serviceProvider = new PetCarePlatform.Core.Models.ServiceProvider
                {
                    UserId = userIdInt,
                    BusinessName = model.BusinessName,
                    BusinessType = model.BusinessType,
                    BusinessNumber = model.BusinessNumber,
                    Description = model.Description,
                    Credentials = model.Credentials,
                    Certifications = model.Certifications,
                    InsuranceInfo = model.InsuranceInfo,
                    LicenseInfo = model.LicenseInfo,
                    ServiceArea = model.ServiceArea,
                    ServiceRadius = model.ServiceRadius,
                    BankingInfo = model.BankingInfo,
                    TaxInfo = model.TaxInfo,
                    IsActive = false, // Will be activated after verification
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createRequest = new PetCarePlatform.Core.DTOs.Requests.CreateServiceProviderRequest
                {
                    UserId = userIdInt,
                    BusinessName = model.BusinessName,
                    BusinessType = model.BusinessType,
                    BusinessNumber = model.BusinessNumber,
                    Description = model.Description,
                    ServiceArea = model.ServiceArea,
                    ServiceRadius = model.ServiceRadius
                };
                var createdProviderResult = await _serviceProviderService.CreateServiceProviderProfileAsync(createRequest, CancellationToken.None);
                if (createdProviderResult.IsFailure)
                {
                    ModelState.AddModelError("", createdProviderResult.ErrorMessage);
                    return View(model);
                }
                var createdProvider = createdProviderResult.Value;

                // Update user information
                var user = await _userManager.FindByIdAsync(userIdInt.ToString());
                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.Phone;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.Province = model.Province;
                    user.PostalCode = model.PostalCode;
                    user.Bio = model.Bio;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                TempData["Success"] = "Service provider profile created successfully! Your profile is now under review and will be activated soon.";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Create POST: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating your profile. Please try again.");
                return View(model);
            }
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
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }
                var serviceProvider = serviceProviderResult.Value;

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
                        UserName = serviceProvider.UserName ?? "",
                        Email = serviceProvider.UserEmail ?? "",
                        AverageRating = serviceProvider.AverageRating,
                        TotalReviews = serviceProvider.TotalReviews
                    }
                };

                // Get all bookings for this provider using BookingQuery
                var bookingQuery = new BookingQuery { ProviderId = serviceProvider.Id, PageSize = 1000 };
                var bookingsResult = await _bookingService.GetBookingsAsync(bookingQuery);
                var allBookings = bookingsResult.IsSuccess ? bookingsResult.Value.Items : new List<BookingResponse>();
                
                // Calculate total bookings
                dashboard.TotalBookings = allBookings.Count();

                // Calculate pending requests (bookings with Pending status)
                var pendingBookings = allBookings.Where(b => b.Status == BookingStatus.Pending).ToList();
                dashboard.PendingRequests = pendingBookings.Count;

                // Get active services using ServiceQuery
                var serviceQuery = new ServiceQuery { ProviderId = serviceProvider.Id, PageSize = 1000 };
                var servicesResult = await _serviceService.GetServicesAsync(serviceQuery);
                var activeServices = servicesResult.IsSuccess ? servicesResult.Value.Items : new List<ServiceResponse>();
                dashboard.ActiveServices = activeServices.Count(s => s.IsActive);

                // Calculate monthly earnings
                var monthlyBookings = allBookings.Where(b => 
                    b.Status == BookingStatus.Completed && 
                    b.CreatedAt >= startOfMonth).ToList();
                dashboard.MonthlyEarnings = monthlyBookings.Sum(b => b.TotalPrice);

                // Get recent requests (last 30 days, excluding pending) using repository to get full entities with navigation properties
                var allBookingsFromRepo = (await _bookingRepository.GetByProviderIdAsync(serviceProvider.Id)).ToList();
                var recentBookingsFromRepo = allBookingsFromRepo
                    .Where(b => b.Status != BookingStatus.Pending && b.Status != BookingStatus.Requested && 
                               b.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(20)
                    .ToList();

                foreach (var booking in recentBookingsFromRepo)
                {
                    dashboard.RecentRequests.Add(new WebModels.RecentBookingRequestViewModel
                    {
                        Id = booking.Id,
                        PetOwnerName = $"{booking.Owner?.User?.FirstName} {booking.Owner?.User?.LastName}".Trim(),
                        PetOwnerEmail = booking.Owner?.User?.Email ?? string.Empty,
                        PetOwnerPhone = booking.Owner?.User?.PhoneNumber ?? string.Empty,
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
                        ServiceName = booking.ServiceName,
                        PetOwnerName = booking.OwnerName,
                        PetName = booking.PetName ?? "",
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
                var availabilityResult = await _serviceProviderService.GetAvailabilityScheduleAsync(providerId);
                if (availabilityResult.IsFailure || availabilityResult.Value == null)
                {
                    return false;
                }
                var availabilitySchedules = availabilityResult.Value;
                
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

        private async Task<bool> CheckAvailabilityForBooking(int providerId, DateTime startTime, DateTime endTime)
        {
            try
            {
                // Get provider's availability schedule
                var availabilityResult = await _serviceProviderService.GetAvailabilityScheduleAsync(providerId);
                if (availabilityResult.IsFailure || availabilityResult.Value == null)
                {
                    return false;
                }
                var availabilitySchedules = availabilityResult.Value;
                
                // Check if the booking time falls within provider's availability
                var dayOfWeek = (int)startTime.DayOfWeek;
                var startTimeOfDay = startTime.TimeOfDay;
                var endTimeOfDay = endTime.TimeOfDay;

                var daySchedule = availabilitySchedules.FirstOrDefault(s => (int)s.DayOfWeek == dayOfWeek);
                
                if (daySchedule == null || !daySchedule.IsAvailable)
                {
                    return false;
                }

                // Check if booking time overlaps with availability
                return startTimeOfDay >= daySchedule.StartTime && endTimeOfDay <= daySchedule.EndTime;
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

                var availabilityResult = await _serviceProviderService.GetAvailabilityScheduleAsync(providerId);
                if (availabilityResult.IsFailure || availabilityResult.Value == null)
                {
                    return false;
                }
                var availabilitySchedules = availabilityResult.Value;
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
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }
                var serviceProvider = serviceProviderResult.Value;

                var currentDate = date ?? DateTime.Now;
                var startOfDay = currentDate.Date;
                var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);
                var startOfWeek = startOfDay.AddDays(-(int)startOfDay.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

                // Get all bookings for this provider
                var bookingQuery = new BookingQuery { ProviderId = serviceProvider.Id, PageSize = 1000 };
                var bookingsResult = await _bookingService.GetBookingsAsync(bookingQuery);
                if (bookingsResult.IsFailure || bookingsResult.Value == null)
                {
                    TempData["Error"] = "Failed to load bookings";
                    return RedirectToAction("Dashboard");
                }
                var allBookings = bookingsResult.Value;

                // Get availability schedule
                var availabilityResult = await _serviceProviderService.GetAvailabilityScheduleAsync(serviceProvider.Id);
                if (availabilityResult.IsFailure || availabilityResult.Value == null)
                {
                    TempData["Error"] = "Failed to load availability schedule";
                    return RedirectToAction("Dashboard");
                }
                var availabilitySchedules = availabilityResult.Value;

                var viewModel = new WebModels.ServiceProviderScheduleViewModel
                {
                    CurrentDate = currentDate,
                    CurrentView = viewType
                };

                // Populate appointments based on view type
                var filteredBookings = viewType switch
                {
                    "day" => allBookings.Items.Where(b => b.StartTime.Date == currentDate.Date),
                    "week" => allBookings.Items.Where(b => b.StartTime >= startOfWeek && b.StartTime <= endOfWeek),
                    "month" => allBookings.Items.Where(b => b.StartTime.Month == currentDate.Month && b.StartTime.Year == currentDate.Year),
                    _ => allBookings.Items.Where(b => b.StartTime.Date == currentDate.Date)
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
                        ServiceName = booking.ServiceName ?? "",
                        PetOwnerName = booking.OwnerName ?? "",
                        PetName = booking.PetName ?? "",
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
                var todayBookings = allBookings.Items.Where(b => b.StartTime.Date == DateTime.Now.Date).ToList();
                var weekBookings = allBookings.Items.Where(b => b.StartTime >= DateTime.Now.Date.AddDays(-7)).ToList();

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

                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return Json(new { success = false, message = "Service provider not found" });
                }
                var serviceProvider = serviceProviderResult.Value;

                var bookingQuery = new BookingQuery { ProviderId = serviceProvider.Id, PageSize = 1000 };
                var bookingsResult = await _bookingService.GetBookingsAsync(bookingQuery);
                if (bookingsResult.IsFailure || bookingsResult.Value == null)
                {
                    return Json(new { success = false, message = "Failed to load bookings" });
                }
                var allBookings = bookingsResult.Value.Items;
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
                    title = b.ServiceName ?? "",
                    start = b.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = b.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    status = b.Status.ToString(),
                    petOwnerName = b.OwnerName ?? "",
                    petName = b.PetName ?? "",
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

                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return Json(new { success = false, message = "Service provider not found" });
                }
                var serviceProvider = serviceProviderResult.Value;

                // Update availability schedule
                var availabilityResult = await _serviceProviderService.GetAvailabilityScheduleAsync(serviceProvider.Id);
                if (availabilityResult.IsFailure || availabilityResult.Value == null)
                {
                    return Json(new { success = false, message = "Failed to load availability schedule" });
                }
                var availabilitySchedules = availabilityResult.Value;
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
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }
                var serviceProvider = serviceProviderResult.Value;

                // Get all services for this provider
                var servicesResult = await _serviceService.GetServicesByProviderIdAsync(serviceProvider.Id);
                if (servicesResult.IsFailure || servicesResult.Value == null)
                {
                    TempData["Error"] = "Failed to load services";
                    return RedirectToAction("Dashboard");
                }
                var services = servicesResult.Value;

                var viewModel = new WebModels.ServiceProviderMyServicesViewModel
                {
                    TotalServices = services.Count(),
                    ActiveServices = services.Count(s => s.IsActive),
                    InactiveServices = services.Count(s => !s.IsActive)
                };

                foreach (var service in services)
                {
                    // Get booking count for this service
                    var serviceBookingQuery = new BookingQuery { ServiceId = service.Id, PageSize = 1000 };
                    var serviceBookingsResult = await _bookingService.GetBookingsAsync(serviceBookingQuery);
                    var serviceBookings = serviceBookingsResult.IsSuccess && serviceBookingsResult.Value != null 
                        ? serviceBookingsResult.Value.Items 
                        : Enumerable.Empty<BookingResponse>();
                    
                    // Calculate average rating
                    var ratingResult = await _serviceService.GetServiceRatingAsync(service.Id);
                    var averageRating = ratingResult.IsSuccess 
                        ? ratingResult.Value 
                        : 0.0;

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
                        TotalReviews = service.ReviewCount,
                        AcceptedPetTypes = service.AcceptedPetTypes,
                        AcceptedPetSizes = service.AcceptedPetSizes,
                        MaxPetsPerBooking = service.MaxPetsPerBooking ?? 1,
                        PrimaryPhotoUrl = service.PhotoUrls?.FirstOrDefault() ?? "/images/default-service.jpg",
                        Photos = service.PhotoUrls?.Select((url, index) => new WebModels.ServicePhotoItem
                        {
                            Id = index,
                            Url = url,
                            Caption = "",
                            IsPrimary = index == 0
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
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return Json(new { success = false, message = "Service provider profile not found." });
                }
                var serviceProvider = serviceProviderResult.Value;

                // Create new service
                var createRequest = new PetCarePlatform.Core.DTOs.Requests.CreateServiceRequest
                {
                    ProviderId = serviceProvider.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Type = Enum.Parse<ServiceType>(model.Type),
                    BasePrice = model.BasePrice,
                    PriceUnit = model.PriceUnit,
                    Location = model.Location,
                    AcceptedPetTypes = model.AcceptedPetTypes,
                    AcceptedPetSizes = model.AcceptedPetSizes,
                    MaxPetsPerBooking = model.MaxPetsPerBooking,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude
                };

                var createResult = await _serviceService.CreateServiceAsync(createRequest);
                if (createResult.IsFailure || createResult.Value == null)
                {
                    return Json(new { success = false, message = createResult.ErrorMessage ?? "Failed to create service." });
                }
                var createdService = createResult.Value;

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
                var serviceResult = await _serviceService.GetServiceByIdAsync(model.Id);
                if (serviceResult.IsFailure || serviceResult.Value == null)
                {
                    return Json(new { success = false, message = "Service not found." });
                }

                // Update service properties
                var updateRequest = new PetCarePlatform.Core.DTOs.Requests.UpdateServiceRequest
                {
                    ServiceId = model.Id,
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

                var updateResult = await _serviceService.UpdateServiceAsync(updateRequest);
                if (updateResult.IsFailure)
                {
                    return Json(new { success = false, message = updateResult.ErrorMessage ?? "Failed to update service." });
                }

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
                var bookingQuery = new BookingQuery { ServiceId = serviceId, PageSize = 1 };
                var bookingsResult = await _bookingService.GetBookingsAsync(bookingQuery);
                if (bookingsResult.IsSuccess && bookingsResult.Value != null && bookingsResult.Value.Items.Any())
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
                var serviceResult = await _serviceService.GetServiceByIdAsync(serviceId);
                if (serviceResult.IsFailure || serviceResult.Value == null)
                {
                    return Json(new { success = false, message = "Service not found." });
                }

                var service = serviceResult.Value;
                var updateRequest = new PetCarePlatform.Core.DTOs.Requests.UpdateServiceRequest
                {
                    ServiceId = service.Id,
                    Title = service.Title,
                    Description = service.Description,
                    Type = service.Type,
                    BasePrice = service.BasePrice,
                    PriceUnit = service.PriceUnit,
                    Location = service.Location,
                    IsActive = !service.IsActive,
                    AcceptedPetTypes = service.AcceptedPetTypes,
                    AcceptedPetSizes = service.AcceptedPetSizes,
                    MaxPetsPerBooking = service.MaxPetsPerBooking,
                    Latitude = service.Latitude,
                    Longitude = service.Longitude
                };

                var updateResult = await _serviceService.UpdateServiceAsync(updateRequest);
                if (updateResult.IsFailure)
                {
                    return Json(new { success = false, message = updateResult.ErrorMessage ?? "Failed to update service status." });
                }

                var status = updateRequest.IsActive ? "activated" : "deactivated";
                return Json(new { success = true, message = $"Service {status} successfully!", isActive = updateRequest.IsActive });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.ToggleServiceStatus: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating the service status." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Reviews()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userId);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    TempData["Error"] = "Service provider profile not found.";
                    return RedirectToAction("Dashboard");
                }
                var serviceProvider = serviceProviderResult.Value;

                var breakdownResult = await _reviewService.GetRatingBreakdownAsync(serviceProvider.Id, CancellationToken.None);
                var breakdown = breakdownResult.IsSuccess ? breakdownResult.Value : null;

                ViewBag.AverageRating = serviceProvider.AverageRating;
                ViewBag.TotalReviews = serviceProvider.TotalReviews;
                ViewBag.FiveStarCount = breakdown?.FiveStarCount ?? 0;
                ViewBag.FourStarCount = breakdown?.FourStarCount ?? 0;
                ViewBag.ThreeStarCount = breakdown?.ThreeStarCount ?? 0;
                ViewBag.TwoStarCount = breakdown?.TwoStarCount ?? 0;
                ViewBag.OneStarCount = breakdown?.OneStarCount ?? 0;

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Reviews: {ex.Message}");
                TempData["Error"] = "An error occurred while loading reviews.";
                return RedirectToAction("Dashboard");
            }
        }
        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Earnings()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userId);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    TempData["Error"] = "Service provider profile not found.";
                    return RedirectToAction("Dashboard");
                }
                var serviceProvider = serviceProviderResult.Value;

                var bookingQuery = new BookingQuery { ProviderId = serviceProvider.Id, PageSize = 1000 };
                var bookingsResult = await _bookingService.GetBookingsAsync(bookingQuery);
                var allBookings = bookingsResult.IsSuccess ? bookingsResult.Value.Items : new List<BookingResponse>();

                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                var totalEarnings = allBookings
                    .Where(b => b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice);

                var thisMonthEarnings = allBookings
                    .Where(b => b.Status == BookingStatus.Completed && b.CreatedAt >= startOfMonth)
                    .Sum(b => b.TotalPrice);

                var pendingPayments = allBookings
                    .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress)
                    .Count();

                var completedPayments = allBookings
                    .Where(b => b.Status == BookingStatus.Completed)
                    .Count();

                ViewBag.TotalEarnings = totalEarnings.ToString("F2");
                ViewBag.ThisMonthEarnings = thisMonthEarnings.ToString("F2");
                ViewBag.PendingPayments = pendingPayments;
                ViewBag.CompletedPayments = completedPayments;

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Earnings: {ex.Message}");
                TempData["Error"] = "An error occurred while loading earnings.";
                return RedirectToAction("Dashboard");
            }
        }
        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Reports()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userId);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    TempData["Error"] = "Service provider profile not found.";
                    return RedirectToAction("Dashboard");
                }
                var serviceProvider = serviceProviderResult.Value;

                ViewBag.ProviderId = serviceProvider.Id;
                ViewBag.ProviderName = serviceProvider.BusinessName;

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Reports: {ex.Message}");
                TempData["Error"] = "An error occurred while loading reports.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectBooking(int bookingId, string reason)
        {
            try
            {
                var bookingResult = await _bookingService.GetBookingByIdAsync(bookingId);
                if (bookingResult.IsFailure || bookingResult.Value == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                var booking = bookingResult.Value;

                // Update booking status to Declined
                await _bookingService.UpdateBookingStatusAsync(booking.Id, BookingStatus.Declined, CancellationToken.None);

                // Send email notification to pet owner
                try
                {
                    // Get owner email from user service - BookingResponse doesn't have navigation properties
                    var owner = await _serviceProviderService.GetServiceProviderByUserIdAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
                    if (owner != null)
                    {
                        var emailSubject = $"Booking Request Declined - {booking.ServiceName}";
                        var emailBody = $@"
                            <h2>Booking Request Declined</h2>
                            <p>We're sorry to inform you that your booking request has been declined by the service provider.</p>
                            <br>
                            <p><strong>Service:</strong> {booking.ServiceName}</p>
                            <p><strong>Pet:</strong> {booking.PetName ?? "N/A"}</p>
                            <p><strong>Requested Date & Time:</strong> {booking.StartTime:MMM dd, yyyy} at {booking.StartTime:hh:mm tt} - {booking.EndTime:hh:mm tt}</p>
                            <p><strong>Reason:</strong> {reason}</p>
                            <br>
                            <p>You can try booking with another service provider or choose a different time slot.</p>
                            <p>Thank you for using PetCare Platform!</p>
                        ";
                        
                        // Note: We need to get the owner's email from a different source since BookingResponse doesn't have it
                        // For now, skip email if we can't get the email address
                    }
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the booking rejection
                    Console.WriteLine($"Failed to send rejection email: {emailEx.Message}");
                }

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
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }
                var serviceProvider = serviceProviderResult.Value;

                // Initialize filters if null
                filters ??= new WebModels.BookingRequestFilters();

                // Get all bookings for this provider using repository to get full entities with navigation properties
                var allBookings = (await _bookingRepository.GetByProviderIdAsync(serviceProvider.Id)).ToList();

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
                    // Note: BookingResponse doesn't have Service.Type, so we can't filter by service type here
                    // This would require getting services first and filtering by service IDs
                }

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    var searchTerm = filters.SearchTerm.ToLower();
                    filteredBookings = filteredBookings.Where(b =>
                        (b.Owner != null && b.Owner.User != null && b.Owner.User.FirstName != null && b.Owner.User.FirstName.ToLower().Contains(searchTerm)) ||
                        (b.Owner != null && b.Owner.User != null && b.Owner.User.LastName != null && b.Owner.User.LastName.ToLower().Contains(searchTerm)) ||
                        (b.Service != null && b.Service.Title != null && b.Service.Title.ToLower().Contains(searchTerm)) ||
                        (b.Pet != null && b.Pet.Name != null && b.Pet.Name.ToLower().Contains(searchTerm))
                    );
                }

                if (filters.ShowOnlyAvailable == true)
                {
                    // Filter to show only bookings where provider is available
                    var availableBookings = new List<Booking>();
                    foreach (var booking in filteredBookings)
                    {
                        // Check if time slot is available
                        var isAvailableResult = await _bookingService.IsTimeSlotAvailableAsync(
                            booking.ServiceId, 
                            booking.StartTime, 
                            booking.EndTime, 
                            booking.Id, 
                            CancellationToken.None);
                        if (isAvailableResult.IsSuccess && isAvailableResult.Value)
                        {
                            availableBookings.Add(booking);
                        }
                    }
                    filteredBookings = availableBookings.AsQueryable();
                }

                // Separate pending and recent requests
                // Pending requests include both Requested and Pending statuses
                var pendingRequests = filteredBookings
                    .Where(b => b.Status == BookingStatus.Requested || b.Status == BookingStatus.Pending)
                    .OrderBy(b => b.CreatedAt)
                    .ToList();

                var recentRequests = filteredBookings
                    .Where(b => b.Status != BookingStatus.Requested && b.Status != BookingStatus.Pending && b.CreatedAt >= DateTime.UtcNow.AddDays(-30))
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
                    // Check if time slot is available
                    var isAvailableResult = await _bookingService.IsTimeSlotAvailableAsync(
                        booking.ServiceId, 
                        booking.StartTime, 
                        booking.EndTime, 
                        booking.Id, 
                        CancellationToken.None);
                    var isAvailable = isAvailableResult.IsSuccess && isAvailableResult.Value;
                    
                    var duration = booking.EndTime - booking.StartTime;
                    var durationText = duration.TotalHours >= 1 
                        ? $"{duration.TotalHours:F1} hours" 
                        : $"{duration.TotalMinutes} minutes";

                    viewModel.PendingRequests.Add(new WebModels.BookingRequestItem
                    {
                        Id = booking.Id,
                        PetOwnerName = $"{booking.Owner?.User?.FirstName} {booking.Owner?.User?.LastName}".Trim(),
                        PetOwnerEmail = booking.Owner?.User?.Email ?? string.Empty,
                        PetOwnerPhone = booking.Owner?.User?.PhoneNumber ?? string.Empty,
                        ServiceName = booking.Service?.Title ?? "",
                        PetName = booking.Pet?.Name ?? "",
                        PetType = booking.Pet?.Type.ToString() ?? "",
                        PetBreed = booking.Pet?.Breed ?? "",
                        RequestDate = booking.CreatedAt,
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime,
                        Duration = durationText,
                        TotalPrice = booking.TotalPrice,
                        Status = booking.Status.ToString(),
                        SpecialInstructions = booking.SpecialInstructions ?? "",
                        Notes = booking.Notes ?? "",
                        IsAvailable = isAvailable,
                        AvailabilityMessage = isAvailable ? "Available" : "Not available",
                        PetOwnerAddress = $"{booking.Owner?.User?.Address}, {booking.Owner?.User?.City}, {booking.Owner?.User?.Province}".Trim(',').Trim(),
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
                        PetOwnerName = $"{booking.Owner?.User?.FirstName} {booking.Owner?.User?.LastName}".Trim(),
                        PetOwnerEmail = booking.Owner?.User?.Email ?? "",
                        PetOwnerPhone = booking.Owner?.User?.PhoneNumber ?? "",
                        ServiceName = booking.Service?.Title ?? "",
                        PetName = booking.Pet?.Name ?? "",
                        PetType = booking.Pet?.Type.ToString() ?? "",
                        PetBreed = booking.Pet?.Breed ?? "",
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
                        PetOwnerAddress = $"{booking.Owner?.User?.Address}, {booking.Owner?.User?.City}, {booking.Owner?.User?.Province}".Trim(',').Trim(),
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
                var bookingResult = await _bookingService.GetBookingByIdAsync(bookingId);
                if (bookingResult.IsFailure || bookingResult.Value == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                var booking = bookingResult.Value;

                // Check if provider is available for this booking
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return Json(new { success = false, message = "Service provider not found" });
                }
                
                // Check availability using the booking's service and time
                var availabilityResult = await _bookingService.IsTimeSlotAvailableAsync(booking.ServiceId, booking.StartTime, booking.EndTime, excludeBookingId: booking.Id);
                if (availabilityResult.IsFailure || !availabilityResult.Value)
                {
                    return Json(new { success = false, message = "You are not available for this booking time" });
                }

                // Update booking status to Confirmed
                await _bookingService.UpdateBookingStatusAsync(booking.Id, BookingStatus.Confirmed, CancellationToken.None);

                // Send email notification to pet owner
                try
                {
                    // Note: Email sending would require getting owner's email from a different source
                    // since BookingResponse doesn't have navigation properties
                    var emailSubject = $"Booking Confirmed - {booking.ServiceName}";
                    var emailBody = $@"
                        <h2>Booking Confirmed!</h2>
                        <p>Great news! Your booking request has been confirmed by the service provider.</p>
                        <br>
                        <p><strong>Service:</strong> {booking.ServiceName}</p>
                        <p><strong>Pet:</strong> {booking.PetName ?? "N/A"}</p>
                        <p><strong>Date & Time:</strong> {booking.StartTime:MMM dd, yyyy} at {booking.StartTime:hh:mm tt} - {booking.EndTime:hh:mm tt}</p>
                        <p><strong>Total Price:</strong> ${booking.TotalPrice:F2}</p>
                        <p><strong>Special Instructions:</strong> {booking.SpecialInstructions ?? "None"}</p>
                        <br>
                        <p><strong>Payment:</strong> Please arrange payment directly with the service provider.</p>
                        <p><strong>Contact:</strong> You can reach the service provider through their contact information.</p>
                        <br>
                        <p>Thank you for using PetCare Platform!</p>
                    ";
                    
                    // Email sending would need owner's email from another source
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the booking acceptance
                    Console.WriteLine($"Failed to send confirmation email: {emailEx.Message}");
                }

                return Json(new { success = true, message = "Booking accepted successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.AcceptBooking: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while accepting the booking" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> BookingDetails(int id)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return NotFound("User not authenticated.");
                }

                // Get service provider
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return NotFound("Service provider not found.");
                }
                var serviceProvider = serviceProviderResult.Value;

                // Get booking using repository to get full entity with navigation properties
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                {
                    return NotFound("Booking not found.");
                }

                // Verify booking belongs to this provider
                if (booking.Service?.ProviderId != serviceProvider.Id)
                {
                    return NotFound("Booking not found or doesn't belong to you.");
                }

                return PartialView("_BookingDetailsPartial", booking);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.BookingDetails: {ex.Message}");
                return NotFound("An error occurred while loading booking details.");
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
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }
                var serviceProvider = serviceProviderResult.Value;

                var reviewsResult = await _serviceProviderService.GetProviderReviewsAsync(serviceProvider.Id);
                var reviews = reviewsResult.IsSuccess && reviewsResult.Value != null ? reviewsResult.Value : Enumerable.Empty<PetCarePlatform.Core.DTOs.Responses.ReviewResponse>();
                var reviewViewModels = reviews.Select(r => new WebModels.ReviewViewModel
                {
                    Id = r.Id,
                    BookingId = r.BookingId,
                    ServiceName = r.ServiceName ?? string.Empty,
                    ReviewerName = r.ReviewerName ?? string.Empty,
                    Rating = r.Rating,
                    Comment = r.Comment ?? string.Empty,
                    CreatedAt = r.CreatedAt
                }).ToList();

                // Get user information to populate profile fields
                var user = await _userManager.FindByIdAsync(userIdInt.ToString());
                
                // Parse UserName to get FirstName and LastName
                var nameParts = serviceProvider.UserName?.Split(' ', 2) ?? new[] { "", "" };
                var firstName = nameParts.Length > 0 ? nameParts[0] : "";
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                var viewModel = new WebModels.ServiceProviderProfileViewModel
                {
                    FirstName = user?.FirstName ?? firstName,
                    LastName = user?.LastName ?? lastName,
                    Email = serviceProvider.UserEmail ?? user?.Email ?? "",
                    Phone = user?.PhoneNumber ?? "",
                    Address = user?.Address ?? "",
                    City = user?.City ?? "",
                    Province = user?.Province ?? "",
                    PostalCode = user?.PostalCode ?? "",
                    Bio = user?.Bio ?? "",
                    ProfilePicture = user?.ProfilePhotoUrl ?? "/images/default-avatar.png",
                    BusinessName = serviceProvider.BusinessName ?? "",
                    Description = serviceProvider.Description ?? "",
                    TaxInfo = "", // TaxInfo not available in ServiceProviderResponse
                    TaxNumber = "", // TaxNumber not available in ServiceProviderResponse
                    BusinessDescription = serviceProvider.Description ?? "",
                    SpecialNotes = "",
                    AverageRating = serviceProvider.AverageRating,
                    TotalReviews = serviceProvider.TotalReviews,
                    Credentials = serviceProvider.Credentials ?? "",
                    Certifications = serviceProvider.Certifications ?? "",
                    InsuranceInfo = serviceProvider.InsuranceInfo ?? "",
                    LicenseInfo = serviceProvider.LicenseInfo ?? "",
                    BackgroundCheckVerified = serviceProvider.BackgroundCheckVerified,
                    BackgroundCheckDate = null, // BackgroundCheckDate not available in ServiceProviderResponse
                    IdentityVerified = serviceProvider.IdentityVerified,
                    ServiceArea = serviceProvider.ServiceArea ?? "",
                    ServiceRadius = serviceProvider.ServiceRadius,
                    BankingInfo = "", // Not available in ServiceProviderResponse
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

        [HttpPost]
        [Authorize(Roles = "ServiceProvider")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                if (profilePicture == null || profilePicture.Length == 0)
                {
                    return Json(new { success = false, message = "No file selected" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(profilePicture.ContentType))
                {
                    return Json(new { success = false, message = "Invalid file type. Please upload an image (JPEG, PNG, GIF, or WebP)" });
                }

                // Validate file size (max 5MB)
                if (profilePicture.Length > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "File size exceeds 5MB limit" });
                }

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileName = $"{userIdInt}_{Guid.NewGuid()}{Path.GetExtension(profilePicture.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                // Update user's profile photo URL
                var user = await _userManager.FindByIdAsync(userIdInt.ToString());
                if (user != null)
                {
                    // Delete old profile picture if it exists and is not the default
                    if (!string.IsNullOrEmpty(user.ProfilePhotoUrl) && 
                        !user.ProfilePhotoUrl.StartsWith("/images/default") &&
                        user.ProfilePhotoUrl.StartsWith("/uploads/profiles/"))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePhotoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                            catch
                            {
                                // Ignore errors when deleting old file
                            }
                        }
                    }

                    user.ProfilePhotoUrl = $"/uploads/profiles/{fileName}";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                return Json(new { success = true, message = "Profile picture uploaded successfully", url = $"/uploads/profiles/{fileName}" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.UploadProfilePicture: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                var errorMessage = "An error occurred while uploading the profile picture. Please try again.";
                #if DEBUG
                errorMessage += $" Error: {ex.Message}";
                #endif
                
                return Json(new { success = false, message = errorMessage });
            }
        }

        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Settings()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get service provider profile
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }
                var serviceProvider = serviceProviderResult.Value;
                
                // Get user information separately since ServiceProviderResponse doesn't have User property
                var user = await _userManager.FindByIdAsync(serviceProvider.UserId.ToString());
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var viewModel = new WebModels.ServiceProviderSettingsViewModel
                {
                    // Account Settings
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    
                    // Business Settings
                    BusinessName = serviceProvider.BusinessName ?? "",
                    BusinessType = serviceProvider.BusinessType ?? "",
                    BusinessNumber = serviceProvider.BusinessNumber ?? "",
                    Description = serviceProvider.Description ?? "",
                    
                    // Location Settings
                    Address = user.Address ?? "",
                    City = user.City ?? "",
                    Province = user.Province ?? "",
                    PostalCode = user.PostalCode ?? "",
                    ServiceArea = serviceProvider.ServiceArea ?? "",
                    ServiceRadius = serviceProvider.ServiceRadius,
                    
                    // Professional Settings
                    Credentials = serviceProvider.Credentials ?? "",
                    Certifications = serviceProvider.Certifications ?? "",
                    InsuranceInfo = serviceProvider.InsuranceInfo ?? "",
                    LicenseInfo = serviceProvider.LicenseInfo ?? "",
                    
                    // Financial Settings - These properties don't exist in ServiceProviderResponse
                    BankingInfo = "", // Not available in ServiceProviderResponse
                    TaxInfo = "", // Not available in ServiceProviderResponse
                    
                    // Notification Settings
                    EmailNotifications = true, // Default to true
                    SMSNotifications = false, // Default to false
                    BookingReminders = true,
                    PaymentNotifications = true,
                    ReviewNotifications = true,
                    
                    // Privacy Settings
                    ProfileVisibility = "Public", // Public, Private, Limited
                    ShowContactInfo = true,
                    ShowLocation = true,
                    ShowReviews = true,
                    
                    // Availability Settings
                    AutoAcceptBookings = false,
                    RequireApproval = true,
                    MaxAdvanceBookingDays = 30,
                    MinAdvanceBookingHours = 2
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Settings: {ex.Message}");
                TempData["Error"] = "An error occurred while loading settings. Please try again.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Settings(WebModels.ServiceProviderSettingsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userIdInt))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get user
                var user = await _userManager.FindByIdAsync(userIdInt.ToString());
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Update user information
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.City = model.City;
                user.Province = model.Province;
                user.PostalCode = model.PostalCode;
                user.UpdatedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                // Get service provider profile
                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userIdInt);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return RedirectToAction("Create", "ServiceProvider");
                }
                var serviceProvider = serviceProviderResult.Value;

                // Update service provider information
                var updateRequest = new PetCarePlatform.Core.DTOs.Requests.UpdateServiceProviderRequest
                {
                    ProviderId = serviceProvider.Id,
                    BusinessName = model.BusinessName ?? serviceProvider.BusinessName,
                    Description = model.Description ?? serviceProvider.Description,
                    Credentials = model.Credentials ?? serviceProvider.Credentials,
                    Certifications = model.Certifications ?? serviceProvider.Certifications,
                    InsuranceInfo = model.InsuranceInfo ?? serviceProvider.InsuranceInfo,
                    LicenseInfo = model.LicenseInfo ?? serviceProvider.LicenseInfo,
                    ServiceArea = model.ServiceArea ?? serviceProvider.ServiceArea,
                    ServiceRadius = model.ServiceRadius != 0 ? model.ServiceRadius : (int?)serviceProvider.ServiceRadius
                };

                var updateResult = await _serviceProviderService.UpdateServiceProviderProfileAsync(updateRequest);
                if (updateResult.IsFailure)
                {
                    ModelState.AddModelError("", updateResult.ErrorMessage ?? "Failed to update settings.");
                    return View(model);
                }

                TempData["Success"] = "Settings updated successfully!";
                return RedirectToAction("Settings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Settings POST: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating settings. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetService(int id)
        {
            try
            {
                var serviceResult = await _serviceService.GetServiceByIdAsync(id);
                if (serviceResult.IsFailure || serviceResult.Value == null)
                {
                    return Json(new { success = false, message = "Service not found." });
                }

                var service = serviceResult.Value;

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

        // Analytics Actions
        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Analytics()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userId);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    TempData["Error"] = "Service provider profile not found.";
                    return RedirectToAction("Dashboard");
                }
                var serviceProvider = serviceProviderResult.Value;

                // Use enterprise pattern methods
                var breakdownResult = await _reviewService.GetRatingBreakdownAsync(serviceProvider.Id, CancellationToken.None);
                var trendsResult = await _reviewService.GetRatingTrendsAsync(serviceProvider.Id, 30, CancellationToken.None);
                var serviceRatingsResult = await _reviewService.GetServiceRatingsAsync(serviceProvider.Id, CancellationToken.None);
                var metricsResult = await _reviewService.GetPerformanceMetricsAsync(serviceProvider.Id, CancellationToken.None);
                var recentReviewsResult = await _reviewService.GetRecentReviewsAsync(serviceProvider.Id, 5, CancellationToken.None);

                // Map to DTOs for ViewModels (maintaining backward compatibility)
                var breakdownDto = breakdownResult.IsSuccess ? new PetCarePlatform.Core.Models.RatingBreakdownDto
                {
                    FiveStarCount = breakdownResult.Value!.FiveStarCount,
                    FourStarCount = breakdownResult.Value.FourStarCount,
                    ThreeStarCount = breakdownResult.Value.ThreeStarCount,
                    TwoStarCount = breakdownResult.Value.TwoStarCount,
                    OneStarCount = breakdownResult.Value.OneStarCount
                } : new PetCarePlatform.Core.Models.RatingBreakdownDto();

                var trendsDto = trendsResult.IsSuccess ? trendsResult.Value!.Select(t => new PetCarePlatform.Core.Models.RatingTrendDto
                {
                    Date = t.Date,
                    AverageRating = t.AverageRating,
                    ReviewCount = t.ReviewCount,
                    Period = t.Period
                }) : new List<PetCarePlatform.Core.Models.RatingTrendDto>();

                var serviceRatingsDto = serviceRatingsResult.IsSuccess ? serviceRatingsResult.Value!.Select(sr => new PetCarePlatform.Core.Models.ServiceRatingDto
                {
                    ServiceId = sr.ServiceId,
                    ServiceName = sr.ServiceName,
                    AverageRating = sr.AverageRating,
                    ReviewCount = sr.ReviewCount,
                    TotalBookings = sr.TotalBookings
                }) : new List<PetCarePlatform.Core.Models.ServiceRatingDto>();

                var metricsDto = metricsResult.IsSuccess ? new PetCarePlatform.Core.Models.ReviewPerformanceMetricsDto
                {
                    ResponseRate = metricsResult.Value!.ResponseRate,
                    AverageResponseTime = metricsResult.Value.AverageResponseTime,
                    TotalResponses = metricsResult.Value.TotalResponses,
                    PendingResponses = metricsResult.Value.PendingResponses,
                    RatingImprovement = metricsResult.Value.RatingImprovement,
                    ReviewsThisMonth = metricsResult.Value.ReviewsThisMonth,
                    ReviewsLastMonth = metricsResult.Value.ReviewsLastMonth
                } : new PetCarePlatform.Core.Models.ReviewPerformanceMetricsDto();

                var recentReviewsDto = recentReviewsResult.IsSuccess ? recentReviewsResult.Value!.Select(rr => new PetCarePlatform.Core.Models.RecentReviewDto
                {
                    Id = rr.Id,
                    ReviewerName = rr.ReviewerName,
                    ServiceName = rr.ServiceName,
                    Rating = rr.Rating,
                    Comment = rr.Comment,
                    Response = rr.Response,
                    CreatedAt = rr.CreatedAt,
                    ResponseDate = rr.ResponseDate
                }) : new List<PetCarePlatform.Core.Models.RecentReviewDto>();

                var analytics = new WebModels.RatingAnalyticsViewModel
                {
                    ProviderId = serviceProvider.Id,
                    ProviderName = serviceProvider.BusinessName,
                    AverageRating = serviceProvider.AverageRating,
                    TotalReviews = serviceProvider.TotalReviews,
                    RatingBreakdown = WebModels.RatingBreakdownViewModel.FromDto(breakdownDto),
                    RatingTrends = trendsDto.Select(WebModels.RatingTrendViewModel.FromDto).ToList(),
                    ServiceRatings = serviceRatingsDto.Select(WebModels.ServiceRatingViewModel.FromDto).ToList(),
                    PerformanceMetrics = WebModels.ReviewPerformanceMetrics.FromDto(metricsDto),
                    RecentReviews = recentReviewsDto.Select(WebModels.RecentReviewViewModel.FromDto).ToList()
                };

                return View(analytics);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.Analytics: {ex.Message}");
                TempData["Error"] = "An error occurred while loading analytics.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> GetAnalyticsData(string type = "overview")
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var serviceProviderResult = await _serviceProviderService.GetServiceProviderByUserIdAsync(userId);
                if (serviceProviderResult.IsFailure || serviceProviderResult.Value == null)
                {
                    return Json(new { success = false, message = "Service provider not found." });
                }
                var serviceProvider = serviceProviderResult.Value;

                switch (type.ToLower())
                {
                    case "rating-breakdown":
                        var breakdownResult = await _reviewService.GetRatingBreakdownAsync(serviceProvider.Id, CancellationToken.None);
                        if (breakdownResult.IsFailure)
                        {
                            return Json(new { success = false, message = breakdownResult.ErrorMessage });
                        }
                        var breakdownDto = new PetCarePlatform.Core.Models.RatingBreakdownDto
                        {
                            FiveStarCount = breakdownResult.Value!.FiveStarCount,
                            FourStarCount = breakdownResult.Value.FourStarCount,
                            ThreeStarCount = breakdownResult.Value.ThreeStarCount,
                            TwoStarCount = breakdownResult.Value.TwoStarCount,
                            OneStarCount = breakdownResult.Value.OneStarCount
                        };
                        return Json(new { success = true, data = WebModels.RatingBreakdownViewModel.FromDto(breakdownDto) });

                    case "rating-trends":
                        var trendsResult = await _reviewService.GetRatingTrendsAsync(serviceProvider.Id, 30, CancellationToken.None);
                        if (trendsResult.IsFailure)
                        {
                            return Json(new { success = false, message = trendsResult.ErrorMessage });
                        }
                        var trendsDto = trendsResult.Value!.Select(t => new PetCarePlatform.Core.Models.RatingTrendDto
                        {
                            Date = t.Date,
                            AverageRating = t.AverageRating,
                            ReviewCount = t.ReviewCount,
                            Period = t.Period
                        });
                        return Json(new { success = true, data = trendsDto.Select(WebModels.RatingTrendViewModel.FromDto) });

                    case "service-ratings":
                        var serviceRatingsResult = await _reviewService.GetServiceRatingsAsync(serviceProvider.Id, CancellationToken.None);
                        if (serviceRatingsResult.IsFailure)
                        {
                            return Json(new { success = false, message = serviceRatingsResult.ErrorMessage });
                        }
                        var serviceRatingsDto = serviceRatingsResult.Value!.Select(sr => new PetCarePlatform.Core.Models.ServiceRatingDto
                        {
                            ServiceId = sr.ServiceId,
                            ServiceName = sr.ServiceName,
                            AverageRating = sr.AverageRating,
                            ReviewCount = sr.ReviewCount,
                            TotalBookings = sr.TotalBookings
                        });
                        return Json(new { success = true, data = serviceRatingsDto.Select(WebModels.ServiceRatingViewModel.FromDto) });

                    case "performance-metrics":
                        var metricsResult = await _reviewService.GetPerformanceMetricsAsync(serviceProvider.Id, CancellationToken.None);
                        if (metricsResult.IsFailure)
                        {
                            return Json(new { success = false, message = metricsResult.ErrorMessage });
                        }
                        var metricsDto = new PetCarePlatform.Core.Models.ReviewPerformanceMetricsDto
                        {
                            ResponseRate = metricsResult.Value!.ResponseRate,
                            AverageResponseTime = metricsResult.Value.AverageResponseTime,
                            TotalResponses = metricsResult.Value.TotalResponses,
                            PendingResponses = metricsResult.Value.PendingResponses,
                            RatingImprovement = metricsResult.Value.RatingImprovement,
                            ReviewsThisMonth = metricsResult.Value.ReviewsThisMonth,
                            ReviewsLastMonth = metricsResult.Value.ReviewsLastMonth
                        };
                        return Json(new { success = true, data = WebModels.ReviewPerformanceMetrics.FromDto(metricsDto) });

                    case "recent-reviews":
                        var recentReviewsResult = await _reviewService.GetRecentReviewsAsync(serviceProvider.Id, 10, CancellationToken.None);
                        if (recentReviewsResult.IsFailure)
                        {
                            return Json(new { success = false, message = recentReviewsResult.ErrorMessage });
                        }
                        // Map to DTO for ViewModel
                        var recentReviewsDto = recentReviewsResult.Value!.Select(rr => new PetCarePlatform.Core.Models.RecentReviewDto
                        {
                            Id = rr.Id,
                            ReviewerName = rr.ReviewerName,
                            ServiceName = rr.ServiceName,
                            Rating = rr.Rating,
                            Comment = rr.Comment,
                            Response = rr.Response,
                            CreatedAt = rr.CreatedAt,
                            ResponseDate = rr.ResponseDate
                        });
                        return Json(new { success = true, data = recentReviewsDto.Select(WebModels.RecentReviewViewModel.FromDto) });

                    default:
                        return Json(new { success = false, message = "Invalid analytics type." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ServiceProviderController.GetAnalyticsData: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while retrieving analytics data." });
            }
        }
    }
}

