using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Filters;
using Oclock.Helpers;
using Oclock.Models;

namespace Oclock.Controllers
{
    [AuthorizeRole(1)]
    public class AdminGestionUsuariosController : Controller
    {
        private readonly By5rqco0trg7fpqgnpvmContext _context;

        public AdminGestionUsuariosController(By5rqco0trg7fpqgnpvmContext context)
        {
            _context = context;
        }

        public IActionResult GestionUsuarios(int pagina = 1)
        {
            int porPagina = 5;
            var total = _context.Usuarios.Count();
            var usuarios = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / porPagina);

            return View(usuarios);
        }

        public IActionResult Expediente(int id)
        {
            var usuario = _context.Usuarios
                .Include(u => u.Expedientes)
                .Include(u => u.Documentos)
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario == null) return NotFound();

            ViewBag.Roles = _context.Rols.ToList();
            ViewBag.DiasVacaciones = VacacionesHelper.AcumularYObtenerSaldo(_context, usuario);

            return View(usuario);
        }

        [HttpPost]
        public IActionResult GuardarUsuario(int idUsuario, string nombre, string apellido,
            string email, string? telefono, string? estado, int idRol)
        {
            var usuario = _context.Usuarios.Find(idUsuario);
            if (usuario == null) return NotFound();

            usuario.Nombre = nombre;
            usuario.Apellido = apellido;
            usuario.Email = email;
            usuario.Telefono = telefono;
            usuario.Estado = estado;
            usuario.IdRol = idRol;

            _context.SaveChanges();
            TempData["Mensaje"] = "Información actualizada correctamente.";
            return RedirectToAction("Expediente", new { id = idUsuario });
        }

        [HttpPost]
        public IActionResult GuardarExpediente(int idUsuario, string? cedula, string? direccion,
            string? ciudad, string? estadoCivil, string? contactoEmergencia, string? telefonoEmergencia, decimal? salario)
        {
            var expediente = _context.Expedientes.FirstOrDefault(e => e.IdUsuario == idUsuario);

            if (expediente == null)
            {
                expediente = new Expediente { IdUsuario = idUsuario };
                _context.Expedientes.Add(expediente);
            }

            expediente.Cedula = cedula;
            expediente.Direccion = direccion;
            expediente.Ciudad = ciudad;
            expediente.EstadoCivil = estadoCivil;
            expediente.ContactoEmergencia = contactoEmergencia;
            expediente.TelefonoEmergencia = telefonoEmergencia;
            expediente.Salario = salario;

            _context.SaveChanges();
            TempData["Mensaje"] = "Expediente actualizado correctamente.";
            return RedirectToAction("Expediente", new { id = idUsuario });
        }

        [HttpPost]
        public async Task<IActionResult> SubirDocumento(int idUsuario, string? categoria, IFormFile archivo)
        {
            if (archivo != null && archivo.Length > 0)
            {
                byte[] contenido;
                using (var ms = new MemoryStream())
                {
                    await archivo.CopyToAsync(ms);
                    contenido = ms.ToArray();
                }

                var doc = new Documento
                {
                    IdUsuario = idUsuario,
                    Categoria = categoria,
                    NombreArchivo = archivo.FileName,
                    ContenidoArchivo = contenido,
                    TipoMime = archivo.ContentType,
                    FechaSubida = DateTime.Now
                };

                _context.Documentos.Add(doc);
                _context.SaveChanges();
                TempData["Mensaje"] = "Documento subido correctamente.";
            }
            else
            {
                TempData["Error"] = "Debe seleccionar un archivo.";
            }

            return RedirectToAction("Expediente", new { id = idUsuario });
        }

        public IActionResult DescargarDocumento(int idDocumento)
        {
            var doc = _context.Documentos.Find(idDocumento);
            if (doc == null || doc.ContenidoArchivo == null) return NotFound();

            return File(doc.ContenidoArchivo, doc.TipoMime ?? "application/octet-stream", doc.NombreArchivo);
        }

        [HttpPost]
        public IActionResult EliminarDocumento(int idDocumento, int idUsuario)
        {
            var doc = _context.Documentos.Find(idDocumento);
            if (doc != null)
            {
                _context.Documentos.Remove(doc);
                _context.SaveChanges();
                TempData["Mensaje"] = "Documento eliminado.";
            }

            return RedirectToAction("Expediente", new { id = idUsuario });
        }
    }
}