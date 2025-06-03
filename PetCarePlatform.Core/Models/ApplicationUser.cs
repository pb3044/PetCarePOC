using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Bio { get; set; }
        public UserType UserType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Message> SentMessages { get; set; }
        public virtual ICollection<Message> ReceivedMessages { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }

    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() : base() { }
        
        public ApplicationRole(string roleName) : base(roleName) { }
        
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
