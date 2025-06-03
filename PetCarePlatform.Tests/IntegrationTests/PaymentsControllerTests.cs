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
    public class PaymentsControllerTests
    {
        private Mock<IPaymentService> _mockPaymentService;
        private Mock<IBookingService> _mockBookingService;
        private PaymentsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _mockBookingService = new Mock<IBookingService>();
            _controller = new PaymentsController(_mockPaymentService.Object, _mockBookingService.Object);
        }

        [TestMethod]
        public async Task CreatePaymentIntent_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var bookingId = 1;
            var booking = new Booking
            {
                Id = bookingId,
                TotalAmount = 25.00m,
                Status = BookingStatus.Pending,
                PetOwnerId = 2
            };
            
            var paymentIntentRequest = new PaymentIntentRequest
            {
                BookingId = bookingId,
                CustomerId = "cus_test123"
            };
            
            var paymentIntentResponse = new PaymentIntentResponse
            {
                ClientSecret = "pi_test_secret",
                PaymentIntentId = "pi_test123"
            };
            
            _mockBookingService.Setup(service => service.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);
                
            _mockPaymentService.Setup(service => service.CreatePaymentIntentAsync(
                booking.TotalAmount, "cad", paymentIntentRequest.CustomerId))
                .ReturnsAsync(paymentIntentResponse);

            // Act
            var result = await _controller.CreatePaymentIntent(paymentIntentRequest);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var response = okResult.Value as PaymentIntentResponse;
            Assert.IsNotNull(response);
            Assert.AreEqual(paymentIntentResponse.ClientSecret, response.ClientSecret);
            Assert.AreEqual(paymentIntentResponse.PaymentIntentId, response.PaymentIntentId);
        }

        [TestMethod]
        public async Task CreatePaymentIntent_InvalidBookingId_ReturnsNotFound()
        {
            // Arrange
            var bookingId = 999;
            
            var paymentIntentRequest = new PaymentIntentRequest
            {
                BookingId = bookingId,
                CustomerId = "cus_test123"
            };
            
            _mockBookingService.Setup(service => service.GetBookingByIdAsync(bookingId))
                .ReturnsAsync((Booking)null);

            // Act
            var result = await _controller.CreatePaymentIntent(paymentIntentRequest);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ProcessRefund_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var bookingId = 1;
            var paymentIntentId = "pi_test123";
            
            var booking = new Booking
            {
                Id = bookingId,
                TotalAmount = 25.00m,
                Status = BookingStatus.Confirmed,
                PaymentIntentId = paymentIntentId
            };
            
            var refundRequest = new RefundRequest
            {
                BookingId = bookingId,
                Reason = "Customer requested"
            };
            
            var refundResponse = new RefundResponse
            {
                RefundId = "re_test123",
                Amount = 25.00m,
                Status = "succeeded"
            };
            
            _mockBookingService.Setup(service => service.GetBookingByIdAsync(bookingId))
                .ReturnsAsync(booking);
                
            _mockPaymentService.Setup(service => service.ProcessRefundAsync(
                paymentIntentId, booking.TotalAmount))
                .ReturnsAsync(refundResponse);
                
            _mockBookingService.Setup(service => service.CancelBookingAsync(bookingId))
                .ReturnsAsync(new Booking { Id = bookingId, Status = BookingStatus.Cancelled });

            // Act
            var result = await _controller.ProcessRefund(refundRequest);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var response = okResult.Value as RefundResponse;
            Assert.IsNotNull(response);
            Assert.AreEqual(refundResponse.RefundId, response.RefundId);
            Assert.AreEqual(refundResponse.Amount, response.Amount);
            Assert.AreEqual(refundResponse.Status, response.Status);
        }

        [TestMethod]
        public async Task ProcessRefund_InvalidBookingId_ReturnsNotFound()
        {
            // Arrange
            var bookingId = 999;
            
            var refundRequest = new RefundRequest
            {
                BookingId = bookingId,
                Reason = "Customer requested"
            };
            
            _mockBookingService.Setup(service => service.GetBookingByIdAsync(bookingId))
                .ReturnsAsync((Booking)null);

            // Act
            var result = await _controller.ProcessRefund(refundRequest);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task CreateCustomer_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var customerRequest = new CustomerRequest
            {
                Email = "test@example.com",
                Name = "Test User"
            };
            
            var customerResponse = new CustomerResponse
            {
                CustomerId = "cus_test123"
            };
            
            _mockPaymentService.Setup(service => service.CreateCustomerAsync(
                customerRequest.Email, customerRequest.Name))
                .ReturnsAsync(customerResponse);

            // Act
            var result = await _controller.CreateCustomer(customerRequest);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var response = okResult.Value as CustomerResponse;
            Assert.IsNotNull(response);
            Assert.AreEqual(customerResponse.CustomerId, response.CustomerId);
        }
    }
}
