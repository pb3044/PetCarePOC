using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using System.Collections.Generic;

namespace PetCarePlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
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

        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _serviceService.GetAllServicesAsync();
            return Ok(services);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetService(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            return Ok(service);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchServices(
            [FromQuery] string? keyword = null,
            [FromQuery] ServiceType? type = null,
            [FromQuery] double? latitude = null,
            [FromQuery] double? longitude = null,
            [FromQuery] int? radiusInKm = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? petTypes = null,
            [FromQuery] string? petSizes = null)
        {
            var services = await _serviceService.SearchServicesAsync(
                keyword ?? string.Empty, // Ensure non-null value
                type,
                latitude,
                longitude,
                radiusInKm,
                minPrice,
                maxPrice,
                petTypes ?? string.Empty, // Ensure non-null value
                petSizes ?? string.Empty); // Ensure non-null value

            return Ok(services);
        }

        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> GetServicesByProvider(int providerId)
        {
            var services = await _serviceProviderService.GetServicesByProviderIdAsync(providerId);
            return Ok(services);
        }

        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var service = new Service
                {
                    ProviderId = request.ProviderId,
                    Title = request.Title,
                    Description = request.Description,
                    Type = request.Type,
                    BasePrice = request.BasePrice,
                    PriceUnit = request.PriceUnit,
                    Location = request.Location,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    AcceptedPetTypes = request.AcceptedPetTypes,
                    AcceptedPetSizes = request.AcceptedPetSizes,
                    MaxPetsPerBooking = request.MaxPetsPerBooking
                };

                var createdService = await _serviceService.CreateServiceAsync(service);
                return CreatedAtAction(nameof(GetService), new { id = createdService.Id }, createdService);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                // Update service fields
                service.Title = request.Title;
                service.Description = request.Description;
                service.Type = request.Type;
                service.BasePrice = request.BasePrice;
                service.PriceUnit = request.PriceUnit;
                service.IsActive = request.IsActive;
                service.Location = request.Location;
                service.Latitude = request.Latitude;
                service.Longitude = request.Longitude;
                service.AcceptedPetTypes = request.AcceptedPetTypes;
                service.AcceptedPetSizes = request.AcceptedPetSizes;
                service.MaxPetsPerBooking = request.MaxPetsPerBooking;

                await _serviceService.UpdateServiceAsync(service);
                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                await _serviceService.DeleteServiceAsync(id);
                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/photos")]
        public async Task<IActionResult> GetServicePhotos(int id)
        {
            try
            {
                var photos = await _serviceService.GetServicePhotosAsync(id);
                return Ok(photos);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/photos")]
        public async Task<IActionResult> AddServicePhoto(int id, [FromBody] AddServicePhotoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var photo = new ServicePhoto
                {
                    ServiceId = id,
                    Url = request.Url,
                    Caption = request.Caption,
                    IsPrimary = request.IsPrimary,
                    CreatedAt = System.DateTime.UtcNow
                };

                await _serviceService.AddServicePhotoAsync(photo);
                return Ok(new { message = "Photo added successfully" });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("photos/{photoId}")]
        public async Task<IActionResult> DeleteServicePhoto(int photoId)
        {
            try
            {
                await _serviceService.DeleteServicePhotoAsync(photoId);
                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{serviceId}/photos/{photoId}/set-primary")]
        public async Task<IActionResult> SetPrimaryPhoto(int serviceId, int photoId)
        {
            try
            {
                await _serviceService.SetPrimaryPhotoAsync(serviceId, photoId);
                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetServiceReviews(int id)
        {
            try
            {
                var reviews = await _serviceService.GetServiceReviewsAsync(id);
                return Ok(reviews);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/rating")]
        public async Task<IActionResult> GetServiceRating(int id)
        {
            try
            {
                var rating = await _serviceService.GetServiceRatingAsync(id);
                return Ok(new { rating });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class CreateServiceRequest
    {
        public int ProviderId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ServiceType Type { get; set; }
        public decimal BasePrice { get; set; }
        public string PriceUnit { get; set; }
        public string Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AcceptedPetTypes { get; set; }
        public string AcceptedPetSizes { get; set; }
        public int MaxPetsPerBooking { get; set; }
    }

    public class UpdateServiceRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public ServiceType Type { get; set; }
        public decimal BasePrice { get; set; }
        public string PriceUnit { get; set; }
        public bool IsActive { get; set; }
        public string Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AcceptedPetTypes { get; set; }
        public string AcceptedPetSizes { get; set; }
        public int MaxPetsPerBooking { get; set; }
    }

    public class AddServicePhotoRequest
    {
        public string Url { get; set; }
        public string Caption { get; set; }
        public bool IsPrimary { get; set; }
    }
}
