using Microsoft.AspNetCore.Mvc;

namespace PetCarePlatform.Web.Controllers
{
    public class SupportController : Controller
    {
        public IActionResult Help()
        {
            ViewData["Title"] = "Help Center";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Us";
            return View();
        }

        public IActionResult FAQ()
        {
            ViewData["Title"] = "Frequently Asked Questions";
            return View();
        }
    }
}

