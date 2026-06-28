using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace WebApp5Identity.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            var user = HttpContext.User as ClaimsPrincipal;
            if (!user.HasClaim(c => c.Type == ClaimTypes.DateOfBirth))
            {
                ViewBag.Message = "Cannot detect the Age - Claim is absent.";
                return View();
            }

            int minAge = 16;
            var dateOfBirth = Convert.ToDateTime(
                user.FindFirst(c => c.Type == ClaimTypes.DateOfBirth).Value);

            if (calculateAge(dateOfBirth) >= minAge)
            {
                ViewBag.Message = "You can view this page.";
            }
            else
            {
                ViewBag.Message = "Your cannot view this page - your age is bellow permitted one.";
            }

            return View();
        }

        private int calculateAge(DateTime dateOfBirth)
        {
            int calculatedAge = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth > DateTime.Today.AddYears(-calculatedAge))
            {
                calculatedAge--;
            }
            return calculatedAge;
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}

