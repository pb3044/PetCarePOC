using System;

namespace PetCareBooking.Core.Models
{
    public class TaxRate
    {
        public int Id { get; set; }
        public string Province { get; set; }
        public string ProvinceCode { get; set; }
        public decimal GstRate { get; set; }
        public decimal PstRate { get; set; }
        public decimal HstRate { get; set; }
        public bool HasHst { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
