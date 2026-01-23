using Microsoft.AspNetCore.Mvc;
using Oclock.Models;

public class UsuarioController : Controller
{
    public IActionResult Index(string tab = "login")
    {
        ViewBag.ShowRegister = (tab == "register");
        return View(new Usuario());
    }

    [HttpPost]
    public IActionResult Login(string email, string password, bool rememberMe)
    {
        // Tu lógica de login aquí
        return RedirectToAction("Dashboard", "Home");
    }

    [HttpPost]
    public IActionResult Register(Usuario model, string confirmar_contraseña)
    {
        // Validar que las contraseñas coincidan
        if (model.Contraseña != confirmar_contraseña)
        {
            ModelState.AddModelError("", "Las contraseñas no coinciden");
            ViewBag.ShowRegister = true;
            return View("Index", model);
        }

        // Tu lógica de registro aquí
        return RedirectToAction("Index", new { tab = "login" });
    }
}