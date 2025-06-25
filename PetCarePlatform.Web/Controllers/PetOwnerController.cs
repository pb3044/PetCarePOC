using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PetCarePlatform.Web.Controllers
{
    public class PetOwnerController : Controller
    {
        private readonly IPetOwnerService _petOwnerService;
        private readonly IMapper mapper;

        public PetOwnerController(IPetOwnerService petOwnerService, IMapper mapper)
        {
            _petOwnerService = petOwnerService;
            this.mapper = mapper;
        }

        public async Task<IActionResult> DashboardAsync()
        {
            // Add any logic you need for the pet owner dashboard
            // For example, get user's pets, recent bookings, etc.

            //********This Method is to test AutoMapper and service
            var petOwner = await _petOwnerService.GetPetOwnerByIdAsync(1);
            if (petOwner == null)
            {
                return NotFound("Pet owner not found.");
            }
            var petOwnerVM = mapper.Map<PetOwnerViewModel>(petOwner);
            return View();
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