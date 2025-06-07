using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class EditProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class SearchUsersViewModel
    {
        public string SearchTerm { get; set; }
        public UserType? UserType { get; set; }
        public IEnumerable<ApplicationUser> Results { get; set; } = new List<ApplicationUser>();
    }

    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; }
        public PetOwner PetOwner { get; set; }
        public PetCarePlatform.Core.Models.ServiceProvider ServiceProvider { get; set; }
        public IEnumerable<Pet> Pets { get; set; } = new List<Pet>();
        public IEnumerable<Service> Services { get; set; } = new List<Service>();
    }
}

