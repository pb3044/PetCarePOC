using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class PetOwner
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PreferredServiceTypes { get; set; }
        public string PreferredProviderAttributes { get; set; }
        public bool ReceiveMarketingEmails { get; set; }
        public bool ReceiveNotifications { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Pet> Pets { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<ServiceProvider> FavoriteProviders { get; set; }
    }
}
