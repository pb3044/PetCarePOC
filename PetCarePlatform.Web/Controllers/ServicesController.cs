using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Web.Models;

namespace PetCarePlatform.Web.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly IServiceProviderService _serviceProviderService;
        private readonly ILocationService _locationService;
        private readonly IConfiguration _configuration;
        private readonly IServiceRepository _serviceRepository;

        public ServicesController(
            IServiceService serviceService,
            IServiceProviderService serviceProviderService,
            ILocationService locationService,
            IConfiguration configuration,
            IServiceRepository serviceRepository)
        {
            _serviceService = serviceService;
            _serviceProviderService = serviceProviderService;
            _locationService = locationService;
            _configuration = configuration;
            _serviceRepository = serviceRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var query = new PetCarePlatform.Core.DTOs.Queries.ServiceQuery
                {
                    PageNumber = 1,
                    PageSize = 100
                };
                var servicesResult = await _serviceService.GetServicesAsync(query, CancellationToken.None);
                if (servicesResult.IsFailure)
                {
                    ViewBag.ErrorMessage = servicesResult.ErrorMessage;
                    ViewBag.ServiceCount = 0;
                    return View(new PetCarePlatform.Core.Common.PagedResult<PetCarePlatform.Core.DTOs.Responses.ServiceResponse>(
                        new List<PetCarePlatform.Core.DTOs.Responses.ServiceResponse>(), 0, 1, 100));
                }
                // Add some debugging information
                ViewBag.ServiceCount = servicesResult.Value?.Items?.Count() ?? 0;
                return View(servicesResult.Value);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, you'd use a proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Error in ServicesController.Index: {ex.Message}");
                
                // Return a view with error information
                ViewBag.ErrorMessage = "An error occurred while loading services. Please try again later.";
                ViewBag.ServiceCount = 0;
                return View(new PetCarePlatform.Core.Common.PagedResult<PetCarePlatform.Core.DTOs.Responses.ServiceResponse>(
                    new List<PetCarePlatform.Core.DTOs.Responses.ServiceResponse>(), 0, 1, 100));
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                return View(service);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(SearchServicesViewModel model)
        {
            try
            {
                // Handle location-based search
                if (!string.IsNullOrEmpty(model.Location) && (!model.Latitude.HasValue || !model.Longitude.HasValue))
                {
                    // Sanitize location input: remove extra spaces, especially before/after commas
                    // Example: "Oak Bay ,BC" -> "Oak Bay, BC"
                    var sanitizedLocation = System.Text.RegularExpressions.Regex.Replace(model.Location, @"\s*,\s*", ", ");
                    sanitizedLocation = sanitizedLocation.Trim();

                    try
                    {
                        var geocodingResult = await _locationService.GeocodeAddressAsync(sanitizedLocation);
                        model.Latitude = geocodingResult.Latitude;
                        model.Longitude = geocodingResult.Longitude;
                        model.Location = geocodingResult.FormattedAddress; // Use the formatted address
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Geocoding failed for: {sanitizedLocation}. Error: {ex.Message}");

                        // If it's a block or timeout, don't try again with suffix as it's likely doomed to fail and waste time
                        bool isBlockedOrTimeout = ex.Message.Contains("403") || ex.Message.Contains("429") || ex is TimeoutException;

                        if (!isBlockedOrTimeout)
                        {
                            // If geocoding fails for other reasons, try with "BC, Canada" suffix for better results
                            try
                            {
                                var locationWithSuffix = sanitizedLocation.Contains(", BC") || sanitizedLocation.Contains(",BC")
                                    ? sanitizedLocation
                                    : $"{sanitizedLocation}, BC, Canada";
                                var geocodingResult = await _locationService.GeocodeAddressAsync(locationWithSuffix);
                                model.Latitude = geocodingResult.Latitude;
                                model.Longitude = geocodingResult.Longitude;
                                model.Location = geocodingResult.FormattedAddress;
                            }
                            catch
                            {
                                // If still fails, continue with text-based search but log the error
                                ModelState.AddModelError("Location", $"Could not find the exact location '{model.Location}'. Showing results based on text search.");
                            }
                        }
                        else
                        {
                            // Log the specific block/timeout
                            ModelState.AddModelError("Location", "Geocoding service is temporarily unavailable. Showing results based on text search.");
                        }
                    }
                }

                // Set default radius if not specified
                if (!model.RadiusInKm.HasValue)
                {
                    model.RadiusInKm = 25; // Default 25km radius
                }

                // Initialize quick filters
                model.QuickFilters = new List<string> { "All", "Dog Walking", "Pet Grooming", "Pet Sitting", "Dog Training", "Pet Boarding" };

                // Build search query
                var searchQuery = new PetCarePlatform.Core.DTOs.Queries.ServiceQuery
                {
                    PageNumber = 1,
                    PageSize = 100,
                    Keyword = model.Keyword,
                    Type = model.Type,
                    MinPrice = model.MinPrice,
                    MaxPrice = model.MaxPrice,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    RadiusInKm = model.RadiusInKm,
                    PetTypes = model.PetType?.ToString(),
                    PetSizes = model.PetSize?.ToString(),
                    MinRating = model.MinRating,
                    SortBy = model.SortBy ?? "Relevance"
                };

                var searchResult = await _serviceService.SearchServicesAsync(searchQuery);
                if (searchResult.IsSuccess && searchResult.Value != null && searchResult.Value.Items != null)
                {
                    // Map ServiceResponse to Service objects for the view
                    model.Results = searchResult.Value.Items.Select(sr => new Service
                    {
                        Id = sr.Id,
                        ProviderId = sr.ProviderId,
                        Title = sr.Title,
                        Description = sr.Description,
                        Type = sr.Type,
                        BasePrice = sr.BasePrice,
                        PriceUnit = sr.PriceUnit,
                        IsActive = sr.IsActive,
                        Location = sr.Location ?? string.Empty,
                        Latitude = sr.Latitude,
                        Longitude = sr.Longitude,
                        AcceptedPetTypes = sr.AcceptedPetTypes,
                        AcceptedPetSizes = sr.AcceptedPetSizes,
                        MaxPetsPerBooking = sr.MaxPetsPerBooking ?? 1,
                        CreatedAt = sr.CreatedAt,
                        UpdatedAt = sr.UpdatedAt,
                        // Create a minimal Provider object with rating info for the view
                        // Note: IsVerified will be false by default - can be enhanced later to fetch full provider details
                        Provider = new PetCarePlatform.Core.Models.ServiceProvider
                        {
                            Id = sr.ProviderId,
                            AverageRating = sr.AverageRating,
                            TotalReviews = sr.ReviewCount,
                            BackgroundCheckVerified = false, // Can be enhanced to fetch from provider details if needed
                            IdentityVerified = false, // Can be enhanced to fetch from provider details if needed
                        }
                    }).ToList();
                }
                else
                {
                    model.Results = new List<Service>();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while searching for services. Please try again.");
                model.Results = new List<Service>();
                return View(model);
            }
        }

        [Authorize(Roles = "ServiceProvider")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "ServiceProvider")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var service = new Service
                {
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.ServiceType,
                    BasePrice = model.BasePrice,
                    PriceUnit = model.PriceUnit,
                    MaxPetsPerBooking = model.MaxPetsPerBooking,
                    AcceptedPetTypes = model.AcceptedPetTypes,
                    AcceptedPetSizes = model.AcceptedPetSizes,
                    Location = model.Location,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createRequest = new CreateServiceRequest
                {
                    ProviderId = service.ProviderId,
                    Title = service.Title,
                    Description = service.Description,
                    Type = service.Type,
                    BasePrice = service.BasePrice,
                    PriceUnit = service.PriceUnit,
                    MaxPetsPerBooking = service.MaxPetsPerBooking,
                    AcceptedPetTypes = service.AcceptedPetTypes,
                    AcceptedPetSizes = service.AcceptedPetSizes,
                    Location = service.Location
                };

                var createdServiceResult = await _serviceService.CreateServiceAsync(createRequest);
                if (createdServiceResult.IsFailure)
                {
                    ModelState.AddModelError("", createdServiceResult.ErrorMessage);
                    return View(model);
                }

                TempData["SuccessMessage"] = "Service created successfully!";
                return RedirectToAction("Details", new { id = createdServiceResult.Value!.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the service: " + ex.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "ServiceProvider")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var serviceResult = await _serviceService.GetServiceByIdAsync(id);
            if (serviceResult.IsFailure || serviceResult.Value == null)
            {
                return NotFound();
            }

            var service = serviceResult.Value;
            var model = new EditServiceViewModel
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                ServiceType = service.Type,
                BasePrice = service.BasePrice,
                PriceUnit = service.PriceUnit,
                MaxPetsPerBooking = service.MaxPetsPerBooking ?? 1,
                AcceptedPetTypes = service.AcceptedPetTypes,
                AcceptedPetSizes = service.AcceptedPetSizes,
                Location = service.Location,
                IsActive = service.IsActive
            };

            return View(model);
        }

        [Authorize(Roles = "ServiceProvider")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var serviceResult = await _serviceService.GetServiceByIdAsync(model.Id, CancellationToken.None);
                if (serviceResult.IsFailure || serviceResult.Value == null)
                {
                    return NotFound();
                }

                var updateRequest = new PetCarePlatform.Core.DTOs.Requests.UpdateServiceRequest
                {
                    ServiceId = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.ServiceType,
                    BasePrice = model.BasePrice,
                    PriceUnit = model.PriceUnit,
                    MaxPetsPerBooking = model.MaxPetsPerBooking,
                    AcceptedPetTypes = model.AcceptedPetTypes,
                    AcceptedPetSizes = model.AcceptedPetSizes,
                    Location = model.Location,
                    IsActive = model.IsActive
                };

                var updateResult = await _serviceService.UpdateServiceAsync(updateRequest, CancellationToken.None);
                if (updateResult.IsFailure)
                {
                    ModelState.AddModelError("", updateResult.ErrorMessage);
                    return View(model);
                }

                TempData["SuccessMessage"] = "Service updated successfully!";
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the service: " + ex.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "ServiceProvider")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _serviceService.DeleteServiceAsync(id);
                TempData["SuccessMessage"] = "Service deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the service: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ByCategory(ServiceType category)
        {
            var query = new PetCarePlatform.Core.DTOs.Queries.ServiceQuery
            {
                Type = category,
                PageNumber = 1,
                PageSize = 100
            };
            var servicesResult = await _serviceService.SearchServicesAsync(query, CancellationToken.None);
            if (servicesResult.IsFailure)
            {
                ViewBag.ErrorMessage = servicesResult.ErrorMessage;
                return View("Index", new PetCarePlatform.Core.Common.PagedResult<PetCarePlatform.Core.DTOs.Responses.ServiceResponse>(
                    new List<PetCarePlatform.Core.DTOs.Responses.ServiceResponse>(), 0, 1, 100));
            }
            ViewBag.Category = category;
            return View("Index", servicesResult.Value);
        }

        public async Task<IActionResult> Reviews(int id)
        {
            var reviewsResult = await _serviceService.GetServiceReviewsAsync(id, CancellationToken.None);
            var serviceResult = await _serviceService.GetServiceByIdAsync(id, CancellationToken.None);
            if (serviceResult.IsSuccess && serviceResult.Value != null)
            {
                ViewBag.Service = serviceResult.Value;
            }
            if (reviewsResult.IsFailure)
            {
                ViewBag.ErrorMessage = reviewsResult.ErrorMessage;
                return View(new PetCarePlatform.Core.Common.PagedResult<PetCarePlatform.Core.DTOs.Responses.ReviewResponse>(
                    new List<PetCarePlatform.Core.DTOs.Responses.ReviewResponse>(), 0, 1, 100));
            }
            return View(reviewsResult.Value);
        }

        public async Task<IActionResult> Photos(int id)
        {
            var photosResult = await _serviceService.GetServicePhotosAsync(id, CancellationToken.None);
            var serviceResult = await _serviceService.GetServiceByIdAsync(id, CancellationToken.None);
            if (serviceResult.IsSuccess && serviceResult.Value != null)
            {
                ViewBag.Service = serviceResult.Value;
            }
            if (photosResult.IsFailure)
            {
                ViewBag.ErrorMessage = photosResult.ErrorMessage;
                return View(new List<PetCarePlatform.Core.DTOs.Responses.ServicePhotoResponse>());
            }
            return View(photosResult.Value);
        }
    }
}

