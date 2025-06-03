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
    public class LocationControllerTests
    {
        private Mock<ILocationService> _mockLocationService;
        private LocationController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockLocationService = new Mock<ILocationService>();
            _controller = new LocationController(_mockLocationService.Object);
        }

        [TestMethod]
        public async Task GeocodeAddress_ValidAddress_ReturnsOkResult()
        {
            // Arrange
            var address = "123 Main St, Toronto, ON, Canada";
            var geocodingResult = new GeocodingResult
            {
                FormattedAddress = "123 Main St, Toronto, ON M5V 2K7, Canada",
                Latitude = 43.6532,
                Longitude = -79.3832,
                PlaceId = "ChIJpTvG15DL1IkRd8S0KlBVNTI"
            };
            
            _mockLocationService.Setup(service => service.GeocodeAddressAsync(address))
                .ReturnsAsync(geocodingResult);

            // Act
            var result = await _controller.GeocodeAddress(address);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var returnedResult = okResult.Value as GeocodingResult;
            Assert.IsNotNull(returnedResult);
            Assert.AreEqual(geocodingResult.FormattedAddress, returnedResult.FormattedAddress);
            Assert.AreEqual(geocodingResult.Latitude, returnedResult.Latitude);
            Assert.AreEqual(geocodingResult.Longitude, returnedResult.Longitude);
        }

        [TestMethod]
        public async Task GeocodeAddress_EmptyAddress_ReturnsBadRequest()
        {
            // Arrange
            string address = "";

            // Act
            var result = await _controller.GeocodeAddress(address);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task CalculateDistance_ValidCoordinates_ReturnsOkResult()
        {
            // Arrange
            double lat1 = 43.6532;
            double lng1 = -79.3832;
            double lat2 = 43.7615;
            double lng2 = -79.4111;
            double distance = 12.5;
            
            _mockLocationService.Setup(service => service.CalculateDistanceAsync(lat1, lng1, lat2, lng2))
                .ReturnsAsync(distance);

            // Act
            var result = await _controller.CalculateDistance(lat1, lng1, lat2, lng2);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var returnedResult = okResult.Value as dynamic;
            Assert.IsNotNull(returnedResult);
            Assert.AreEqual(distance, returnedResult.distanceKm);
        }

        [TestMethod]
        public async Task FindNearbyProviders_ValidParameters_ReturnsOkResult()
        {
            // Arrange
            double latitude = 43.6532;
            double longitude = -79.3832;
            double radiusKm = 10;
            string serviceType = "Dog Walking";
            
            var providers = new List<NearbyServiceProvider>
            {
                new NearbyServiceProvider
                {
                    ProviderId = 1,
                    ProviderName = "Provider 1",
                    Latitude = 43.6600,
                    Longitude = -79.3900,
                    DistanceKm = 1.2,
                    Rating = 4.8
                },
                new NearbyServiceProvider
                {
                    ProviderId = 2,
                    ProviderName = "Provider 2",
                    Latitude = 43.6700,
                    Longitude = -79.3700,
                    DistanceKm = 2.5,
                    Rating = 4.5
                }
            };
            
            _mockLocationService.Setup(service => service.FindNearbyServiceProvidersAsync(
                latitude, longitude, radiusKm, serviceType))
                .ReturnsAsync(providers);

            // Act
            var result = await _controller.FindNearbyProviders(latitude, longitude, radiusKm, serviceType);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var returnedProviders = okResult.Value as List<NearbyServiceProvider>;
            Assert.IsNotNull(returnedProviders);
            Assert.AreEqual(2, returnedProviders.Count);
        }

        [TestMethod]
        public void GetStaticMapUrl_ValidParameters_ReturnsOkResult()
        {
            // Arrange
            double latitude = 43.6532;
            double longitude = -79.3832;
            int zoom = 14;
            int width = 600;
            int height = 400;
            
            string mapUrl = "https://maps.googleapis.com/maps/api/staticmap?center=43.6532,-79.3832&zoom=14&size=600x400&markers=color:red%7C43.6532,-79.3832&key=test_api_key";
            
            _mockLocationService.Setup(service => service.GenerateStaticMapUrl(
                latitude, longitude, zoom, width, height))
                .Returns(mapUrl);

            // Act
            var result = _controller.GetStaticMapUrl(latitude, longitude, zoom, width, height);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var returnedResult = okResult.Value as dynamic;
            Assert.IsNotNull(returnedResult);
            Assert.AreEqual(mapUrl, returnedResult.mapUrl);
        }

        [TestMethod]
        public void GetDirectionsUrl_ValidParameters_ReturnsOkResult()
        {
            // Arrange
            string origin = "123 Main St, Toronto, ON";
            string destination = "456 Queen St, Toronto, ON";
            
            string directionsUrl = "https://www.google.com/maps/dir/?api=1&origin=123%20Main%20St%2C%20Toronto%2C%20ON&destination=456%20Queen%20St%2C%20Toronto%2C%20ON&travelmode=driving";
            
            _mockLocationService.Setup(service => service.GenerateDirectionsUrl(origin, destination))
                .Returns(directionsUrl);

            // Act
            var result = _controller.GetDirectionsUrl(origin, destination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            
            var returnedResult = okResult.Value as dynamic;
            Assert.IsNotNull(returnedResult);
            Assert.AreEqual(directionsUrl, returnedResult.directionsUrl);
        }

        [TestMethod]
        public void GetDirectionsUrl_EmptyParameters_ReturnsBadRequest()
        {
            // Arrange
            string origin = "";
            string destination = "";

            // Act
            var result = _controller.GetDirectionsUrl(origin, destination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }
    }
}
