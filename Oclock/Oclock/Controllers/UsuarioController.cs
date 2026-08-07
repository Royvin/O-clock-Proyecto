using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Models;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Oclock.Controllers
{
    public class UsuarioController : Controller
    {
        private const string RememberMeCookieName = "OclockRememberMe";

        private readonly By5rqco0trg7fpqgnpvmContext _context;
        private readonly IConfiguration _config;
        private readonly IDataProtector _rememberMeProtector;

        public UsuarioController(
            By5rqco0trg7fpqgnpvmContext context,
            IConfiguration config,
            IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _config = config;
            _rememberMeProtector = dataProtectionProvider.CreateProtector("Oclock.RememberMe.Cookie");
        }

        public async Task<IActionResult> Index(string tab = "login")
        {
            bool showRegister = tab == "register";
            ViewBag.ShowRegister = showRegister;

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            if (!showRegister)
            {
                int? usuarioIdSesion = HttpContext.Session.GetInt32("UsuarioId");
                int? usuarioRolSesion = HttpContext.Session.GetInt32("UsuarioRol");

                if (usuarioIdSesion.HasValue && usuarioRolSesion.HasValue)
                {
                    return RedireccionarPorRol(usuarioRolSesion.Value);
                }

                var usuarioRecordado = await ObtenerUsuarioRecordadoAsync();

                if (usuarioRecordado != null)
                {
                    GuardarSesionUsuario(usuarioRecordado);
                    return RedireccionarPorRol(usuarioRecordado.IdRol);
                }

                string? emailRecordado = ObtenerEmailRecordadoDesdeCookie();

                if (!string.IsNullOrWhiteSpace(emailRecordado))
                {
                    ViewBag.RememberedEmail = emailRecordado;
                    ViewBag.RememberMeChecked = true;
                }
            }

            var model = new Usuario();

            if (showRegister)
            {
                model.FechaContratacion = DateOnly.FromDateTime(AhoraCostaRica());
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            try
            {
                email = (email ?? "").Trim();
                string emailNormalizado = email.ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.ErrorMessage = "Debe ingresar correo y contraseña.";
                    ViewBag.ShowRegister = false;
                    ViewBag.RememberedEmail = email;
                    ViewBag.RememberMeChecked = rememberMe;
                    return View("Index", new Usuario());
                }

                var usuario = await _context.Usuarios
                    .Include(u => u.IdRolNavigation)
                    .FirstOrDefaultAsync(u =>
                        u.Email != null &&
                        u.Email.ToLower() == emailNormalizado &&
                        u.Activo == true);

                if (usuario == null)
                {
                    ViewBag.ErrorMessage = "Credenciales incorrectas.";
                    ViewBag.ShowRegister = false;
                    ViewBag.RememberedEmail = email;
                    ViewBag.RememberMeChecked = rememberMe;
                    return View("Index", new Usuario());
                }

                string passwordEncriptada = EncriptarContraseña(password);

                if (usuario.Contraseña != passwordEncriptada)
                {
                    ViewBag.ErrorMessage = "Credenciales incorrectas.";
                    ViewBag.ShowRegister = false;
                    ViewBag.RememberedEmail = email;
                    ViewBag.RememberMeChecked = rememberMe;
                    return View("Index", new Usuario());
                }

                GuardarSesionUsuario(usuario);

                if (rememberMe)
                {
                    GuardarCookieRecordarme(usuario);
                }
                else
                {
                    EliminarCookieRecordarme();
                }

                return RedireccionarPorRol(usuario.IdRol);
            }
            catch
            {
                ViewBag.ErrorMessage = "No se pudo iniciar sesión. Intente nuevamente.";
                ViewBag.ShowRegister = false;
                ViewBag.RememberedEmail = email;
                ViewBag.RememberMeChecked = rememberMe;
                return View("Index", new Usuario());
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Usuario model, string confirmar_contraseña)
        {
            try
            {
                PrepararModeloRegistro(model);
                var navKeys = new[]
                {
                    nameof(model.IdRolNavigation),
                    nameof(model.BonosAsignados),
                    nameof(model.Capacitacions),
                    nameof(model.Documentos),
                    nameof(model.Expedientes),
                    nameof(model.Marcas),
                    nameof(model.Notificacions),
                    nameof(model.Solicituds),
                    nameof(model.UsuarioHorarios),
                };

                foreach (var key in navKeys)
                {
                    if (ModelState.ContainsKey(key))
                        ModelState.Remove(key);
                }

                ValidarRegistro(model, confirmar_contraseña);

                if (!ModelState.IsValid)
                {
                    ViewBag.ShowRegister = true;
                    return View("Index", model);
                }

                string emailNormalizado = model.Email.Trim().ToLowerInvariant();

                bool emailExiste = await _context.Usuarios
                    .AnyAsync(u =>
                        u.Email != null &&
                        u.Email.ToLower() == emailNormalizado);

                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Este correo electrónico ya está registrado.");
                    ViewBag.ShowRegister = true;
                    return View("Index", model);
                }

                model.Email = emailNormalizado;
                model.Telefono = model.Telefono?.Trim();
                model.Contraseña = EncriptarContraseña(model.Contraseña);
                model.IdRol = 2;
                model.Activo = true;
                model.Estado = "Activo";

                _context.Usuarios.Add(model);
                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Usuario registrado exitosamente. Por favor inicia sesión.";
                ViewBag.ShowRegister = false;
                ViewBag.RememberedEmail = model.Email;
                return View("Index", new Usuario());
            }
            catch
            {
                ModelState.AddModelError("", "No se pudo registrar el usuario. Revise los datos e intente nuevamente.");
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
                    ViewBag.ErrorMessage = "Debe ingresar un correo electrónico.";
                    return View();
                }

                email = email.Trim();
                string emailNormalizado = email.ToLowerInvariant();

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.Email != null &&
                        u.Email.ToLower() == emailNormalizado &&
                        u.Activo == true);

                TempData["SuccessMessage"] = "Si el correo existe, se enviará un código de recuperación.";

                if (usuario == null)
                {
                    return RedirectToAction("ForgotPassword");
                }

                string codigo = GenerarCodigo6Digitos();
                DateTime expira = AhoraCostaRica().AddMinutes(10);

                HttpContext.Session.SetString("ResetEmail", usuario.Email ?? emailNormalizado);
                HttpContext.Session.SetString("ResetCodigo", codigo);
                HttpContext.Session.SetString("ResetExpira", expira.ToString("O"));

                EnviarCorreoCodigo(usuario.Email ?? emailNormalizado, codigo);

                TempData["SuccessMessage"] = "Te enviamos un código al correo. Revisa tu bandeja.";
                return RedirectToAction("ResetPassword");
            }
            catch
            {
                ViewBag.ErrorMessage = "No se pudo enviar el correo de recuperación. Intente nuevamente.";
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

                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(codigoGuardado) ||
                    string.IsNullOrWhiteSpace(expiraStr))
                {
                    ViewBag.ErrorMessage = "La solicitud expiró. Vuelve a solicitar el código.";
                    return View();
                }

                DateTime expira = DateTime.Parse(expiraStr, null, DateTimeStyles.RoundtripKind);

                if (AhoraCostaRica() > expira)
                {
                    LimpiarSesionRecuperacion();

                    ViewBag.ErrorMessage = "El código expiró. Solicita uno nuevo.";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim() != codigoGuardado)
                {
                    ViewBag.ErrorMessage = "El código es incorrecto.";
                    return View();
                }

                if (!ContraseñaValida(nuevaContrasena))
                {
                    ViewBag.ErrorMessage = "La contraseña debe tener al menos 8 caracteres e incluir letras y números.";
                    return View();
                }

                if (nuevaContrasena != confirmarContrasena)
                {
                    ViewBag.ErrorMessage = "Las contraseñas no coinciden.";
                    return View();
                }

                string emailNormalizado = email.Trim().ToLowerInvariant();

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.Email != null &&
                        u.Email.ToLower() == emailNormalizado &&
                        u.Activo == true);

                if (usuario == null)
                {
                    ViewBag.ErrorMessage = "Usuario no encontrado.";
                    return View();
                }

                usuario.Contraseña = EncriptarContraseña(nuevaContrasena);
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                LimpiarSesionRecuperacion();

                TempData["SuccessMessage"] = "Contraseña actualizada. Ya puedes iniciar sesión.";
                return RedirectToAction("Index", new { tab = "login" });
            }
            catch
            {
                ViewBag.ErrorMessage = "No se pudo restablecer la contraseña. Intente nuevamente.";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            EliminarCookieRecordarme();
            return RedirectToAction("Index");
        }

        private static DateTime AhoraCostaRica()
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch
            {
                return DateTime.Now;
            }
        }

        private static void PrepararModeloRegistro(Usuario model)
        {
            model.Nombre = (model.Nombre ?? "").Trim();
            model.Apellido = (model.Apellido ?? "").Trim();
            model.Email = (model.Email ?? "").Trim();
            model.Telefono = (model.Telefono ?? "").Trim();
            model.Contraseña = model.Contraseña ?? "";
        }

        private void ValidarRegistro(Usuario model, string confirmarContraseña)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            }
            else if (!TextoNombreValido(model.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre solo debe contener letras y espacios.");
            }

            if (string.IsNullOrWhiteSpace(model.Apellido))
            {
                ModelState.AddModelError("Apellido", "El apellido es obligatorio.");
            }
            else if (!TextoNombreValido(model.Apellido))
            {
                ModelState.AddModelError("Apellido", "El apellido solo debe contener letras y espacios.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "El correo electrónico es obligatorio.");
            }
            else if (!EmailValido(model.Email))
            {
                ModelState.AddModelError("Email", "Ingrese un correo electrónico válido.");
            }

            if (string.IsNullOrWhiteSpace(model.Telefono))
            {
                ModelState.AddModelError("Telefono", "El teléfono es obligatorio.");
            }
            else if (!TelefonoValido(model.Telefono))
            {
                ModelState.AddModelError("Telefono", "El teléfono debe contener exactamente 8 dígitos numéricos.");
            }

            if (!model.FechaContratacion.HasValue)
            {
                ModelState.AddModelError("FechaContratacion", "La fecha de contratación es obligatoria.");
            }
            else
            {
                var hoy = DateOnly.FromDateTime(AhoraCostaRica());

                if (model.FechaContratacion.Value > hoy)
                {
                    ModelState.AddModelError("FechaContratacion", "La fecha de contratación no puede ser futura.");
                }
            }

            if (string.IsNullOrWhiteSpace(model.Contraseña))
            {
                ModelState.AddModelError("Contraseña", "La contraseña es obligatoria.");
            }
            else if (!ContraseñaValida(model.Contraseña))
            {
                ModelState.AddModelError("Contraseña", "La contraseña debe tener al menos 8 caracteres e incluir letras y números.");
            }

            if (string.IsNullOrWhiteSpace(confirmarContraseña))
            {
                ModelState.AddModelError("", "Debe confirmar la contraseña.");
            }
            else if (model.Contraseña != confirmarContraseña)
            {
                ModelState.AddModelError("", "Las contraseñas no coinciden.");
            }
        }

        private static bool TextoNombreValido(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            return Regex.IsMatch(valor.Trim(), @"^[\p{L}\s'-]+$");
        }

        private static bool EmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var mail = new MailAddress(email.Trim());
                return mail.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }

        private static bool TelefonoValido(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return false;
            }

            return Regex.IsMatch(telefono.Trim(), @"^\d{8}$");
        }

        private static bool ContraseñaValida(string? contraseña)
        {
            if (string.IsNullOrWhiteSpace(contraseña))
            {
                return false;
            }

            if (contraseña.Length < 8)
            {
                return false;
            }

            bool tieneLetra = contraseña.Any(char.IsLetter);
            bool tieneNumero = contraseña.Any(char.IsDigit);

            return tieneLetra && tieneNumero;
        }

        private void GuardarSesionUsuario(Usuario usuario)
        {
            HttpContext.Session.SetInt32("UsuarioId", usuario.IdUsuario);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "");
            HttpContext.Session.SetString("UsuarioEmail", usuario.Email ?? "");
            HttpContext.Session.SetInt32("UsuarioRol", usuario.IdRol);
        }

        private IActionResult RedireccionarPorRol(int idRol)
        {
            if (idRol == 1)
            {
                return RedirectToAction("AdminHome", "Home");
            }

            if (idRol == 2)
            {
                return RedirectToAction("Marcas", "Empleado");
            }

            ViewBag.ErrorMessage = "Rol no reconocido.";
            ViewBag.ShowRegister = false;
            return View("Index", new Usuario());
        }

        private void GuardarCookieRecordarme(Usuario usuario)
        {
            string email = usuario.Email ?? "";
            string valorCookie = $"{usuario.IdUsuario}|{email}";
            string valorProtegido = _rememberMeProtector.Protect(valorCookie);

            Response.Cookies.Append(RememberMeCookieName, valorProtegido, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            });
        }

        private void EliminarCookieRecordarme()
        {
            Response.Cookies.Delete(RememberMeCookieName);
        }

        private async Task<Usuario?> ObtenerUsuarioRecordadoAsync()
        {
            try
            {
                if (!Request.Cookies.TryGetValue(RememberMeCookieName, out string? valorCookie))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(valorCookie))
                {
                    return null;
                }

                string valorDesprotegido = _rememberMeProtector.Unprotect(valorCookie);
                string[] partes = valorDesprotegido.Split('|');

                if (partes.Length != 2)
                {
                    EliminarCookieRecordarme();
                    return null;
                }

                if (!int.TryParse(partes[0], out int idUsuario))
                {
                    EliminarCookieRecordarme();
                    return null;
                }

                string emailNormalizado = partes[1].Trim().ToLowerInvariant();

                var usuario = await _context.Usuarios
                    .Include(u => u.IdRolNavigation)
                    .FirstOrDefaultAsync(u =>
                        u.IdUsuario == idUsuario &&
                        u.Email != null &&
                        u.Email.ToLower() == emailNormalizado &&
                        u.Activo == true);

                if (usuario == null)
                {
                    EliminarCookieRecordarme();
                    return null;
                }

                return usuario;
            }
            catch
            {
                EliminarCookieRecordarme();
                return null;
            }
        }

        private string? ObtenerEmailRecordadoDesdeCookie()
        {
            try
            {
                if (!Request.Cookies.TryGetValue(RememberMeCookieName, out string? valorCookie))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(valorCookie))
                {
                    return null;
                }

                string valorDesprotegido = _rememberMeProtector.Unprotect(valorCookie);
                string[] partes = valorDesprotegido.Split('|');

                if (partes.Length != 2)
                {
                    return null;
                }

                return partes[1];
            }
            catch
            {
                return null;
            }
        }

        private void LimpiarSesionRecuperacion()
        {
            HttpContext.Session.Remove("ResetEmail");
            HttpContext.Session.Remove("ResetCodigo");
            HttpContext.Session.Remove("ResetExpira");
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
                throw new Exception("Configuración SMTP incompleta.");
            }

            if (!int.TryParse(portStr, out int port))
            {
                throw new Exception("Puerto SMTP inválido.");
            }

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
                    mail.Body =
                        "Tu código de recuperación es: " + codigo +
                        "\n\nEste código vence en 10 minutos." +
                        "\n\nSi no solicitaste este código, puedes ignorar este correo." +
                        "\n\nSaludos," +
                        "\nSoporte O'Clock";
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

                if (value < 0)
                {
                    value = Math.Abs(value);
                }

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
    }
}