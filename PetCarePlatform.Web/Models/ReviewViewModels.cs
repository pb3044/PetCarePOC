using System.ComponentModel.DataAnnotations;

namespace PetCarePlatform.Web.Models
{
    public class CreateReviewViewModel
    {
        public int BookingId { get; set; }
        public string ServiceName { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Comment { get; set; }
    }

    public class EditReviewViewModel
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Comment { get; set; }
    }

    public class ReviewViewModel
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string ServiceName { get; set; }
        public string ReviewerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

