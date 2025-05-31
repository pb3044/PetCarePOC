using Microsoft.EntityFrameworkCore;
using PetCareBooking.Core.Models;
using PetCareBooking.Infrastructure.Data;
using PetCareBooking.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace PetCareBooking.Services.Implementations
{
    public class TaxService : ITaxService
    {
        private readonly ApplicationDbContext _context;

        public TaxService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaxCalculationResult> CalculateTaxAsync(decimal amount, string provinceCode)
        {
            if (string.IsNullOrEmpty(provinceCode))
            {
                throw new ArgumentException("Province code cannot be null or empty", nameof(provinceCode));
            }

            var taxRate = await _context.TaxRates
                .FirstOrDefaultAsync(tr => tr.ProvinceCode == provinceCode && tr.IsActive);

            if (taxRate == null)
            {
                throw new InvalidOperationException($"No active tax rate found for province code: {provinceCode}");
            }

            var result = new TaxCalculationResult
            {
                SubTotal = amount
            };

            if (taxRate.HasHst)
            {
                // HST provinces (ON, NB, NS, NL, PE)
                result.HstAmount = Math.Round(amount * taxRate.HstRate, 2);
                result.GstAmount = 0;
                result.PstAmount = 0;
            }
            else
            {
                // GST + PST provinces
                result.GstAmount = Math.Round(amount * taxRate.GstRate, 2);
                result.PstAmount = Math.Round(amount * taxRate.PstRate, 2);
                result.HstAmount = 0;
            }

            result.TotalAmount = result.SubTotal + result.GstAmount + result.PstAmount + result.HstAmount;
            return result;
        }
    }
}
