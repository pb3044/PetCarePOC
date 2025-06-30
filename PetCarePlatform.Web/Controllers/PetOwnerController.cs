using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Web.ViewModels;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using PetCarePlatform.Web.Models;

namespace PetCarePlatform.Web.Controllers
{
    public class PetOwnerController : Controller
    {
        private readonly IPetOwnerService _petOwnerService;
        private readonly IUserService _userService;
        private readonly IPetRepository _petRepository;
        private readonly IMapper mapper;
        private readonly IBookingService _bookingService;
        private readonly IServiceRepository _serviceRepository;
        private readonly IServiceService _serviceService;

        public PetOwnerController(
            IPetOwnerService petOwnerService, 
            IUserService userService, 
            IPetRepository petRepository, 
            IMapper mapper, 
            IBookingService bookingService, 
            IServiceRepository serviceRepository,
            IServiceService serviceService)
        {
            _petOwnerService = petOwnerService;
            _userService = userService;
            _petRepository = petRepository;
            this.mapper = mapper;
            _bookingService = bookingService;
            _serviceRepository = serviceRepository;
            _serviceService = serviceService;
        }

        [Authorize(Roles = "PetOwner")]      
        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userService.GetUserByIdAsync(userId);
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
            {
                return NotFound("Pet owner not found.");
            }

            var viewModel = new PetOwnerDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Pets = petOwner.Pets?.ToList() ?? new List<Pet>(),
                RecentBookings = petOwner.Bookings?
                    .OrderByDescending(b => b.StartTime)
                    .Take(5)
                    .ToList() ?? new List<Booking>(),
                FavoriteProviders = petOwner.FavoriteProviders?.ToList() ?? new List<Core.Models.ServiceProvider>()
            };

