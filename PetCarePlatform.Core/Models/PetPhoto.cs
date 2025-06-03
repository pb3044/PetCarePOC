using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class PetPhoto
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public string Url { get; set; }
        public string Caption { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public virtual Pet Pet { get; set; }
    }
}
