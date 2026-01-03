using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Interfaces;

namespace PetCarePlatform.Infrastructure.Location
{
    /// <summary>
    /// OpenStreetMap service implementation using Nominatim API for geocoding.
    /// Completely free, no API key required.
    /// Nominatim Usage Policy: https://operations.osmfoundation.org/policies/nominatim/
    /// - Maximum 1 request per second
    /// - User-Agent header is required
    /// </summary>
    public class OpenStreetMapService : ILocationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenStreetMapService> _logger;
        private const string NominatimBaseUrl = "https://nominatim.openstreetmap.org";
        
        // Rate limiting: Nominatim allows 1 request per second
        private static readonly SemaphoreSlim _rateLimiter = new SemaphoreSlim(1, 1);
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly TimeSpan _minRequestInterval = TimeSpan.FromSeconds(1.1); // Slightly more than 1 second for safety

        public OpenStreetMapService(HttpClient httpClient, ILogger<OpenStreetMapService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // User-Agent header is configured in Program.cs via HttpClient factory
        }

        public async Task<GeocodingResult> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address cannot be null or empty", nameof(address));
            }

            // Enforce rate limiting: 1 request per second
            await _rateLimiter.WaitAsync();
            try
            {
                var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
                if (timeSinceLastRequest < _minRequestInterval)
                {
                    var delay = _minRequestInterval - timeSinceLastRequest;
                    await Task.Delay(delay);
                }
                _lastRequestTime = DateTime.UtcNow;

                return await GeocodeAddressInternalAsync(address);
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        private async Task<GeocodingResult> GeocodeAddressInternalAsync(string address)
        {
            const int maxRetries = 2;
            int retryCount = 0;

            _logger.LogDebug("Geocoding address: {Address}", address);

            while (retryCount <= maxRetries)
            {
                try
                {
                    var encodedAddress = Uri.EscapeDataString(address);
                    var url = $"{NominatimBaseUrl}/search?format=json&q={encodedAddress}&limit=1&addressdetails=1";
                    
                    _logger.LogDebug("Making request to Nominatim API: {Url}", url);
                    var response = await _httpClient.GetAsync(url);
                    
                    // Handle specific HTTP status codes
                    if (!response.IsSuccessStatusCode)
                    {
                        var statusCode = (int)response.StatusCode;
                        var reasonPhrase = response.ReasonPhrase;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        
                        _logger.LogWarning(
                            "Nominatim API returned error status {StatusCode} {ReasonPhrase} for address: {Address}. Response: {ResponseBody}",
                            statusCode, reasonPhrase, address, responseBody);
                        
                        // 403 Forbidden - usually means User-Agent issue or rate limiting
                        if (statusCode == 403)
                        {
                            _logger.LogError(
                                "Nominatim API returned 403 Forbidden. User-Agent header may be missing or invalid. " +
                                "Check Program.cs configuration. Request URL: {Url}. Response: {ResponseBody}",
                                url, responseBody);
                            throw new HttpRequestException(
                                $"Nominatim API returned 403 Forbidden. " +
                                $"This usually means the User-Agent header is missing or invalid, or the IP is blocked. " +
                                $"Response: {reasonPhrase}. Body: {responseBody}. " +
                                $"Please ensure User-Agent is configured in Program.cs. " +
                                $"Request URL: {url}");
                        }
                        
                        // 429 Too Many Requests - rate limiting
                        if (statusCode == 429)
                        {
                            if (retryCount < maxRetries)
                            {
                                retryCount++;
                                var retryDelay = TimeSpan.FromSeconds(Math.Pow(2, retryCount)); // Exponential backoff
                                _logger.LogInformation(
                                    "Rate limit hit (429). Retrying after {Delay} seconds (attempt {RetryCount}/{MaxRetries})",
                                    retryDelay.TotalSeconds, retryCount, maxRetries);
                                await Task.Delay(retryDelay);
                                continue;
                            }
                            _logger.LogError("Rate limit exceeded after {MaxRetries} retries", maxRetries);
                            throw new HttpRequestException(
                                $"Nominatim API returned 429 Too Many Requests. " +
                                $"Rate limit exceeded. Please wait before making more requests. " +
                                $"Response: {reasonPhrase}");
                        }
                        
                        // Other HTTP errors
                        throw new HttpRequestException(
                            $"Nominatim API returned error: {statusCode} {reasonPhrase}. " +
                            $"Response body: {responseBody}");
                    }
                    
                    var content = await response.Content.ReadFromJsonAsync<List<NominatimGeocodingResult>>();
                    
                    if (content == null || content.Count == 0)
                    {
                        _logger.LogWarning("No geocoding results found for address: {Address}", address);
                        throw new Exception($"No geocoding results found for address: {address}");
                    }
                    
                    var result = content[0];
                    _logger.LogInformation(
                        "Successfully geocoded address: {Address} -> ({Latitude}, {Longitude})",
                        address, result.Lat, result.Lon);
                    
                    return new GeocodingResult
                    {
                        FormattedAddress = result.DisplayName ?? address,
                        Latitude = double.Parse(result.Lat),
                        Longitude = double.Parse(result.Lon),
                        PlaceId = result.PlaceId?.ToString() ?? string.Empty
                    };
                }
                catch (HttpRequestException)
                {
                    // Re-throw HTTP exceptions as-is (they already have good error messages)
                    throw;
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogError(ex, "Request to Nominatim API timed out after {Timeout} seconds for address: {Address}",
                        _httpClient.Timeout.TotalSeconds, address);
                    throw new TimeoutException($"Request to Nominatim API timed out after {_httpClient.Timeout.TotalSeconds} seconds. Address: {address}", ex);
                }
                catch (Exception ex) when (retryCount < maxRetries && !(ex is HttpRequestException))
                {
                    // Retry on transient errors (but not HTTP exceptions)
                    retryCount++;
                    var retryDelay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    _logger.LogWarning(ex,
                        "Transient error geocoding address. Retrying after {Delay} seconds (attempt {RetryCount}/{MaxRetries}): {Address}",
                        retryDelay.TotalSeconds, retryCount, maxRetries, address);
                    await Task.Delay(retryDelay);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error geocoding address: {Address}", address);
                    throw new Exception($"Error geocoding address '{address}': {ex.Message}", ex);
                }
            }

            _logger.LogError("Failed to geocode address after {MaxRetries} retries: {Address}", maxRetries, address);
            throw new Exception($"Failed to geocode address after {maxRetries} retries: {address}");
        }

        public Task<double> CalculateDistanceAsync(double lat1, double lng1, double lat2, double lng2)
        {
            // Use Haversine formula to calculate distance between two points
            // No API call needed - pure mathematical calculation
            var distance = HaversineDistance(lat1, lng1, lat2, lng2);
            return Task.FromResult(distance);
        }

        public Task<IEnumerable<NearbyServiceProvider>> FindNearbyServiceProvidersAsync(double latitude, double longitude, double radiusKm, string serviceType = null)
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
            // Use OpenStreetMap static map service
            // Alternative: Could use Leaflet static image or other OSM static map services
            return $"https://www.openstreetmap.org/export/embed.html?bbox={(longitude - 0.01)},{latitude - 0.01},{(longitude + 0.01)},{latitude + 0.01}&layer=mapnik&marker={latitude},{longitude}";
        }

        public string GenerateDirectionsUrl(string originAddress, string destinationAddress)
        {
            var origin = Uri.EscapeDataString(originAddress);
            var destination = Uri.EscapeDataString(destinationAddress);
            return $"https://www.openstreetmap.org/directions?from={origin}&to={destination}";
        }

        /// <summary>
        /// Calculate the great-circle distance between two points on Earth using the Haversine formula.
        /// Returns distance in kilometers.
        /// </summary>
        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers
            
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return R * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }

    // Nominatim API response models
    public class NominatimGeocodingResult
    {
        [JsonPropertyName("place_id")]
        public long? PlaceId { get; set; }
        
        [JsonPropertyName("licence")]
        public string Licence { get; set; }
        
        [JsonPropertyName("osm_type")]
        public string OsmType { get; set; }
        
        [JsonPropertyName("osm_id")]
        public long OsmId { get; set; }
        
        [JsonPropertyName("boundingbox")]
        public List<string> BoundingBox { get; set; }
        
        [JsonPropertyName("lat")]
        public string Lat { get; set; }
        
        [JsonPropertyName("lon")]
        public string Lon { get; set; }
        
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
        
        [JsonPropertyName("class")]
        public string Class { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("importance")]
        public double Importance { get; set; }
        
        [JsonPropertyName("address")]
        public NominatimAddress Address { get; set; }
    }

    public class NominatimAddress
    {
        [JsonPropertyName("house_number")]
        public string HouseNumber { get; set; }
        
        [JsonPropertyName("road")]
        public string Road { get; set; }
        
        [JsonPropertyName("city")]
        public string City { get; set; }
        
        [JsonPropertyName("state")]
        public string State { get; set; }
        
        [JsonPropertyName("postcode")]
        public string Postcode { get; set; }
        
        [JsonPropertyName("country")]
        public string Country { get; set; }
    }
}

