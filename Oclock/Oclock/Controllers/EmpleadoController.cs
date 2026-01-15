using Microsoft.AspNetCore.Mvc;

namespace Oclock.Controllers
{
    public class EmpleadoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult HistorialMarcas()
        {
            return View();
        }

        public IActionResult Marcas()
        {
            return View();
        }

        public IActionResult Solicitudes()
        {
            return View();
        }
    }
}
