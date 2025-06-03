using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using System.Collections.Generic;

namespace PetCarePlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("geocode")]
        public async Task<IActionResult> GeocodeAddress([FromQuery] string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return BadRequest("Address is required");
            }

            try
            {
                var result = await _locationService.GeocodeAddressAsync(address);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("distance")]
        public async Task<IActionResult> CalculateDistance(
            [FromQuery] double lat1,
            [FromQuery] double lng1,
            [FromQuery] double lat2,
            [FromQuery] double lng2)
        {
            try
            {
                var distance = await _locationService.CalculateDistanceAsync(lat1, lng1, lat2, lng2);
                return Ok(new { distanceKm = distance });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("nearby-providers")]
        public async Task<IActionResult> FindNearbyProviders(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radiusKm = 10,
            [FromQuery] string serviceType = null)
        {
            try
            {
                var providers = await _locationService.FindNearbyServiceProvidersAsync(
                    latitude, longitude, radiusKm, serviceType);
                return Ok(providers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("static-map")]
        public IActionResult GetStaticMapUrl(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] int zoom = 14,
            [FromQuery] int width = 600,
            [FromQuery] int height = 400)
        {
            var url = _locationService.GenerateStaticMapUrl(latitude, longitude, zoom, width, height);
            return Ok(new { mapUrl = url });
        }

        [HttpGet("directions")]
        public IActionResult GetDirectionsUrl(
            [FromQuery] string origin,
            [FromQuery] string destination)
        {
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
            {
                return BadRequest("Origin and destination addresses are required");
            }

            var url = _locationService.GenerateDirectionsUrl(origin, destination);
            return Ok(new { directionsUrl = url });
        }
    }
}
