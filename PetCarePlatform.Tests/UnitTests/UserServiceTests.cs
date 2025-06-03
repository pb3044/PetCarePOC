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
    public class UserServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private IUserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [TestMethod]
        public async Task GetUserById_ExistingId_ReturnsUser()
        {
            // Arrange
            var userId = 1;
            var expectedUser = new User { Id = userId, Email = "test@example.com", FirstName = "Test", LastName = "User" };
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser.Id, result.Id);
            Assert.AreEqual(expectedUser.Email, result.Email);
            Assert.AreEqual(expectedUser.FirstName, result.FirstName);
            Assert.AreEqual(expectedUser.LastName, result.LastName);
        }

        [TestMethod]
        public async Task GetUserById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var userId = 999;
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateUser_ValidUser_ReturnsCreatedUser()
        {
            // Arrange
            var newUser = new User 
            { 
                Email = "new@example.com", 
                FirstName = "New", 
                LastName = "User",
                PhoneNumber = "1234567890"
            };
            
            var createdUser = new User 
            { 
                Id = 1, 
                Email = newUser.Email, 
                FirstName = newUser.FirstName, 
                LastName = newUser.LastName,
                PhoneNumber = newUser.PhoneNumber
            };
            
            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _userService.CreateUserAsync(newUser);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(createdUser.Id, result.Id);
            Assert.AreEqual(createdUser.Email, result.Email);
            Assert.AreEqual(createdUser.FirstName, result.FirstName);
            Assert.AreEqual(createdUser.LastName, result.LastName);
            Assert.AreEqual(createdUser.PhoneNumber, result.PhoneNumber);
        }

        [TestMethod]
        public async Task UpdateUser_ExistingUser_ReturnsUpdatedUser()
        {
            // Arrange
            var userId = 1;
            var existingUser = new User 
            { 
                Id = userId, 
                Email = "test@example.com", 
                FirstName = "Test", 
                LastName = "User",
                PhoneNumber = "1234567890"
            };
            
            var updatedUser = new User 
            { 
                Id = userId, 
                Email = "updated@example.com", 
                FirstName = "Updated", 
                LastName = "User",
                PhoneNumber = "0987654321"
            };
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
                
            _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updatedUser);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(updatedUser.Id, result.Id);
            Assert.AreEqual(updatedUser.Email, result.Email);
            Assert.AreEqual(updatedUser.FirstName, result.FirstName);
            Assert.AreEqual(updatedUser.LastName, result.LastName);
            Assert.AreEqual(updatedUser.PhoneNumber, result.PhoneNumber);
        }

        [TestMethod]
        public async Task DeleteUser_ExistingUser_ReturnsTrue()
        {
            // Arrange
            var userId = 1;
            var existingUser = new User { Id = userId, Email = "test@example.com" };
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
                
            _mockUserRepository.Setup(repo => repo.DeleteAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteUser_NonExistingUser_ReturnsFalse()
        {
            // Arrange
            var userId = 999;
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
