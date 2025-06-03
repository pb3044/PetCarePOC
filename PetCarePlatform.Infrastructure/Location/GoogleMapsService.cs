using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Infrastructure.Location
{
    public class GoogleMapsService : ILocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IConfiguration _configuration;

        public GoogleMapsService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _apiKey = _configuration["GoogleMaps:ApiKey"];
            _httpClient = httpClient;
        }

        public async Task<GeocodingResult> GeocodeAddressAsync(string address)
        {
            try
            {
                var encodedAddress = Uri.EscapeDataString(address);
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_apiKey}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadFromJsonAsync<GoogleGeocodingResponse>();
                
                if (content.Status != "OK")
                {
                    throw new Exception($"Geocoding API error: {content.Status}");
                }
                
                if (content.Results.Count == 0)
                {
                    throw new Exception("No geocoding results found");
                }
                
                var result = content.Results[0];
                return new GeocodingResult
                {
                    FormattedAddress = result.FormattedAddress,
                    Latitude = result.Geometry.Location.Lat,
                    Longitude = result.Geometry.Location.Lng,
                    PlaceId = result.PlaceId
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error geocoding address: {ex.Message}", ex);
            }
        }

        public async Task<double> CalculateDistanceAsync(double lat1, double lng1, double lat2, double lng2)
        {
            try
            {
                var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={lat1},{lng1}&destinations={lat2},{lng2}&mode=driving&key={_apiKey}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadFromJsonAsync<GoogleDistanceMatrixResponse>();
                
                if (content.Status != "OK")
                {
                    throw new Exception($"Distance Matrix API error: {content.Status}");
                }
                
                if (content.Rows.Count == 0 || content.Rows[0].Elements.Count == 0)
                {
                    throw new Exception("No distance results found");
                }
                
                var element = content.Rows[0].Elements[0];
                if (element.Status != "OK")
                {
                    throw new Exception($"Distance calculation error: {element.Status}");
                }
                
                // Return distance in kilometers
                return element.Distance.Value / 1000.0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating distance: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<NearbyServiceProvider>> FindNearbyServiceProvidersAsync(double latitude, double longitude, double radiusKm, string serviceType = null)
        {
            // This would typically involve a database query using spatial data
            // For now, we'll implement a simplified version that could be expanded later
            
            // In a real implementation, you would:
            // 1. Query your database for service providers within the radius
            // 2. Filter by service type if provided
            // 3. Calculate exact distances and sort by proximity
            
            throw new NotImplementedException("This method requires database integration with spatial queries");
        }

        public string GenerateStaticMapUrl(double latitude, double longitude, int zoom = 14, int width = 600, int height = 400)
        {
            return $"https://maps.googleapis.com/maps/api/staticmap?center={latitude},{longitude}&zoom={zoom}&size={width}x{height}&markers=color:red%7C{latitude},{longitude}&key={_apiKey}";
        }

        public string GenerateDirectionsUrl(string originAddress, string destinationAddress)
        {
            var origin = Uri.EscapeDataString(originAddress);
            var destination = Uri.EscapeDataString(destinationAddress);
            return $"https://www.google.com/maps/dir/?api=1&origin={origin}&destination={destination}&travelmode=driving";
        }
    }

    public class GoogleGeocodingResponse
    {
        public string Status { get; set; }
        public List<GoogleGeocodingResult> Results { get; set; }
    }

    public class GoogleGeocodingResult
    {
        public string FormattedAddress { get; set; }
        public string PlaceId { get; set; }
        public GoogleGeometry Geometry { get; set; }
    }

    public class GoogleGeometry
    {
        public GoogleLocation Location { get; set; }
    }

    public class GoogleLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class GoogleDistanceMatrixResponse
    {
        public string Status { get; set; }
        public List<GoogleDistanceMatrixRow> Rows { get; set; }
    }

    public class GoogleDistanceMatrixRow
    {
        public List<GoogleDistanceMatrixElement> Elements { get; set; }
    }

    public class GoogleDistanceMatrixElement
    {
        public string Status { get; set; }
        public GoogleDistanceMatrixValue Distance { get; set; }
        public GoogleDistanceMatrixValue Duration { get; set; }
    }

    public class GoogleDistanceMatrixValue
    {
        public double Value { get; set; }
        public string Text { get; set; }
    }
}
