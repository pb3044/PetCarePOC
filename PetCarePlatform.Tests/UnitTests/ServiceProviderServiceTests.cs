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
    public class ServiceProviderServiceTests
    {
        private Mock<IServiceProviderRepository> _mockServiceProviderRepository;
        private IServiceProviderService _serviceProviderService;

        [TestInitialize]
        public void Setup()
        {
            _mockServiceProviderRepository = new Mock<IServiceProviderRepository>();
            _serviceProviderService = new ServiceProviderService(_mockServiceProviderRepository.Object);
        }

        [TestMethod]
        public async Task GetServiceProviderById_ExistingId_ReturnsServiceProvider()
        {
            // Arrange
            var providerId = 1;
            var expectedProvider = new ServiceProvider 
            { 
                Id = providerId, 
                UserId = 10,
                BusinessName = "Pet Care Pro",
                Description = "Professional pet care services",
                YearsOfExperience = 5,
                IsVerified = true
            };
            
            _mockServiceProviderRepository.Setup(repo => repo.GetByIdAsync(providerId))
                .ReturnsAsync(expectedProvider);

            // Act
            var result = await _serviceProviderService.GetServiceProviderByIdAsync(providerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedProvider.Id, result.Id);
            Assert.AreEqual(expectedProvider.UserId, result.UserId);
            Assert.AreEqual(expectedProvider.BusinessName, result.BusinessName);
            Assert.AreEqual(expectedProvider.Description, result.Description);
            Assert.AreEqual(expectedProvider.YearsOfExperience, result.YearsOfExperience);
            Assert.AreEqual(expectedProvider.IsVerified, result.IsVerified);
        }

        [TestMethod]
        public async Task GetServiceProviderById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var providerId = 999;
            
            _mockServiceProviderRepository.Setup(repo => repo.GetByIdAsync(providerId))
                .ReturnsAsync((ServiceProvider)null);

            // Act
            var result = await _serviceProviderService.GetServiceProviderByIdAsync(providerId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetServiceProviderByUserId_ExistingUserId_ReturnsServiceProvider()
        {
            // Arrange
            var userId = 10;
            var expectedProvider = new ServiceProvider 
            { 
                Id = 1, 
                UserId = userId,
                BusinessName = "Pet Care Pro",
                Description = "Professional pet care services",
                YearsOfExperience = 5,
                IsVerified = true
            };
            
            _mockServiceProviderRepository.Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(expectedProvider);

            // Act
            var result = await _serviceProviderService.GetServiceProviderByUserIdAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedProvider.Id, result.Id);
            Assert.AreEqual(expectedProvider.UserId, result.UserId);
            Assert.AreEqual(expectedProvider.BusinessName, result.BusinessName);
        }

        [TestMethod]
        public async Task CreateServiceProvider_ValidProvider_ReturnsCreatedProvider()
        {
            // Arrange
            var newProvider = new ServiceProvider 
            { 
                UserId = 10,
                BusinessName = "New Pet Care",
                Description = "New pet care services",
                YearsOfExperience = 3,
                IsVerified = false
            };
            
            var createdProvider = new ServiceProvider 
            { 
                Id = 1, 
                UserId = newProvider.UserId,
                BusinessName = newProvider.BusinessName,
                Description = newProvider.Description,
                YearsOfExperience = newProvider.YearsOfExperience,
                IsVerified = newProvider.IsVerified
            };
            
            _mockServiceProviderRepository.Setup(repo => repo.AddAsync(It.IsAny<ServiceProvider>()))
                .ReturnsAsync(createdProvider);

            // Act
            var result = await _serviceProviderService.CreateServiceProviderAsync(newProvider);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(createdProvider.Id, result.Id);
            Assert.AreEqual(createdProvider.UserId, result.UserId);
            Assert.AreEqual(createdProvider.BusinessName, result.BusinessName);
            Assert.AreEqual(createdProvider.Description, result.Description);
            Assert.AreEqual(createdProvider.YearsOfExperience, result.YearsOfExperience);
            Assert.AreEqual(createdProvider.IsVerified, result.IsVerified);
        }

        [TestMethod]
        public async Task UpdateServiceProvider_ExistingProvider_ReturnsUpdatedProvider()
        {
            // Arrange
            var providerId = 1;
            var existingProvider = new ServiceProvider 
            { 
                Id = providerId, 
                UserId = 10,
                BusinessName = "Pet Care Pro",
                Description = "Professional pet care services",
                YearsOfExperience = 5,
                IsVerified = true
            };
            
            var updatedProvider = new ServiceProvider 
            { 
                Id = providerId, 
                UserId = 10,
                BusinessName = "Updated Pet Care",
                Description = "Updated description",
                YearsOfExperience = 7,
                IsVerified = true
            };
            
            _mockServiceProviderRepository.Setup(repo => repo.GetByIdAsync(providerId))
                .ReturnsAsync(existingProvider);
                
            _mockServiceProviderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<ServiceProvider>()))
                .ReturnsAsync(updatedProvider);

            // Act
            var result = await _serviceProviderService.UpdateServiceProviderAsync(providerId, updatedProvider);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(updatedProvider.Id, result.Id);
            Assert.AreEqual(updatedProvider.BusinessName, result.BusinessName);
            Assert.AreEqual(updatedProvider.Description, result.Description);
            Assert.AreEqual(updatedProvider.YearsOfExperience, result.YearsOfExperience);
        }

        [TestMethod]
        public async Task GetTopRatedServiceProviders_ReturnsProviders()
        {
            // Arrange
            var providers = new List<ServiceProvider>
            {
                new ServiceProvider { Id = 1, BusinessName = "Top Provider 1", AverageRating = 4.9 },
                new ServiceProvider { Id = 2, BusinessName = "Top Provider 2", AverageRating = 4.8 },
                new ServiceProvider { Id = 3, BusinessName = "Top Provider 3", AverageRating = 4.7 }
            };
            
            _mockServiceProviderRepository.Setup(repo => repo.GetTopRatedProvidersAsync(It.IsAny<int>()))
                .ReturnsAsync(providers);

            // Act
            var result = await _serviceProviderService.GetTopRatedServiceProvidersAsync(3);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(providers[0].Id, result[0].Id);
            Assert.AreEqual(providers[1].Id, result[1].Id);
            Assert.AreEqual(providers[2].Id, result[2].Id);
        }

        [TestMethod]
        public async Task VerifyServiceProvider_ExistingProvider_ReturnsVerifiedProvider()
        {
            // Arrange
            var providerId = 1;
            var existingProvider = new ServiceProvider 
            { 
                Id = providerId, 
                UserId = 10,
                BusinessName = "Pet Care Pro",
                IsVerified = false
            };
            
            var verifiedProvider = new ServiceProvider 
            { 
                Id = providerId, 
                UserId = 10,
                BusinessName = "Pet Care Pro",
                IsVerified = true
            };
            
            _mockServiceProviderRepository.Setup(repo => repo.GetByIdAsync(providerId))
                .ReturnsAsync(existingProvider);
                
            _mockServiceProviderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<ServiceProvider>()))
                .ReturnsAsync(verifiedProvider);

            // Act
            var result = await _serviceProviderService.VerifyServiceProviderAsync(providerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsVerified);
        }
    }
}
