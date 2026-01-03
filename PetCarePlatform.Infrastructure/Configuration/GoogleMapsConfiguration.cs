namespace PetCarePlatform.Infrastructure.Configuration
{
    /// <summary>
    /// Strongly-typed Google Maps configuration.
    /// </summary>
    public class GoogleMapsConfiguration
    {
        public const string SectionName = "GoogleMaps";

        public string ApiKey { get; set; } = string.Empty;
    }
}

