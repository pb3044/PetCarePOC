using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Web.ViewModels;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PetCarePlatform.Web.Controllers
{
    public class PetOwnerController : Controller
    {
        private readonly IPetOwnerService _petOwnerService;
        private readonly IUserService _userService;
        private readonly IMapper mapper;

        public PetOwnerController(IPetOwnerService petOwnerService,IUserService userService, IMapper mapper)
        {
            _petOwnerService = petOwnerService;
            _userService = userService;
            this.mapper = mapper;
        }

       // [Authorize(Roles = "PetOwner")]      
        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userService.GetUserByIdAsync(userId);
            var petOwner = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwner == null)
            {
                return NotFound("Pet owner not found.");
            }

            var viewModel = new PetOwnerDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Pets = petOwner.Pets?.ToList() ?? new List<Pet>(),
                RecentBookings = petOwner.Bookings?
                    .OrderByDescending(b => b.StartTime)
                    .Take(5)
                    .ToList() ?? new List<Booking>(),
                FavoriteProviders = petOwner.FavoriteProviders?.ToList() ?? new List<Core.Models.ServiceProvider>()
            };

            return View(viewModel);
        }

        // You can add other actions for pet owners here
        public IActionResult MyPets()
        {
            return View();
        }

        public IActionResult MyBookings()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }
    }
}