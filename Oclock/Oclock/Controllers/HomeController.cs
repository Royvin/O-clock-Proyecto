using Microsoft.AspNetCore.Mvc;
using Oclock.Filters;
using Oclock.Models;
using System.Diagnostics;

namespace Oclock.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [AuthorizeRole(1)]
        public IActionResult AdminHome()
        {
            return View();
        }

        [AuthorizeRole(2)]
        public IActionResult ColaboradorHome()
        {
            return RedirectToAction("Marcas", "Empleado");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}