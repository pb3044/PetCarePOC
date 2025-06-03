using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Core.Services;

namespace PetCarePlatform.Tests.UnitTests
{
    [TestClass]
    public class BookingServiceTests
    {
        private Mock<IBookingRepository> _mockBookingRepository;
        private Mock<IServiceRepository> _mockServiceRepository;
        private Mock<IPetOwnerRepository> _mockPetOwnerRepository;
        private Mock<IServiceProviderRepository> _mockServiceProviderRepository;
        private Mock<INotificationService> _mockNotificationService;
        private IBookingService _bookingService;

        [TestInitialize]
        public void Setup()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockServiceRepository = new Mock<IServiceRepository>();
            _mockPetOwnerRepository = new Mock<IPetOwnerRepository>();
            _mockServiceProviderRepository = new Mock<IServiceProviderRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            
            _bookingService = new BookingService(
                _mockBookingRepository.Object,
                _mockServiceRepository.Object,
                _mockPetOwnerRepository.Object,
                _mockServiceProviderRepository.Object,
                _mockNotificationService.Object);
        }

        [TestMethod]
        public async Task CreateBooking_ValidBooking_ReturnsCreatedBooking()
        {
            // Arrange
            var newBooking = new Booking
            {
                PetOwnerId = 1,
                ServiceId = 2,
                ServiceProviderId = 3,
                PetId = 4,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(1),
                Status = BookingStatus.Pending,
                TotalAmount = 25.00m
            };
            
            var createdBooking = new Booking
            {
                Id = 1,
                PetOwnerId = newBooking.PetOwnerId,
                ServiceId = newBooking.ServiceId,
                ServiceProviderId = newBooking.ServiceProviderId,
                PetId = newBooking.PetId,
                StartTime = newBooking.StartTime,
                EndTime = newBooking.EndTime,
                Status = newBooking.Status,
                TotalAmount = newBooking.TotalAmount,
                CreatedAt = DateTime.Now
            };
            
            _mockServiceRepository.Setup(repo => repo.GetByIdAsync(newBooking.ServiceId))
                .ReturnsAsync(new Service { Id = newBooking.ServiceId, IsActive = true });
                
            _mockPetOwnerRepository.Setup(repo => repo.GetByIdAsync(newBooking.PetOwnerId))
                .ReturnsAsync(new PetOwner { Id = newBooking.PetOwnerId });
                
            _mockServiceProviderRepository.Setup(repo => repo.GetByIdAsync(newBooking.ServiceProviderId))
                .ReturnsAsync(new ServiceProvider { Id = newBooking.ServiceProviderId, IsActive = true });
                
            _mockBookingRepository.Setup(repo => repo.AddAsync(It.IsAny<Booking>()))
                .ReturnsAsync(createdBooking);

            // Act
            var result = await _bookingService.CreateBookingAsync(newBooking);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(createdBooking.Id, result.Id);
            Assert.AreEqual(createdBooking.PetOwnerId, result.PetOwnerId);
            Assert.AreEqual(createdBooking.ServiceId, result.ServiceId);
            Assert.AreEqual(createdBooking.ServiceProviderId, result.ServiceProviderId);
            Assert.AreEqual(createdBooking.Status, result.Status);
            Assert.AreEqual(createdBooking.TotalAmount, result.TotalAmount);
        }

