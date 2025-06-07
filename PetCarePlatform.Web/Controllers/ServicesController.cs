using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Web.Models;

namespace PetCarePlatform.Web.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly IServiceProviderService _serviceProviderService;

        public ServicesController(
            IServiceService serviceService,
            IServiceProviderService serviceProviderService)
        {
            _serviceService = serviceService;
            _serviceProviderService = serviceProviderService;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _serviceService.GetAllServicesAsync();
            return View(services);
        }

        public async Task<IActionResult> Details(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpGet]
        public async Task<IActionResult> Search(SearchServicesViewModel model)
        {
            if (string.IsNullOrEmpty(model.Keyword) && !model.Type.HasValue && !model.MinPrice.HasValue && !model.MaxPrice.HasValue)
            {
                // If no search criteria, show all services
                model.Results = await _serviceService.GetAllServicesAsync();
            }
            else
            {
                // Perform search based on criteria
                model.Results = await _serviceService.SearchServicesAsync(
                    model.Keyword,
                    model.Type,
                    model.Latitude,
                    model.Longitude,
                    model.RadiusInKm,
                    model.MinPrice,
                    model.MaxPrice);
            }

            return View(model);
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

                var createdService = await _serviceService.CreateServiceAsync(service);
                TempData["SuccessMessage"] = "Service created successfully!";
                return RedirectToAction("Details", new { id = createdService.Id });
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
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            var model = new EditServiceViewModel
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                ServiceType = service.Type,
                BasePrice = service.BasePrice,
                PriceUnit = service.PriceUnit,
                MaxPetsPerBooking = service.MaxPetsPerBooking,
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
                var service = await _serviceService.GetServiceByIdAsync(model.Id);
                if (service == null)
                {
                    return NotFound();
                }

                service.Title = model.Title;
                service.Description = model.Description;
                service.Type = model.ServiceType;
                service.BasePrice = model.BasePrice;
                service.PriceUnit = model.PriceUnit;
                service.MaxPetsPerBooking = model.MaxPetsPerBooking;
                service.AcceptedPetTypes = model.AcceptedPetTypes;
                service.AcceptedPetSizes = model.AcceptedPetSizes;
                service.Location = model.Location;
                service.IsActive = model.IsActive;
                service.UpdatedAt = DateTime.UtcNow;

                await _serviceService.UpdateServiceAsync(service);
                TempData["SuccessMessage"] = "Service updated successfully!";
                return RedirectToAction("Details", new { id = service.Id });
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
            var services = await _serviceService.SearchServicesAsync(type: category);
            ViewBag.Category = category;
            return View("Index", services);
        }

        public async Task<IActionResult> Reviews(int id)
        {
            var reviews = await _serviceService.GetServiceReviewsAsync(id);
            var service = await _serviceService.GetServiceByIdAsync(id);
            ViewBag.Service = service;
            return View(reviews);
        }

        public async Task<IActionResult> Photos(int id)
        {
            var photos = await _serviceService.GetServicePhotosAsync(id);
            var service = await _serviceService.GetServiceByIdAsync(id);
            ViewBag.Service = service;
            return View(photos);
        }
    }
}

