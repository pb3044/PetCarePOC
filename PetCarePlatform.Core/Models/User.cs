using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneConfirmed { get; set; }
        public bool IsActive { get; set; }
        public UserType UserType { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Bio { get; set; }
        
        // Navigation properties
        public virtual PetOwner PetOwner { get; set; }
        public virtual ServiceProvider ServiceProvider { get; set; }
        public virtual ICollection<Review> ReviewsGiven { get; set; }
        public virtual ICollection<Review> ReviewsReceived { get; set; }
        public virtual ICollection<Message> MessagesSent { get; set; }
        public virtual ICollection<Message> MessagesReceived { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }

    public enum UserType
    {
        PetOwner,
        ServiceProvider,
        Admin
    }
}