        [TestMethod]
        public async Task GetBookingById_ExistingId_ReturnsBooking()
        {
            // Arrange
            var bookingId = 1;
            var expectedBooking = new Booking
            {
                Id = bookingId,
                PetOwnerId = 1,
                ServiceId = 2,
                ServiceProviderId = 3,
                PetId = 4,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(1),
                Status = BookingStatus.Confirmed,
                TotalAmount = 25.00m
            };
            
            _mockBookingRepository.Setup(repo => repo.GetByIdAsync(bookingId))
                .ReturnsAsync(expectedBooking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(bookingId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedBooking.Id, result.Id);
            Assert.AreEqual(expectedBooking.Status, result.Status);
        }

        [TestMethod]
        public async Task GetBookingsByPetOwner_ReturnsBookings()
        {
            // Arrange
            var petOwnerId = 1;
            var bookings = new List<Booking>
            {
                new Booking { Id = 1, PetOwnerId = petOwnerId, Status = BookingStatus.Confirmed },
                new Booking { Id = 2, PetOwnerId = petOwnerId, Status = BookingStatus.Completed },
                new Booking { Id = 3, PetOwnerId = petOwnerId, Status = BookingStatus.Pending }
            };
            
            _mockBookingRepository.Setup(repo => repo.GetByPetOwnerIdAsync(petOwnerId))
                .ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetBookingsByPetOwnerIdAsync(petOwnerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetBookingsByServiceProvider_ReturnsBookings()
        {
            // Arrange
            var serviceProviderId = 3;
            var bookings = new List<Booking>
            {
                new Booking { Id = 1, ServiceProviderId = serviceProviderId, Status = BookingStatus.Confirmed },
                new Booking { Id = 2, ServiceProviderId = serviceProviderId, Status = BookingStatus.Completed }
            };
            
            _mockBookingRepository.Setup(repo => repo.GetByServiceProviderIdAsync(serviceProviderId))
                .ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetBookingsByServiceProviderIdAsync(serviceProviderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task ConfirmBooking_PendingBooking_ReturnsConfirmedBooking()
        {
            // Arrange
            var bookingId = 1;
            var pendingBooking = new Booking
            {
                Id = bookingId,
                Status = BookingStatus.Pending,
                ServiceProviderId = 3,
                PetOwnerId = 1
            };
            
            var confirmedBooking = new Booking
            {
                Id = bookingId,
                Status = BookingStatus.Confirmed,
                ServiceProviderId = 3,
                PetOwnerId = 1
            };
            
            _mockBookingRepository.Setup(repo => repo.GetByIdAsync(bookingId))
                .ReturnsAsync(pendingBooking);
                
            _mockBookingRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Booking>()))
                .ReturnsAsync(confirmedBooking);

            // Act
            var result = await _bookingService.ConfirmBookingAsync(bookingId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(BookingStatus.Confirmed, result.Status);
        }

        [TestMethod]
        public async Task CancelBooking_ConfirmedBooking_ReturnsCancelledBooking()
        {
            // Arrange
            var bookingId = 1;
            var confirmedBooking = new Booking
            {
                Id = bookingId,
                Status = BookingStatus.Confirmed,
                ServiceProviderId = 3,
                PetOwnerId = 1
            };
            
            var cancelledBooking = new Booking
            {
                Id = bookingId,
                Status = BookingStatus.Cancelled,
                ServiceProviderId = 3,
                PetOwnerId = 1
            };
            
            _mockBookingRepository.Setup(repo => repo.GetByIdAsync(bookingId))
                .ReturnsAsync(confirmedBooking);
                
            _mockBookingRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Booking>()))
                .ReturnsAsync(cancelledBooking);

            // Act
            var result = await _bookingService.CancelBookingAsync(bookingId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(BookingStatus.Cancelled, result.Status);
        }

        [TestMethod]
        public async Task CompleteBooking_ConfirmedBooking_ReturnsCompletedBooking()
        {
            // Arrange
            var bookingId = 1;
            var confirmedBooking = new Booking
            {
                Id = bookingId,
                Status = BookingStatus.Confirmed,
                ServiceProviderId = 3,
                PetOwnerId = 1
            };
            
            var completedBooking = new Booking
            {
                Id = bookingId,
                Status = BookingStatus.Completed,
                ServiceProviderId = 3,
                PetOwnerId = 1
            };
            
            _mockBookingRepository.Setup(repo => repo.GetByIdAsync(bookingId))
                .ReturnsAsync(confirmedBooking);
                
            _mockBookingRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Booking>()))
                .ReturnsAsync(completedBooking);

            // Act
            var result = await _bookingService.CompleteBookingAsync(bookingId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(BookingStatus.Completed, result.Status);
        }
    }
}
