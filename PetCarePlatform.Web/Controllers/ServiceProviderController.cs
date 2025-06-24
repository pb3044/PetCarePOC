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
    public class ServiceProviderController : Controller
    {
        public IActionResult Dashboard()
        {
            // Add any logic you need for the pet owner dashboard
            // For example, get user's pets, recent bookings, etc.

            return View();
        }

        // You can add other actions for pet owners here
        public IActionResult Schedule()
        {
            return View();
        }

        public IActionResult MyServices()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult BookingRequest()
        {
            return View();
        }

        public IActionResult Reviews()
        {
            return View();
        }
        public IActionResult Earnings()
        {
            return View();
        }
        public IActionResult Reports()
        {
            return View();
        }
    }

}

