using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetCarePlatform.Web.Models
{
    public class CreateReviewViewModel
    {
        public int BookingId { get; set; }
        
        [Display(Name = "Service")]
        public string ServiceName { get; set; } = string.Empty;
        
        [Display(Name = "Provider")]
        public string ProviderName { get; set; } = string.Empty;
        
        [Display(Name = "Pet")]
        public string PetName { get; set; } = string.Empty;
        
        [Display(Name = "Service Date")]
        public DateTime ServiceDate { get; set; }

        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please write a review")]
        [StringLength(1000, ErrorMessage = "Review must be between 10 and 1000 characters", MinimumLength = 10)]
        [Display(Name = "Your Review")]
        public string Comment { get; set; } = string.Empty;

        // Photo upload support
        [Display(Name = "Photos (Optional)")]
        public List<IFormFile> Photos { get; set; } = new List<IFormFile>();
        
        [Display(Name = "Photo Captions")]
        public List<string> PhotoCaptions { get; set; } = new List<string>();
    }

    public class EditReviewViewModel
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        
        [Display(Name = "Service")]
        public string ServiceName { get; set; } = string.Empty;
        
        [Display(Name = "Provider")]
        public string ProviderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please write a review")]
        [StringLength(1000, ErrorMessage = "Review must be between 10 and 1000 characters", MinimumLength = 10)]
        [Display(Name = "Your Review")]
        public string Comment { get; set; } = string.Empty;
    }

    public class ServiceReviewsViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public IEnumerable<PetCarePlatform.Core.Models.Review> Reviews { get; set; } = new List<PetCarePlatform.Core.Models.Review>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ReviewViewModel
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewItemViewModel
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanRespond { get; set; }
        public List<ReviewPhotoViewModel> Photos { get; set; } = new List<ReviewPhotoViewModel>();
    }

    public class ReviewPhotoViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ProviderResponseViewModel
    {
        public int ReviewId { get; set; }
        
        [Required(ErrorMessage = "Please write a response")]
        [StringLength(500, ErrorMessage = "Response must be between 10 and 500 characters", MinimumLength = 10)]
        [Display(Name = "Your Response")]
        public string Response { get; set; } = string.Empty;
        
        [Display(Name = "Reviewer")]
        public string ReviewerName { get; set; } = string.Empty;
        
        [Display(Name = "Service")]
        public string ServiceName { get; set; } = string.Empty;
        
        [Display(Name = "Rating")]
        public int Rating { get; set; }
        
        [Display(Name = "Review")]
        public string Comment { get; set; } = string.Empty;
    }
}