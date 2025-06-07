using System.ComponentModel.DataAnnotations;

namespace PetCarePlatform.Web.Models
{
    public class ComposeMessageViewModel
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }
    }
}

