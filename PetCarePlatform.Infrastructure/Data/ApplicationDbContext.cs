using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PetOwner> PetOwners { get; set; }
        public DbSet<ServiceProvider> ServiceProviders { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PetPhoto> PetPhotos { get; set; }
        public DbSet<ServicePhoto> ServicePhotos { get; set; }
        public DbSet<ReviewPhoto> ReviewPhotos { get; set; }
        public DbSet<AvailabilitySchedule> AvailabilitySchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure entity relationships and constraints

            // PetOwner
            builder.Entity<PetOwner>()
                .HasOne(po => po.User)
                .WithOne()
                .HasForeignKey<PetOwner>(po => po.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceProvider
            builder.Entity<ServiceProvider>()
                .HasOne(sp => sp.User)
                .WithOne()
                .HasForeignKey<ServiceProvider>(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Pet
            builder.Entity<Pet>()
                .HasOne(p => p.Owner)
                .WithMany(po => po.Pets)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Service
            builder.Entity<Service>()
                .HasOne(s => s.Provider)
                .WithMany(sp => sp.Services)
                .HasForeignKey(s => s.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking
            builder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Owner)
                .WithMany(po => po.Bookings)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Pet)
                .WithMany()
                .HasForeignKey(b => b.PetId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Review
            builder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithOne(b => b.Review)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Service)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message
            //builder.Entity<Message>()
            //    .HasOne(m => m.Sender)
            //    .WithMany(u => u.SentMessages)
            //    .HasForeignKey(m => m.SenderId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.Entity<Message>()
            //    .HasOne(m => m.Receiver)
            //    .WithMany(u => u.ReceivedMessages)
            //    .HasForeignKey(m => m.ReceiverId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //// Payment
            //builder.Entity<Payment>()
            //    .HasOne(p => p.Booking)
            //    .WithOne(b => b.Payment)
            //    .HasForeignKey<Payment>(p => p.BookingId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Notification
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Photos
            builder.Entity<PetPhoto>()
                .HasOne(pp => pp.Pet)
                .WithMany(p => p.Photos)
                .HasForeignKey(pp => pp.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ServicePhoto>()
                .HasOne(sp => sp.Service)
                .WithMany(s => s.Photos)
                .HasForeignKey(sp => sp.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReviewPhoto>()
                .HasOne(rp => rp.Review)
                .WithMany(r => r.Photos)
                .HasForeignKey(rp => rp.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // AvailabilitySchedule
            builder.Entity<AvailabilitySchedule>()
                .HasOne(a => a.Provider)
                .WithMany(sp => sp.AvailabilitySchedules)
                .HasForeignKey(a => a.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
