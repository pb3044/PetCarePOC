using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class ReviewPhoto
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public string Url { get; set; }
        public string Caption { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public virtual Review Review { get; set; }
    }
}
