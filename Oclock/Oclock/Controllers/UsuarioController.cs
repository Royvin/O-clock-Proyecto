using Microsoft.AspNetCore.Mvc;
using Oclock.Models;
using Oclock.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Globalization;

namespace Oclock.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly By5rqco0trg7fpqgnpvmContext _context;
        private readonly IConfiguration _config;

        public UsuarioController(By5rqco0trg7fpqgnpvmContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public IActionResult Index(string tab = "login")
        {
            ViewBag.ShowRegister = (tab == "register");

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View(new Usuario());
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.IdRolNavigation)
                    .FirstOrDefaultAsync(u => u.Email == email && u.Activo == true);

                if (usuario == null)
                {
                    ViewBag.ErrorMessage = "Credenciales incorrectas";
                    ViewBag.ShowRegister = false;
                    return View("Index", new Usuario());
                }

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

                var emailExiste = await _context.Usuarios
                    .AnyAsync(u => u.Email == model.Email);

                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Este correo electrónico ya está registrado");
                    ViewBag.ShowRegister = true;
                    return View("Index", model);
                }

                model.Contraseña = EncriptarContraseña(model.Contraseña);
                model.IdRol = 2;
                model.Activo = true;
                model.Estado = "Activo";

                if (!model.FechaContratacion.HasValue)
                {
                    model.FechaContratacion = DateOnly.FromDateTime(DateTime.Now);
                }

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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    ViewBag.ErrorMessage = "Debes ingresar un correo.";
                    return View();
                }

                email = email.Trim();

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email && u.Activo == true);

                TempData["SuccessMessage"] = "Si el correo existe, se enviará un código de recuperación.";

                if (usuario == null)
                {
                    return RedirectToAction("ForgotPassword");
                }

                string codigo = GenerarCodigo6Digitos();
                DateTime expira = DateTime.Now.AddMinutes(10);

                HttpContext.Session.SetString("ResetEmail", email);
                HttpContext.Session.SetString("ResetCodigo", codigo);
                HttpContext.Session.SetString("ResetExpira", expira.ToString("O"));

                EnviarCorreoCodigo(email, codigo);

                TempData["SuccessMessage"] = "Te enviamos un código al correo. Revisa tu bandeja.";
                return RedirectToAction("ResetPassword");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al enviar el correo: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string codigo, string nuevaContrasena, string confirmarContrasena)
        {
            try
            {
                var email = HttpContext.Session.GetString("ResetEmail");
                var codigoGuardado = HttpContext.Session.GetString("ResetCodigo");
                var expiraStr = HttpContext.Session.GetString("ResetExpira");

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(codigoGuardado) || string.IsNullOrWhiteSpace(expiraStr))
                {
                    ViewBag.ErrorMessage = "La solicitud expiró. Vuelve a solicitar el código.";
                    return View();
                }

                DateTime expira = DateTime.Parse(expiraStr, null, DateTimeStyles.RoundtripKind);

                if (DateTime.Now > expira)
                {
                    HttpContext.Session.Remove("ResetEmail");
                    HttpContext.Session.Remove("ResetCodigo");
                    HttpContext.Session.Remove("ResetExpira");

                    ViewBag.ErrorMessage = "El código expiró. Solicita uno nuevo.";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim() != codigoGuardado)
                {
                    ViewBag.ErrorMessage = "El código es incorrecto.";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(nuevaContrasena) || nuevaContrasena.Length < 8)
                {
                    ViewBag.ErrorMessage = "La contraseña debe tener al menos 8 caracteres.";
                    return View();
                }

                if (nuevaContrasena != confirmarContrasena)
                {
                    ViewBag.ErrorMessage = "Las contraseñas no coinciden.";
                    return View();
                }

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email && u.Activo == true);

                if (usuario == null)
                {
                    ViewBag.ErrorMessage = "Usuario no encontrado.";
                    return View();
                }

                usuario.Contraseña = EncriptarContraseña(nuevaContrasena);
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("ResetEmail");
                HttpContext.Session.Remove("ResetCodigo");
                HttpContext.Session.Remove("ResetExpira");

                TempData["SuccessMessage"] = "Contraseña actualizada. Ya puedes iniciar sesión.";
                return RedirectToAction("Index", new { tab = "login" });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al restablecer: " + ex.Message;
                return View();
            }
        }

        private void EnviarCorreoCodigo(string destinatario, string codigo)
        {
            var host = _config["Smtp:Host"];
            var portStr = _config["Smtp:Port"];
            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Pass"];
            var from = _config["Smtp:From"];
            var enableSslStr = _config["Smtp:EnableSsl"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portStr) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass) ||
                string.IsNullOrWhiteSpace(from))
            {
                throw new Exception("Faltan datos SMTP en appsettings.json (Smtp:Host, Port, User, Pass, From).");
            }

            int port = int.Parse(portStr);

            bool enableSsl = true;
            if (!string.IsNullOrWhiteSpace(enableSslStr))
            {
                bool.TryParse(enableSslStr, out enableSsl);
            }

            using (var client = new SmtpClient(host, port))
            {
                client.Credentials = new NetworkCredential(user, pass);
                client.EnableSsl = enableSsl;

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(from);
                    mail.To.Add(destinatario);
                    mail.Subject = "O'Clock - Recuperación de contraseña";
                    mail.Body = "Tu código de recuperación es: " + codigo + "\n\nEste código vence en 10 minutos." + "\n\n Saludos"+ "\n\n Soporte Oclock";
                    mail.IsBodyHtml = false;

                    client.Send(mail);
                }
            }
        }

        private string GenerarCodigo6Digitos()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                int value = BitConverter.ToInt32(bytes, 0);
                if (value < 0) value = -value;
                return (value % 1000000).ToString("D6");
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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}