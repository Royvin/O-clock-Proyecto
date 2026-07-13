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

        public IActionResult Index()
        {
            int? usuarioRol = HttpContext.Session.GetInt32("UsuarioRol");

            if (usuarioRol == 1)
            {
                return RedirectToAction("AdminHome", "Home");
            }

            if (usuarioRol == 2)
            {
                return RedirectToAction("Marcas", "Empleado");
            }

            return RedirectToAction("Index", "Usuario", new { tab = "login" });
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
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            return View(new ErrorViewModel
            {
                RequestId = requestId
            });
        }
    }
}