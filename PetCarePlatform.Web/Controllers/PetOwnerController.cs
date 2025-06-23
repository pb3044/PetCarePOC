using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Web.Models;

namespace PetCarePlatform.Web.Controllers
{
    public class PetOwnerController : Controller
    {
        public IActionResult Dashboard()
        {
            // Add any logic you need for the pet owner dashboard
            // For example, get user's pets, recent bookings, etc.

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

