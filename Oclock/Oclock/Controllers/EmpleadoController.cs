using Microsoft.AspNetCore.Mvc;
using Oclock.Filters;

namespace Oclock.Controllers
{
    [AuthorizeRole(2)]
    public class EmpleadoController : Controller
    {

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
