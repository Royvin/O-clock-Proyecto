using Microsoft.AspNetCore.Mvc;
using Oclock.Filters;

namespace Oclock.Controllers
{
    [AuthorizeRole(1)]
    public class AdminController : Controller
    {

        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Bonos()
        {
            return View();
        }
        public IActionResult BonosParametroConfig()
        {
            return View();
        }

        public IActionResult FeriadosConfig()
        {
            return View();
        }

        public IActionResult GestionDocumentos()
        {
            return View();
        }
        public IActionResult HorariosConfig()
        {
            return View();
        }
        public IActionResult NotificacionesConfig()
        {
            return View();
        }

        public IActionResult RankingPuntualidad()
        {
            return View();
        }
        public IActionResult Reportes()
        {
            return View();
        }
        public IActionResult VerMarcas()
        {
            return View();
        }

        
    }
}
