namespace PetCarePlatform.Core.DTOs.Responses
{
    public class ServicePhotoResponse
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

