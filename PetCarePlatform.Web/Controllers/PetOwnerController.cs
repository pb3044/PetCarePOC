using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.Common;
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
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IServiceService _serviceService;
        private readonly IEmailService _emailService;
        private readonly IServiceProviderService _serviceProviderService;

        public PetOwnerController(
            IPetOwnerService petOwnerService, 
            IUserService userService, 
            IPetRepository petRepository, 
            IMapper mapper, 
            IBookingService bookingService,
            IBookingRepository bookingRepository,
            IServiceRepository serviceRepository,
            IServiceService serviceService,
            IEmailService emailService,
            IServiceProviderService serviceProviderService)
        {
            _petOwnerService = petOwnerService;
            _userService = userService;
            _petRepository = petRepository;
            this.mapper = mapper;
            _bookingService = bookingService;
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _serviceService = serviceService;
            _emailService = emailService;
            _serviceProviderService = serviceProviderService;
        }

        [Authorize(Roles = "PetOwner")]      
        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userResult = await _userService.GetUserByIdAsync(userId);
            if (userResult.IsFailure || userResult.Value == null)
            {
                return NotFound("User not found.");
            }
            var user = userResult.Value;

            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                return NotFound("Pet owner not found.");
            }
            var petOwner = petOwnerResult.Value;

            var pets = (await _petRepository.GetByOwnerIdAsync(petOwner.Id)).ToList();

            var bookingQuery = new BookingQuery { OwnerId = petOwner.Id, PageSize = 10, SortBy = "StartTime", SortOrder = "desc" };
            var bookingsResult = await _bookingService.GetBookingsAsync(bookingQuery);
            var recentBookings = bookingsResult.IsSuccess 
                ? bookingsResult.Value.Items.Select(b => new Booking
                {
                    Id = b.Id,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    Status = b.Status,
                    TotalPrice = b.TotalPrice,
                    CreatedAt = b.CreatedAt
                }).ToList()
                : new List<Booking>();

            var favoriteProviders = new List<Core.Models.ServiceProvider>();

            var viewModel = new PetOwnerDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Pets = pets,
                RecentBookings = recentBookings,
                FavoriteProviders = favoriteProviders
            };

            return View(viewModel);
        }

        // You can add other actions for pet owners here
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> MyPets()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                return NotFound("Pet owner not found.");
            }
            var petOwner = petOwnerResult.Value;
            var pets = (await _petRepository.GetByOwnerIdAsync(petOwner.Id)).ToList();
            return View(pets);
        }

        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                return NotFound("Pet owner not found.");
            }
            var petOwner = petOwnerResult.Value;

            // Get bookings for this pet owner using the repository to get full entities with navigation properties
            var bookings = await _bookingRepository.GetByOwnerIdAsync(petOwner.Id);
            
            return View(bookings);
        }

        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                return NotFound("Pet owner not found.");
            }
            var petOwner = petOwnerResult.Value;

            var userResult = await _userService.GetUserByIdAsync(userId);
            if (userResult.IsFailure || userResult.Value == null)
            {
                return NotFound("User not found.");
            }
            var user = userResult.Value;

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
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    TempData["Error"] = "Pet owner not found";
                    return RedirectToAction("Dashboard");
                }
                var petOwner = petOwnerResult.Value;

                var userResult = await _userService.GetUserByIdAsync(userId);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Dashboard");
                }
                var user = userResult.Value;

                // Create update request DTOs
                var updateUserRequest = new UpdateUserProfileRequest
                {
                    UserId = userId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    Province = model.Province,
                    PostalCode = model.PostalCode,
                    Bio = model.Bio
                };

                var updatePetOwnerRequest = new UpdatePetOwnerRequest
                {
                    PetOwnerId = petOwner.Id,
                    ReceiveNotifications = model.ReceiveNotifications,
                    ReceiveMarketingEmails = model.ReceiveMarketingEmails
                };

                // Save changes
                var updateUserResult = await _userService.UpdateUserProfileAsync(updateUserRequest);
                if (updateUserResult.IsFailure)
                {
                    TempData["Error"] = updateUserResult.ErrorMessage;
                    return View(model);
                }

                var updatePetOwnerResult = await _petOwnerService.UpdatePetOwnerProfileAsync(updatePetOwnerRequest);
                if (updatePetOwnerResult.IsFailure)
                {
                    TempData["Error"] = updatePetOwnerResult.ErrorMessage;
                    return View(model);
                }

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating profile: {ex.Message}";
                return View(model);
            }
        }

        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> PetDetails(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                return NotFound("Pet owner not found.");
            }
            var petOwner = petOwnerResult.Value;
            var pets = await _petRepository.GetByOwnerIdAsync(petOwner.Id);
            var pet = pets.FirstOrDefault(p => p.Id == id);
            if (pet == null)
            {
                return NotFound("Pet not found.");
            }
            return View(pet);
        }

        [HttpGet]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> EditPet(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                return NotFound("Pet owner not found.");
            }
            var petOwner = petOwnerResult.Value;
            var pets = await _petRepository.GetByOwnerIdAsync(petOwner.Id);
            var pet = pets.FirstOrDefault(p => p.Id == id);
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
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                TempData["Error"] = "Pet owner not found";
                return RedirectToAction("MyPets");
            }
            var petOwner = petOwnerResult.Value;
            var pets = await _petRepository.GetByOwnerIdAsync(petOwner.Id);
            var existingPet = pets.FirstOrDefault(p => p.Id == model.Id);
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
            await _petRepository.UpdateAsync(existingPet);
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
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    TempData["Error"] = "Pet owner not found";
                    return RedirectToAction("MyPets");
                }
                var petOwner = petOwnerResult.Value;

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
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    return NotFound("Pet owner not found.");
                }
                var petOwner = petOwnerResult.Value;

                // Get user's pets
                var userPets = (await _petRepository.GetByOwnerIdAsync(petOwner.Id)).ToList();
                if (!userPets.Any())
                {
                    TempData["Error"] = "You need to add pets before booking services. Please add a pet first.";
                    return RedirectToAction("AddPet");
                }

                // Get all available services using ServiceQuery
                var serviceQuery = new ServiceQuery { PageSize = 1000 };
                var servicesResult = await _serviceService.GetServicesAsync(serviceQuery);
                var availableServices = servicesResult.IsSuccess ? servicesResult.Value.Items : new List<ServiceResponse>();
                
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
                        ProviderName = selectedService.ProviderBusinessName ?? "Unknown Provider",
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
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                var petOwner = petOwnerResult.IsSuccess ? petOwnerResult.Value : null;
                model.UserPets = petOwner != null ? (await _petRepository.GetByOwnerIdAsync(petOwner.Id)).ToList() : new List<Pet>();
                var serviceQuery = new ServiceQuery { PageSize = 1000 };
                var servicesResult = await _serviceService.GetServicesAsync(serviceQuery);
                model.AvailableServices = servicesResult.IsSuccess ? servicesResult.Value.Items : new List<ServiceResponse>();
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    TempData["Error"] = "Pet owner not found";
                    return RedirectToAction("Dashboard");
                }
                var petOwner = petOwnerResult.Value;

                // Validate that the pet belongs to the user
                var pets = await _petRepository.GetByOwnerIdAsync(petOwner.Id);
                var selectedPet = pets.FirstOrDefault(p => p.Id == model.PetId);
                if (selectedPet == null)
                {
                    TempData["Error"] = "Selected pet not found or doesn't belong to you";
                    return RedirectToAction("BookService");
                }

                // Validate service exists and is active
                var serviceResult = await _serviceService.GetServiceByIdAsync(model.ServiceId);
                if (serviceResult.IsFailure || serviceResult.Value == null || !serviceResult.Value.IsActive)
                {
                    TempData["Error"] = "Selected service is not available";
                    return RedirectToAction("BookService");
                }
                var service = serviceResult.Value;

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
                var availabilityCheck = await _bookingService.IsTimeSlotAvailableAsync(model.ServiceId, bookingDateTime, endDateTime, null, CancellationToken.None);
                if (availabilityCheck.IsFailure || !availabilityCheck.Value)
                {
                    TempData["Error"] = "The selected time slot is not available. Please choose a different time.";
                    return RedirectToAction("BookService");
                }

                // Create the booking request
                var createBookingRequest = new CreateBookingRequest
                {
                    OwnerId = petOwner.Id,
                    PetId = model.PetId,
                    ServiceId = model.ServiceId,
                    StartTime = bookingDateTime,
                    EndTime = endDateTime,
                    SpecialInstructions = model.SpecialInstructions,
                    TotalPrice = service.BasePrice // Use the service's base price
                };

                // Save the booking
                var createdBookingResult = await _bookingService.CreateBookingAsync(createBookingRequest);
                if (createdBookingResult.IsFailure)
                {
                    TempData["Error"] = createdBookingResult.ErrorMessage;
                    return RedirectToAction("BookService");
                }
                var createdBooking = createdBookingResult.Value;

                // Send email notification to service provider
                try
                {
                    var serviceProviderResult = await _serviceProviderService.GetServiceProviderByIdAsync(service.ProviderId);
                    if (serviceProviderResult.IsSuccess && serviceProviderResult.Value != null)
                    {
                        var serviceProvider = serviceProviderResult.Value;
                        var emailSubject = $"New Booking Request - {service.Title}";
                        var emailBody = $@"
                            <h2>New Booking Request</h2>
                            <p><strong>Service:</strong> {service.Title}</p>
                            <p><strong>Pet Owner:</strong> {petOwner.UserName}</p>
                            <p><strong>Pet:</strong> {createdBooking.PetName}</p>
                            <p><strong>Date & Time:</strong> {bookingDateTime:MMM dd, yyyy} at {startTimeSpan:hh:mm tt} - {endTimeSpan:hh:mm tt}</p>
                            <p><strong>Special Instructions:</strong> {model.SpecialInstructions ?? "None"}</p>
                            <p><strong>Total Price:</strong> ${service.BasePrice:F2}</p>
                            <br>
                            <p>Please log in to your account to confirm or decline this booking request.</p>
                            <p><a href='https://yourdomain.com/ServiceProvider/BookingRequest/{createdBooking.Id}'>View Booking Details</a></p>
                        ";
                        
                        await _emailService.SendEmailAsync(serviceProvider.UserEmail, emailSubject, emailBody);
                    }
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the booking
                    // In production, you might want to log this to a proper logging system
                    Console.WriteLine($"Failed to send email notification: {emailEx.Message}");
                }

                TempData["Success"] = "Booking request submitted successfully! The service provider will be notified and will confirm your booking shortly.";
                return RedirectToAction("MyBookings");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating booking: {ex.Message}";
                
                // Repopulate the pets dropdown and services
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                var petOwner = petOwnerResult.IsSuccess ? petOwnerResult.Value : null;
                model.UserPets = petOwner != null ? (await _petRepository.GetByOwnerIdAsync(petOwner.Id)).ToList() : new List<Pet>();
                var serviceQuery = new ServiceQuery { PageSize = 1000 };
                var servicesResult = await _serviceService.GetServicesAsync(serviceQuery);
                model.AvailableServices = servicesResult.IsSuccess ? servicesResult.Value.Items : new List<ServiceResponse>();
                
                return View(model);
            }
        }

        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> BookingDetails(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                return NotFound("Pet owner not found.");
            var petOwner = petOwnerResult.Value;

            // Use repository to get full Booking entity with navigation properties
            var booking = await _bookingRepository.GetByIdAsync(id);
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
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    return Json(new { success = false, message = "Pet owner not found." });
                }
                var petOwner = petOwnerResult.Value;

                var bookingResult = await _bookingService.GetBookingByIdAsync(id);
                if (bookingResult.IsFailure || bookingResult.Value == null || bookingResult.Value.OwnerId != petOwner.Id)
                {
                    return Json(new { success = false, message = "Booking not found or doesn't belong to you." });
                }

                var booking = bookingResult.Value;

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
                var cancelRequest = new CancelBookingRequest
                {
                    BookingId = booking.Id,
                    CancellationReason = "Cancelled by pet owner"
                };
                var cancelResult = await _bookingService.CancelBookingAsync(cancelRequest);
                if (cancelResult.IsFailure)
                {
                    return Json(new { success = false, message = cancelResult.ErrorMessage });
                }

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
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                TempData["Error"] = "Pet owner not found.";
                return RedirectToAction("MyBookings");
            }
            var petOwner = petOwnerResult.Value;

            var bookingResult = await _bookingService.GetBookingByIdAsync(id);
            if (bookingResult.IsFailure || bookingResult.Value == null || bookingResult.Value.OwnerId != petOwner.Id)
            {
                TempData["Error"] = "Booking not found or doesn't belong to you.";
                return RedirectToAction("MyBookings");
            }

            var booking = bookingResult.Value;

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
                ServiceTitle = booking.ServiceName,
                ProviderName = booking.OwnerName, // Note: BookingResponse doesn't have ProviderName, using OwnerName
                PetName = booking.PetName,
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
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    TempData["Error"] = "Pet owner not found.";
                    return RedirectToAction("MyBookings");
                }
                var petOwner = petOwnerResult.Value;

                var bookingResult = await _bookingService.GetBookingByIdAsync(model.Id);
                if (bookingResult.IsFailure || bookingResult.Value == null || bookingResult.Value.OwnerId != petOwner.Id)
                {
                    TempData["Error"] = "Booking not found or doesn't belong to you.";
                    return RedirectToAction("MyBookings");
                }

                var booking = bookingResult.Value;

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
                var availabilityResult = await _bookingService.IsTimeSlotAvailableAsync(booking.ServiceId, bookingDateTime, endDateTime, booking.Id, CancellationToken.None);
                if (availabilityResult.IsFailure || !availabilityResult.Value)
                {
                    ModelState.AddModelError("", "The selected time slot is not available. Please choose a different time.");
                    return View(model);
                }

                // Update the booking
                var updateRequest = new UpdateBookingRequest
                {
                    BookingId = booking.Id,
                    StartTime = bookingDateTime,
                    EndTime = endDateTime,
                    SpecialInstructions = model.SpecialInstructions
                };
                
                var updateResult = await _bookingService.UpdateBookingAsync(updateRequest);
                if (updateResult.IsFailure)
                {
                    TempData["Error"] = updateResult.ErrorMessage;
                    return View(model);
                }

                TempData["Success"] = "Booking updated successfully!";
                return RedirectToAction("MyBookings");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating booking: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Settings()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    return NotFound("Pet owner not found.");
                }
                var petOwner = petOwnerResult.Value;

                var userResult = await _userService.GetUserByIdAsync(userId);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    return NotFound("User not found.");
                }
                var user = userResult.Value;

                var viewModel = new PetOwnerSettingsViewModel
                {
                    // Account Settings
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    
                    // Location Settings
                    Address = user.Address ?? "",
                    City = user.City ?? "",
                    Province = user.Province ?? "",
                    PostalCode = user.PostalCode ?? "",
                    Bio = user.Bio ?? "",
                    
                    // Notification Settings
                    EmailNotifications = petOwner.ReceiveNotifications,
                    SMSNotifications = false, // Default to false
                    BookingReminders = true,
                    PaymentNotifications = true,
                    ReviewNotifications = true,
                    MarketingEmails = petOwner.ReceiveMarketingEmails,
                    
                    // Privacy Settings
                    ProfileVisibility = "Public", // Default to public
                    ShowContactInfo = true,
                    ShowLocation = true,
                    ShowPets = true,
                    
                    // Preferences
                    PreferredServiceTypes = petOwner.PreferredServiceTypes ?? "",
                    PreferredProviders = petOwner.PreferredProviderAttributes ?? "",
                    MaxTravelDistance = 25, // Default 25km
                    PreferredBookingAdvance = 24 // Default 24 hours
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PetOwnerController.Settings: {ex.Message}");
                TempData["Error"] = "An error occurred while loading settings. Please try again.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Settings(PetOwnerSettingsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    TempData["Error"] = "Pet owner not found";
                    return RedirectToAction("Dashboard");
                }
                var petOwner = petOwnerResult.Value;

                var userResult = await _userService.GetUserByIdAsync(userId);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Dashboard");
                }
                var user = userResult.Value;

                // Create update request DTOs
                var updateUserRequest = new UpdateUserProfileRequest
                {
                    UserId = userId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    Province = model.Province,
                    PostalCode = model.PostalCode,
                    Bio = model.Bio
                };

                var updatePetOwnerRequest = new UpdatePetOwnerRequest
                {
                    PetOwnerId = petOwner.Id,
                    ReceiveNotifications = model.EmailNotifications,
                    ReceiveMarketingEmails = model.MarketingEmails,
                    PreferredServiceTypes = model.PreferredServiceTypes,
                    PreferredProviderAttributes = model.PreferredProviders
                };

                var updateUserResult = await _userService.UpdateUserProfileAsync(updateUserRequest);
                if (updateUserResult.IsFailure)
                {
                    TempData["Error"] = updateUserResult.ErrorMessage;
                    return View(model);
                }

                var updatePetOwnerResult = await _petOwnerService.UpdatePetOwnerProfileAsync(updatePetOwnerRequest);
                if (updatePetOwnerResult.IsFailure)
                {
                    TempData["Error"] = updatePetOwnerResult.ErrorMessage;
                    return View(model);
                }

                TempData["Success"] = "Settings updated successfully!";
                return RedirectToAction("Settings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PetOwnerController.Settings POST: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating settings. Please try again.");
                return View(model);
            }
        }
    }
}