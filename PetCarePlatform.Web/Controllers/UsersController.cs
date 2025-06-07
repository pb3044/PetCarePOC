using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Web.Models;
using System.Security.Claims;

namespace PetCarePlatform.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
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

        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                User = user
            };

            // Load additional data based on user type
            if (User.IsInRole("PetOwner"))
            {
                model.PetOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            }
            else if (User.IsInRole("ServiceProvider"))
            {
                model.ServiceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userId);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Province = user.Province,
                PostalCode = user.PostalCode
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user properties
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.City = model.City;
                user.Province = model.Province;
                user.PostalCode = model.PostalCode;
                user.UpdatedAt = DateTime.UtcNow;

                await _userService.UpdateUserProfileAsync(user);
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating your profile: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while changing your password: " + ex.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> PetOwnerDashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            
            if (petOwner == null)
            {
                return NotFound();
            }

            return View(petOwner);
        }

        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> ServiceProviderDashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var serviceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(userId);
            
            if (serviceProvider == null)
            {
                return NotFound();
            }

            return View(serviceProvider);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers()
        {
            // Since GetAllUsersAsync doesn't exist, we'll create a simple view
            // In a real application, you might need to implement pagination or search
            // For now, we'll just show a message that this feature needs implementation
            ViewBag.Message = "User management functionality needs to be implemented with proper pagination and search.";
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            try
            {
                await _userService.DeactivateUserAsync(id);
                TempData["SuccessMessage"] = "User deactivated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deactivating the user: " + ex.Message;
            }

            return RedirectToAction("ManageUsers");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            try
            {
                await _userService.ReactivateUserAsync(id);
                TempData["SuccessMessage"] = "User reactivated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while reactivating the user: " + ex.Message;
            }

            return RedirectToAction("ManageUsers");
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                User = user
            };

            // Load additional data based on user type
            if (user.UserType == UserType.PetOwner)
            {
                model.PetOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(id);
            }
            else if (user.UserType == UserType.ServiceProvider)
            {
                model.ServiceProvider = await _serviceProviderService.GetServiceProviderByUserIdAsync(id);
            }

            return View(model);
        }
    }
}

