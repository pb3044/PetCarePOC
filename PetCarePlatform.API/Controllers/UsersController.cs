using Microsoft.AspNetCore.Mvc;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPetOwnerService _petOwnerService;
        private readonly IServiceProviderService _serviceProviderService;

        public UsersController(
            IUserService userService,
            IPetOwnerService petOwnerService,
            IServiceProviderService serviceProviderService)
        {
            _userService = userService;
            _petOwnerService = petOwnerService;
            _serviceProviderService = serviceProviderService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Create user
                var user = new ApplicationUser
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    City = request.City,
                    Province = request.Province,
                    PostalCode = request.PostalCode,
                    UserType = request.UserType
                };

                var createdUser = await _userService.RegisterUserAsync(user, request.Password);

                // Create profile based on user type
                if (request.UserType == UserType.PetOwner)
                {
                    var petOwner = new PetOwner
                    {
                        UserId = createdUser.Id,
                        PreferredServiceTypes = request.PreferredServiceTypes,
                        ReceiveMarketingEmails = request.ReceiveMarketingEmails
                    };

                    await _petOwnerService.CreatePetOwnerProfileAsync(petOwner);
                }
                else if (request.UserType == UserType.ServiceProvider)
                {
                    var serviceProvider = new Core.Models.ServiceProvider
                    {
                        UserId = createdUser.Id,
                        BusinessName = request.BusinessName,
                        Description = request.Description,
                        ServiceArea = request.ServiceArea,
                        ServiceRadius = request.ServiceRadius
                    };

                    await _serviceProviderService.CreateServiceProviderProfileAsync(serviceProvider);
                }

                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user fields
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                user.ProfilePhotoUrl = request.ProfilePhotoUrl;
                user.Address = request.Address;
                user.City = request.City;
                user.Province = request.Province;
                user.PostalCode = request.PostalCode;
                user.Latitude = request.Latitude;
                user.Longitude = request.Longitude;
                user.Bio = request.Bio;

                await _userService.UpdateUserProfileAsync(user);

                // Update profile based on user type
                if (user.UserType == UserType.PetOwner && request.PetOwnerProfile != null)
                {
                    var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(id);
                    if (petOwner != null)
                    {
                        petOwner.PreferredServiceTypes = request.PetOwnerProfile.PreferredServiceTypes;
                        petOwner.PreferredProviderAttributes = request.PetOwnerProfile.PreferredProviderAttributes;
                        petOwner.ReceiveMarketingEmails = request.PetOwnerProfile.ReceiveMarketingEmails;
                        petOwner.ReceiveNotifications = request.PetOwnerProfile.ReceiveNotifications;

                        await _petOwnerService.UpdatePetOwnerProfileAsync(petOwner);
                    }
                }
                else if (user.UserType == UserType.ServiceProvider && request.ServiceProviderProfile != null)
                {
                    var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(id);
                    if (serviceProvider != null)
                    {
                        serviceProvider.BusinessName = request.ServiceProviderProfile.BusinessName;
                        serviceProvider.Description = request.ServiceProviderProfile.Description;
                        serviceProvider.Credentials = request.ServiceProviderProfile.Credentials;
                        serviceProvider.Certifications = request.ServiceProviderProfile.Certifications;
                        serviceProvider.InsuranceInfo = request.ServiceProviderProfile.InsuranceInfo;
                        serviceProvider.LicenseInfo = request.ServiceProviderProfile.LicenseInfo;
                        serviceProvider.ServiceArea = request.ServiceProviderProfile.ServiceArea;
                        serviceProvider.ServiceRadius = request.ServiceProviderProfile.ServiceRadius;

                        await _serviceProviderService.UpdateServiceProviderProfileAsync(serviceProvider);
                    }
                }

                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService.RequestPasswordResetAsync(request.Email);
            return Ok(new { message = "If your email is registered, you will receive a password reset link." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
                return Ok(new { message = "Password has been reset successfully." });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/confirm-email")]
        public async Task<IActionResult> ConfirmEmail(int id, [FromQuery] string token)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                await _userService.ConfirmEmailAsync(user.Email, token);
                return Ok(new { message = "Email confirmed successfully." });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class RegisterUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public UserType UserType { get; set; }

        // Pet Owner specific fields
        public string PreferredServiceTypes { get; set; }
        public bool ReceiveMarketingEmails { get; set; }

        // Service Provider specific fields
        public string BusinessName { get; set; }
        public string Description { get; set; }
        public string ServiceArea { get; set; }
        public int ServiceRadius { get; set; }
    }

    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Bio { get; set; }

        // Profile specific updates
        public PetOwnerProfileUpdateRequest PetOwnerProfile { get; set; }
        public ServiceProviderProfileUpdateRequest ServiceProviderProfile { get; set; }
    }

    public class PetOwnerProfileUpdateRequest
    {
        public string PreferredServiceTypes { get; set; }
        public string PreferredProviderAttributes { get; set; }
        public bool ReceiveMarketingEmails { get; set; }
        public bool ReceiveNotifications { get; set; }
    }

    public class ServiceProviderProfileUpdateRequest
    {
        public string BusinessName { get; set; }
        public string Description { get; set; }
        public string Credentials { get; set; }
        public string Certifications { get; set; }
        public string InsuranceInfo { get; set; }
        public string LicenseInfo { get; set; }
        public string ServiceArea { get; set; }
        public int ServiceRadius { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
