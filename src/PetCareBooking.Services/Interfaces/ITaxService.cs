using PetCareBooking.Core.Models;
using System;
using System.Threading.Tasks;

namespace PetCareBooking.Services.Interfaces
{
    public interface ITaxService
    {
        Task<TaxCalculationResult> CalculateTaxAsync(decimal amount, string provinceCode);
    }

    public class TaxCalculationResult
    {
        public decimal SubTotal { get; set; }
        public decimal GstAmount { get; set; }
        public decimal PstAmount { get; set; }
        public decimal HstAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
