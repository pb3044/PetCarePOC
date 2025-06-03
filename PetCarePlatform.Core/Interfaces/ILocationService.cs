using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetCarePlatform.Core.Interfaces
{
    public interface ILocationService
    {
        Task<GeocodingResult> GeocodeAddressAsync(string address);
        Task<double> CalculateDistanceAsync(double lat1, double lng1, double lat2, double lng2);
        Task<IEnumerable<NearbyServiceProvider>> FindNearbyServiceProvidersAsync(double latitude, double longitude, double radiusKm, string serviceType = null);
        string GenerateStaticMapUrl(double latitude, double longitude, int zoom = 14, int width = 600, int height = 400);
        string GenerateDirectionsUrl(string originAddress, string destinationAddress);
    }

    public class GeocodingResult
    {
        public string FormattedAddress { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PlaceId { get; set; }
    }

    public class NearbyServiceProvider
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DistanceKm { get; set; }
        public string Address { get; set; }
        public double Rating { get; set; }
        public List<string> ServiceTypes { get; set; }
    }
}
