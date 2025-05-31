using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Core.Models;

namespace PetCareBooking.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<TaxRate> TaxRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Service>()
                .HasOne(s => s.Provider)
                .WithMany()
                .HasForeignKey(s => s.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Pet)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Provider)
                .WithMany()
                .HasForeignKey(b => b.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Canadian tax rates
            SeedCanadianTaxRates(modelBuilder);
        }

        private void SeedCanadianTaxRates(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaxRate>().HasData(
                new TaxRate
                {
                    Id = 1,
                    Province = "Alberta",
                    ProvinceCode = "AB",
                    GstRate = 0.05m,
                    PstRate = 0.00m,
                    HstRate = 0.00m,
                    HasHst = false,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 2,
                    Province = "British Columbia",
                    ProvinceCode = "BC",
                    GstRate = 0.05m,
                    PstRate = 0.07m,
                    HstRate = 0.00m,
                    HasHst = false,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 3,
                    Province = "Manitoba",
                    ProvinceCode = "MB",
                    GstRate = 0.05m,
                    PstRate = 0.07m,
                    HstRate = 0.00m,
                    HasHst = false,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 4,
                    Province = "New Brunswick",
                    ProvinceCode = "NB",
                    GstRate = 0.00m,
                    PstRate = 0.00m,
                    HstRate = 0.15m,
                    HasHst = true,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 5,
                    Province = "Newfoundland and Labrador",
                    ProvinceCode = "NL",
                    GstRate = 0.00m,
                    PstRate = 0.00m,
                    HstRate = 0.15m,
                    HasHst = true,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 6,
                    Province = "Northwest Territories",
                    ProvinceCode = "NT",
                    GstRate = 0.05m,
                    PstRate = 0.00m,
                    HstRate = 0.00m,
                    HasHst = false,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 7,
                    Province = "Nova Scotia",
                    ProvinceCode = "NS",
                    GstRate = 0.00m,
                    PstRate = 0.00m,
                    HstRate = 0.15m,
                    HasHst = true,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 8,
                    Province = "Nunavut",
                    ProvinceCode = "NU",
                    GstRate = 0.05m,
                    PstRate = 0.00m,
                    HstRate = 0.00m,
                    HasHst = false,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 9,
                    Province = "Ontario",
                    ProvinceCode = "ON",
                    GstRate = 0.00m,
                    PstRate = 0.00m,
                    HstRate = 0.13m,
                    HasHst = true,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 10,
                    Province = "Prince Edward Island",
                    ProvinceCode = "PE",
                    GstRate = 0.00m,
                    PstRate = 0.00m,
                    HstRate = 0.15m,
                    HasHst = true,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 11,
                    Province = "Quebec",
                    ProvinceCode = "QC",
                    GstRate = 0.05m,
                    PstRate = 0.09975m,
                    HstRate = 0.00m,
                    HasHst = false,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 12,
                    Province = "Saskatchewan",
                    ProvinceCode = "SK",
                    GstRate = 0.05m,
                    PstRate = 0.06m,
                    HstRate = 0.00m,
                    HasHst = false,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                },
                new TaxRate
                {
                    Id = 13,
                    Province = "Yukon",
                    ProvinceCode = "YT",
                    GstRate = 0.05m,
                    PstRate = 0.00m,
                    HstRate = 0.00m,
                    HasHst = false,
                    EffectiveDate = new DateTime(2023, 1, 1),
                    IsActive = true
                }
            );
        }
    }
}
