using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.ViewModels
{
    public class PetOwnerDashboardViewModel
    {       
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public List<Pet> Pets { get; set; } = new();
        public List<Booking> RecentBookings { get; set; } = new();
        public List<Core.Models.ServiceProvider> FavoriteProviders { get; set; } = new();

        public int TotalPets => Pets?.Count ?? 0;
        public int TotalBookings => RecentBookings?.Count ?? 0;

        public int RequestedBookings => CountByStatus(BookingStatus.Requested);
        public int ConfirmedBookings => CountByStatus(BookingStatus.Confirmed);
        public int InProgressBookings => CountByStatus(BookingStatus.InProgress);
        public int CompletedBookings => CountByStatus(BookingStatus.Completed);
        public int CancelledBookings => CountByStatus(BookingStatus.Cancelled);
        public int DeclinedBookings => CountByStatus(BookingStatus.Declined);

        public int UpcomingBookings =>
            RecentBookings?.Count(b => b.StartTime >= DateTime.UtcNow &&
                                       (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress)) ?? 0;

        public int PendingRequests => RequestedBookings;

        public double AverageRating =>
            FavoriteProviders != null && FavoriteProviders.Any()
                ? Math.Round(FavoriteProviders.Average(p => p.AverageRating), 1)
                : 0;

        private int CountByStatus(BookingStatus status) =>
            RecentBookings?.Count(b => b.Status == status) ?? 0;
    }
}