            return View(viewModel);
        }

        // You can add other actions for pet owners here
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> MyPets()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
            {
                return NotFound("Pet owner not found.");
            }
            var pets = petOwner.Pets?.ToList() ?? new List<Pet>();
            return View(pets);
        }

        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
            {
                return NotFound("Pet owner not found.");
            }

            // Get bookings for this pet owner
            var bookings = await _bookingService.GetBookingsByOwnerIdAsync(petOwner.Id);
            
            return View(bookings);
        }

        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
            {
                return NotFound("Pet owner not found.");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Map to ViewModel
            var viewModel = new PetOwnerProfileViewModel
            {
                Id = petOwner.Id,
                UserId = petOwner.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Province = user.Province,
                PostalCode = user.PostalCode,
                Bio = user.Bio,
                ReceiveNotifications = petOwner.ReceiveNotifications,
                ReceiveMarketingEmails = petOwner.ReceiveMarketingEmails
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(PetOwnerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwner == null)
                {
                    TempData["Error"] = "Pet owner not found";
                    return RedirectToAction("Dashboard");
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Dashboard");
                }

                // Update User information
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.City = model.City;
                user.Province = model.Province;
                user.PostalCode = model.PostalCode;
                user.Bio = model.Bio;
                user.UpdatedAt = DateTime.UtcNow;

                // Update PetOwner preferences
                petOwner.ReceiveNotifications = model.ReceiveNotifications;
                petOwner.ReceiveMarketingEmails = model.ReceiveMarketingEmails;
                petOwner.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _userService.UpdateUserProfileAsync(user);
                await _petOwnerService.UpdatePetOwnerProfileAsync(petOwner);

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating profile: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> PetDetails(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
            {
                return NotFound("Pet owner not found.");
            }
            var pet = petOwner.Pets?.FirstOrDefault(p => p.Id == id);
            if (pet == null)
            {
                return NotFound("Pet not found.");
            }
            return View(pet);
        }

        [HttpGet]
        public async Task<IActionResult> EditPet(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
            {
                return NotFound("Pet owner not found.");
            }
            var pet = petOwner.Pets?.FirstOrDefault(p => p.Id == id);
            if (pet == null)
            {
                return NotFound("Pet not found.");
            }
            // Map Pet to EditPetViewModel
            var vm = new EditPetViewModel
            {
                Id = pet.Id,
                OwnerId = pet.OwnerId,
                Name = pet.Name,
                Type = pet.Type,
                Breed = pet.Breed,
                Age = pet.Age,
                Size = pet.Size,
                Gender = pet.Gender,
                IsNeutered = pet.IsNeutered,
                MedicalInformation = pet.MedicalInformation,
                SpecialNeeds = pet.SpecialNeeds,
                Temperament = pet.Temperament,
                FeedingInstructions = pet.FeedingInstructions,
                ExerciseNeeds = pet.ExerciseNeeds,
                BehavioralNotes = pet.BehavioralNotes,
                EmergencyContactName = pet.EmergencyContactName,
                EmergencyContactPhone = pet.EmergencyContactPhone,
                VeterinarianName = pet.VeterinarianName,
                VeterinarianPhone = pet.VeterinarianPhone
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPet(EditPetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
            {
                TempData["Error"] = "Pet owner not found";
                return RedirectToAction("MyPets");
            }
            var existingPet = petOwner.Pets?.FirstOrDefault(p => p.Id == model.Id);
            if (existingPet == null)
            {
                TempData["Error"] = "Pet not found or doesn't belong to you";
                return RedirectToAction("MyPets");
            }
            // Map ViewModel to Pet
            existingPet.Name = model.Name ?? string.Empty;
            existingPet.Type = model.Type;
            existingPet.Breed = model.Breed ?? string.Empty;
            existingPet.Age = model.Age;
            existingPet.Size = model.Size ?? string.Empty;
            existingPet.Gender = model.Gender ?? string.Empty;
            existingPet.IsNeutered = model.IsNeutered;
            existingPet.MedicalInformation = model.MedicalInformation ?? string.Empty;
            existingPet.SpecialNeeds = model.SpecialNeeds ?? string.Empty;
            existingPet.Temperament = model.Temperament ?? string.Empty;
            existingPet.FeedingInstructions = model.FeedingInstructions ?? string.Empty;
            existingPet.ExerciseNeeds = model.ExerciseNeeds ?? string.Empty;
            existingPet.BehavioralNotes = model.BehavioralNotes ?? string.Empty;
            existingPet.EmergencyContactName = model.EmergencyContactName ?? string.Empty;
            existingPet.EmergencyContactPhone = model.EmergencyContactPhone ?? string.Empty;
            existingPet.VeterinarianName = model.VeterinarianName ?? string.Empty;
            existingPet.VeterinarianPhone = model.VeterinarianPhone ?? string.Empty;
            await _petOwnerService.UpdatePetAsync(existingPet);
            TempData["Success"] = "Pet updated successfully!";
            return RedirectToAction("PetDetails", new { id = model.Id });
        }

        [HttpGet]
        public IActionResult AddPet()
        {
            return View(new AddPetViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPet(AddPetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwner == null)
                {
                    TempData["Error"] = "Pet owner not found";
                    return RedirectToAction("MyPets");
                }

                // Create new Pet from ViewModel
                var newPet = new Pet
                {
                    OwnerId = petOwner.Id,
                    Name = model.Name ?? string.Empty,
                    Type = model.Type,
                    Breed = model.Breed ?? string.Empty,
                    Age = model.Age,
                    Size = model.Size ?? string.Empty,
                    Gender = model.Gender ?? string.Empty,
                    IsNeutered = model.IsNeutered,
                    MedicalInformation = model.MedicalInformation ?? string.Empty,
                    SpecialNeeds = model.SpecialNeeds ?? string.Empty,
                    Temperament = model.Temperament ?? string.Empty,
                    FeedingInstructions = model.FeedingInstructions ?? string.Empty,
                    ExerciseNeeds = model.ExerciseNeeds ?? string.Empty,
                    BehavioralNotes = model.BehavioralNotes ?? string.Empty,
                    EmergencyContactName = model.EmergencyContactName ?? string.Empty,
                    EmergencyContactPhone = model.EmergencyContactPhone ?? string.Empty,
                    VeterinarianName = model.VeterinarianName ?? string.Empty,
                    VeterinarianPhone = model.VeterinarianPhone ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _petRepository.CreateAsync(newPet);
                TempData["Success"] = "Pet added successfully!";
                return RedirectToAction("MyPets");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding pet: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> BookService(int? serviceId = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwner == null)
                {
                    return NotFound("Pet owner not found.");
                }

                // Get user's pets
                var userPets = petOwner.Pets?.ToList() ?? new List<Pet>();
                if (!userPets.Any())
                {
                    TempData["Error"] = "You need to add pets before booking services. Please add a pet first.";
                    return RedirectToAction("AddPet");
                }

                // Get all available services
                var availableServices = await _serviceService.GetAllServicesAsync();
                
                if (!availableServices.Any())
                {
                    TempData["Warning"] = "No services are currently available. Please check back later.";
                }

                // If a specific service is selected, populate the form
                if (serviceId.HasValue)
                {
                    var selectedService = availableServices.FirstOrDefault(s => s.Id == serviceId.Value);
                    if (selectedService == null)
                    {
                        TempData["Error"] = "Selected service not found.";
                        return RedirectToAction("BookService");
                    }

                    var viewModel = new BookServiceViewModel
                    {
                        ServiceId = serviceId.Value,
                        ServiceTitle = selectedService.Title,
                        ServiceDescription = selectedService.Description,
                        ServicePrice = selectedService.BasePrice,
                        ServicePriceUnit = selectedService.PriceUnit,
                        ProviderName = selectedService.Provider?.BusinessName ?? "Unknown Provider",
                        ServiceLocation = selectedService.Location,
                        UserPets = userPets,
                        AvailableServices = availableServices,
                        BookingDate = DateTime.Today.AddDays(1), // Default to tomorrow
                        StartTime = "09:00", // Default to 9 AM
                        EndTime = "10:00" // Default to 10 AM
                    };
                    return View(viewModel);
                }

                // Show the service selection page
                return View("BookService", new BookServiceViewModel
                {
                    UserPets = userPets,
                    AvailableServices = availableServices,
                    BookingDate = DateTime.Today.AddDays(1),
                    StartTime = "09:00",
                    EndTime = "10:00"
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading booking page: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookService(BookServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate the pets dropdown and services
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                model.UserPets = petOwner.Pets?.ToList() ?? new List<Pet>();
                model.AvailableServices = await _serviceService.GetAllServicesAsync();
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwner == null)
                {
                    TempData["Error"] = "Pet owner not found";
                    return RedirectToAction("Dashboard");
                }

                // Validate that the pet belongs to the user
                var selectedPet = petOwner.Pets?.FirstOrDefault(p => p.Id == model.PetId);
                if (selectedPet == null)
                {
                    TempData["Error"] = "Selected pet not found or doesn't belong to you";
                    return RedirectToAction("BookService");
                }

                // Validate service exists and is active
                var service = await _serviceService.GetServiceByIdAsync(model.ServiceId);
                if (service == null || !service.IsActive)
                {
                    TempData["Error"] = "Selected service is not available";
                    return RedirectToAction("BookService");
                }

                // Parse time strings to TimeSpan
                if (!TimeSpan.TryParse(model.StartTime, out var startTimeSpan))
                {
                    TempData["Error"] = "Invalid start time format";
                    return RedirectToAction("BookService");
                }

                if (!TimeSpan.TryParse(model.EndTime, out var endTimeSpan))
                {
                    TempData["Error"] = "Invalid end time format";
                    return RedirectToAction("BookService");
                }

                // Validate booking date is in the future
                var bookingDateTime = model.BookingDate.Date.Add(startTimeSpan);
                if (bookingDateTime <= DateTime.Now)
                {
                    TempData["Error"] = "Booking date and time must be in the future";
                    return RedirectToAction("BookService");
                }

                // Validate end time is after start time
                var endDateTime = model.BookingDate.Date.Add(endTimeSpan);
                if (endDateTime <= bookingDateTime)
                {
                    TempData["Error"] = "End time must be after start time";
                    return RedirectToAction("BookService");
                }

                // Check if time slot is available
                if (!await _bookingService.IsTimeSlotAvailableAsync(model.ServiceId, bookingDateTime, endDateTime))
                {
                    TempData["Error"] = "The selected time slot is not available. Please choose a different time.";
                    return RedirectToAction("BookService");
                }

                // Create the booking
                var booking = new Booking
                {
                    OwnerId = petOwner.Id,
                    PetId = model.PetId,
                    ServiceId = model.ServiceId,
                    StartTime = bookingDateTime,
                    EndTime = endDateTime,
                    SpecialInstructions = model.SpecialInstructions,
                    Status = BookingStatus.Requested,
                    TotalPrice = service.BasePrice, // Use the service's base price
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Save the booking
                await _bookingService.CreateBookingAsync(booking);

                TempData["Success"] = "Booking request submitted successfully! The service provider will review and confirm your booking.";
                return RedirectToAction("MyBookings");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating booking: {ex.Message}";
                
                // Repopulate the pets dropdown and services
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                model.UserPets = petOwner.Pets?.ToList() ?? new List<Pet>();
                model.AvailableServices = await _serviceService.GetAllServicesAsync();
                
                return View(model);
            }
        }

        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> BookingDetails(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
                return NotFound("Pet owner not found.");

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null || booking.OwnerId != petOwner.Id)
                return NotFound("Booking not found.");

            return PartialView("_BookingDetailsPartial", booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwner == null)
                {
                    return Json(new { success = false, message = "Pet owner not found." });
                }

                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null || booking.OwnerId != petOwner.Id)
                {
                    return Json(new { success = false, message = "Booking not found or doesn't belong to you." });
                }

                // Check if booking can be cancelled
                if (booking.Status == BookingStatus.Cancelled)
                {
                    return Json(new { success = false, message = "Booking is already cancelled." });
                }

                if (booking.Status == BookingStatus.Completed)
                {
                    return Json(new { success = false, message = "Cannot cancel a completed booking." });
                }

                if (booking.StartTime <= DateTime.Now)
                {
                    return Json(new { success = false, message = "Cannot cancel a booking that has already started." });
                }

                // Cancel the booking
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingService.UpdateBookingAsync(booking);

                return Json(new { success = true, message = "Booking cancelled successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error cancelling booking: {ex.Message}" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> EditBooking(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
            {
                TempData["Error"] = "Pet owner not found.";
                return RedirectToAction("MyBookings");
            }

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null || booking.OwnerId != petOwner.Id)
            {
                TempData["Error"] = "Booking not found or doesn't belong to you.";
                return RedirectToAction("MyBookings");
            }

            // Check if booking can be edited
            if (booking.Status == BookingStatus.Cancelled)
            {
                TempData["Error"] = "Cannot edit a cancelled booking.";
                return RedirectToAction("MyBookings");
            }

            if (booking.Status == BookingStatus.Completed)
            {
                TempData["Error"] = "Cannot edit a completed booking.";
                return RedirectToAction("MyBookings");
            }

            if (booking.StartTime <= DateTime.Now)
            {
                TempData["Error"] = "Cannot edit a booking that has already started.";
                return RedirectToAction("MyBookings");
            }

            var viewModel = new EditBookingViewModel
            {
                Id = booking.Id,
                BookingDate = booking.StartTime.Date,
                StartTime = booking.StartTime.ToString("HH:mm"),
                EndTime = booking.EndTime.ToString("HH:mm"),
                SpecialInstructions = booking.SpecialInstructions,
                ServiceTitle = booking.Service?.Title,
                ProviderName = booking.Service?.Provider?.BusinessName,
                PetName = booking.Pet?.Name,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> EditBooking(EditBookingViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    // Repopulate display-only fields for redisplay
            //    var booking = await _bookingService.GetBookingByIdAsync(model.Id);
            //    if (booking != null)
            //    {
            //        model.ServiceTitle = booking.Service?.Title;
            //        model.ProviderName = booking.Service?.Provider?.BusinessName;
            //        model.PetName = booking.Pet?.Name;
            //        model.TotalPrice = booking.TotalPrice;
            //        model.Status = booking.Status;
            //    }
            //    return View(model);
            //}

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwner == null)
                {
                    TempData["Error"] = "Pet owner not found.";
                    return RedirectToAction("MyBookings");
                }

                var booking = await _bookingService.GetBookingByIdAsync(model.Id);
                if (booking == null || booking.OwnerId != petOwner.Id)
                {
                    TempData["Error"] = "Booking not found or doesn't belong to you.";
                    return RedirectToAction("MyBookings");
                }

                // Check if booking can be edited
                if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                {
                    TempData["Error"] = "Cannot edit this booking.";
                    return RedirectToAction("MyBookings");
                }

                if (booking.StartTime <= DateTime.Now)
                {
                    TempData["Error"] = "Cannot edit a booking that has already started.";
                    return RedirectToAction("MyBookings");
                }

                // Parse time strings to TimeSpan
                if (!TimeSpan.TryParse(model.StartTime, out var startTimeSpan))
                {
                    ModelState.AddModelError("StartTime", "Invalid start time format");
                    return View(model);
                }

                if (!TimeSpan.TryParse(model.EndTime, out var endTimeSpan))
                {
                    ModelState.AddModelError("EndTime", "Invalid end time format");
                    return View(model);
                }

                // Validate booking date is in the future
                var bookingDateTime = model.BookingDate.Date.Add(startTimeSpan);
                if (bookingDateTime <= DateTime.Now)
                {
                    ModelState.AddModelError("BookingDate", "Booking date and time must be in the future");
                    return View(model);
                }

                // Validate end time is after start time
                var endDateTime = model.BookingDate.Date.Add(endTimeSpan);
                if (endDateTime <= bookingDateTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                    return View(model);
                }

                // Check if new time slot is available (excluding current booking)
                if (!await _bookingService.IsTimeSlotAvailableAsync(booking.ServiceId, bookingDateTime, endDateTime, excludeBookingId: booking.Id))
                {
                    ModelState.AddModelError("", "The selected time slot is not available. Please choose a different time.");
                    return View(model);
                }

                // Update the booking
                booking.StartTime = bookingDateTime;
                booking.EndTime = endDateTime;
                booking.SpecialInstructions = model.SpecialInstructions;
                booking.UpdatedAt = DateTime.UtcNow;
                
                await _bookingService.UpdateBookingAsync(booking);

                TempData["Success"] = "Booking updated successfully!";
                return RedirectToAction("MyBookings");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating booking: {ex.Message}";
                return View(model);
            }
        }
    }
}