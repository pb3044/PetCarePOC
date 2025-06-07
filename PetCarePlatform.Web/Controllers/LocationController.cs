using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;

namespace PetCarePlatform.Web.Controllers
{
    public class LocationController : Controller
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCoordinates(string address)
        {
            try
            {
                var result = await _locationService.GeocodeAddressAsync(address);
                return Json(new { 
                    success = true, 
                    latitude = result.Latitude, 
                    longitude = result.Longitude,
                    formattedAddress = result.FormattedAddress,
                    placeId = result.PlaceId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            try
            {
                var distance = await _locationService.CalculateDistanceAsync(lat1, lng1, lat2, lng2);
                return Json(new { success = true, distance });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindNearbyProviders(double latitude, double longitude, double radiusKm, string serviceType = null)
        {
            try
            {
                var providers = await _locationService.FindNearbyServiceProvidersAsync(latitude, longitude, radiusKm, serviceType);
                return Json(new { success = true, providers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetStaticMapUrl(double latitude, double longitude, int zoom = 14, int width = 600, int height = 400)
        {
            try
            {
                var mapUrl = _locationService.GenerateStaticMapUrl(latitude, longitude, zoom, width, height);
                return Json(new { success = true, mapUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetDirectionsUrl(string originAddress, string destinationAddress)
        {
            try
            {
                var directionsUrl = _locationService.GenerateDirectionsUrl(originAddress, destinationAddress);
                return Json(new { success = true, directionsUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}

