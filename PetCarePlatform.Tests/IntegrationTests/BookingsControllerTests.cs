using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PetCarePlatform.API.Controllers;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Tests.IntegrationTests
{
    [TestClass]
    public class BookingsControllerTests
    {
        private Mock<IBookingService> _mockBookingService;
        private BookingsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockBookingService = new Mock<IBookingService>();
            _controller = new BookingsController(_mockBookingService.Object);
        }

        [TestMethod]
        public async Task GetBooking_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var bookingId = 1;
            var booking = new Booking
            {
                Id = bookingId,
                PetOwnerId = 1,
                ServiceId = 2,
                ServiceProviderId = 3,
                Status = BookingStatus.Confirmed
            };
            
            _mockBookingService.Setup(service => service.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);

            // Act
            var result = await _controller.GetBooking(bookingId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var returnedBooking = okResult.Value as Booking;
            Assert.IsNotNull(returnedBooking);
            Assert.AreEqual(bookingId, returnedBooking.Id);
        }

        [TestMethod]
        public async Task GetBooking_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var bookingId = 999;
            
            _mockBookingService.Setup(service => service.GetBookingByIdAsync(bookingId))
                .ReturnsAsync((Booking)null);

            // Act
            var result = await _controller.GetBooking(bookingId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task CreateBooking_ValidBooking_ReturnsCreatedAtAction()
        {
            // Arrange
            var newBooking = new Booking
            {
                PetOwnerId = 1,
                ServiceId = 2,
                ServiceProviderId = 3,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(1)
            };
            
            var createdBooking = new Booking
            {
                Id = 1,
                PetOwnerId = newBooking.PetOwnerId,
                ServiceId = newBooking.ServiceId,
                ServiceProviderId = newBooking.ServiceProviderId,
                StartTime = newBooking.StartTime,
                EndTime = newBooking.EndTime,
                Status = BookingStatus.Pending
            };
            
            _mockBookingService.Setup(service => service.CreateBookingAsync(It.IsAny<Booking>()))
                .ReturnsAsync(createdBooking);

            // Act
            var result = await _controller.CreateBooking(newBooking);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            
            var returnedBooking = createdResult.Value as Booking;
            Assert.IsNotNull(returnedBooking);
            Assert.AreEqual(createdBooking.Id, returnedBooking.Id);
        }

        [TestMethod]
        public async Task ConfirmBooking_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var bookingId = 1;
            var confirmedBooking = new Booking
            {
                Id = bookingId,
                Status = BookingStatus.Confirmed
            };
            
            _mockBookingService.Setup(service => service.ConfirmBookingAsync(bookingId))
                .ReturnsAsync(confirmedBooking);

            // Act
            var result = await _controller.ConfirmBooking(bookingId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var returnedBooking = okResult.Value as Booking;
            Assert.IsNotNull(returnedBooking);
            Assert.AreEqual(BookingStatus.Confirmed, returnedBooking.Status);
        }

        [TestMethod]
        public async Task CancelBooking_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var bookingId = 1;
            var cancelledBooking = new Booking
            {
                Id = bookingId,
                Status = BookingStatus.Cancelled
            };
            
            _mockBookingService.Setup(service => service.CancelBookingAsync(bookingId))
                .ReturnsAsync(cancelledBooking);

            // Act
            var result = await _controller.CancelBooking(bookingId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var returnedBooking = okResult.Value as Booking;
            Assert.IsNotNull(returnedBooking);
            Assert.AreEqual(BookingStatus.Cancelled, returnedBooking.Status);
        }

        [TestMethod]
        public async Task GetBookingsByPetOwner_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var petOwnerId = 1;
            var bookings = new List<Booking>
            {
                new Booking { Id = 1, PetOwnerId = petOwnerId },
                new Booking { Id = 2, PetOwnerId = petOwnerId }
            };
            
            _mockBookingService.Setup(service => service.GetBookingsByPetOwnerIdAsync(petOwnerId))
                .ReturnsAsync(bookings);

            // Act
            var result = await _controller.GetBookingsByPetOwner(petOwnerId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var returnedBookings = okResult.Value as List<Booking>;
            Assert.IsNotNull(returnedBookings);
            Assert.AreEqual(2, returnedBookings.Count);
        }
    }
}
