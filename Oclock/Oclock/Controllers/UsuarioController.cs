using Microsoft.AspNetCore.Mvc;
using Oclock.Models;
using Oclock.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Oclock.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly By5rqco0trg7fpqgnpvmContext _context;

        public UsuarioController(By5rqco0trg7fpqgnpvmContext context)
        {
            _context = context;
        }

        public IActionResult Index(string tab = "login")
        {
            ViewBag.ShowRegister = (tab == "register");
            return View(new Usuario());
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            try
            {
                // Buscar usuario por email
                var usuario = await _context.Usuarios
                    .Include(u => u.IdRolNavigation)
                    .FirstOrDefaultAsync(u => u.Email == email && u.Activo == true);

                if (usuario == null)
                {
                    ViewBag.ErrorMessage = "Credenciales incorrectas";
                    ViewBag.ShowRegister = false;
                    return View("Index", new Usuario());
                }

                // Verificar contraseña (encriptada)
                string passwordEncriptada = EncriptarContraseña(password);

                if (usuario.Contraseña != passwordEncriptada)
                {
                    ViewBag.ErrorMessage = "Credenciales incorrectas";
                    ViewBag.ShowRegister = false;
                    return View("Index", new Usuario());
                }

                HttpContext.Session.SetInt32("UsuarioId", usuario.IdUsuario);
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                HttpContext.Session.SetInt32("UsuarioRol", usuario.IdRol);

                // Redirigir según el rol
                if (usuario.IdRol == 1) 
                {
                    return RedirectToAction("AdminHome", "Home");
                }
                else if (usuario.IdRol == 2) 
                {
                    return RedirectToAction("Marcas", "Empleado");
                }
                else
                {
                    ViewBag.ErrorMessage = "Rol no reconocido";
                    ViewBag.ShowRegister = false;
                    return View("Index", new Usuario());
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al iniciar sesión: " + ex.Message;
                ViewBag.ShowRegister = false;
                return View("Index", new Usuario());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(Usuario model, string confirmar_contraseña)
        {
            try
            {
                if (model.Contraseña != confirmar_contraseña)
                {
                    ModelState.AddModelError("", "Las contraseñas no coinciden");
                    ViewBag.ShowRegister = true;
                    return View("Index", model);
                }

                // Verificar si el email ya existe
                var emailExiste = await _context.Usuarios
                    .AnyAsync(u => u.Email == model.Email);

                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Este correo electrónico ya está registrado");
                    ViewBag.ShowRegister = true;
                    return View("Index", model);
                }

                // Encriptar contraseña
                model.Contraseña = EncriptarContraseña(model.Contraseña);

                // Asignar valores por defecto
                model.IdRol = 2;
                model.Activo = true;
                model.Estado = "Activo";

                // Si no se proporciona fecha de contratación, usar la fecha actual
                if (!model.FechaContratacion.HasValue)
                {
                    model.FechaContratacion = DateOnly.FromDateTime(DateTime.Now);
                }

                // Guardar en la base de datos
                _context.Usuarios.Add(model);
                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Usuario registrado exitosamente. Por favor inicia sesión.";
                ViewBag.ShowRegister = false;
                return View("Index", new Usuario());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al registrar usuario: " + ex.Message);
                ViewBag.ShowRegister = true;
                return View("Index", model);
            }
        }

        private string EncriptarContraseña(string contraseña)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contraseña));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Método para cerrar sesión
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}