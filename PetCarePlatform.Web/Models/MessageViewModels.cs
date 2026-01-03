using System.ComponentModel.DataAnnotations;

namespace PetCarePlatform.Web.Models
{
    public class ComposeMessageViewModel
    {
        [Required]
        public int ReceiverId { get; set; }

        public int? BookingId { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }
}

