using Microsoft.AspNetCore.Mvc;
using Oclock.Data;
using Oclock.Models;
using System;
using System.Linq;

namespace Oclock.Controllers
{
    public class NotificacionController : Controller
    {
        private readonly By5rqco0trg7fpqgnpvmContext _context;

        public NotificacionController(By5rqco0trg7fpqgnpvmContext context)
        {
            _context = context;
        }

 
        [HttpGet]
        public IActionResult ObtenerResumen()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");
            int? rol = HttpContext.Session.GetInt32("UsuarioRol");

            if (idUsuario == null)
                return Unauthorized();

            var noLeidas = _context.Notificacions
                .Where(n => n.IdUsuario == idUsuario && n.Leida == false)
                .OrderByDescending(n => n.FechaNotificacion)
                .ToList();

            // ── Agrupar por tipo de prefijo ──────────────────────────────
            
            var solicitudes = noLeidas
                .Where(n => n.Mensaje != null &&
                            (n.Mensaje.StartsWith("[SOLICITUD]") ||
                             n.Mensaje.StartsWith("[NUEVA_SOLICITUD]")))
                .ToList();

            var bonos = noLeidas
                .Where(n => n.Mensaje != null && n.Mensaje.StartsWith("[BONO]"))
                .ToList();

            // ── Construir items del dropdown ─────────────────────────────
            var items = new List<object>();

            if (solicitudes.Any())
            {
                // Determinar ruta según rol
                string rutaSolicitudes = rol == 1
                    ? "/Home/AdminHome"   // ← ajusta si tu ruta difiere
                    : "/Empleado/Solicitudes";       // ← ajusta si tu ruta difiere

                string mensaje = rol == 1
                    ? $"Tienes {solicitudes.Count} solicitud{(solicitudes.Count > 1 ? "es" : "")} nueva{(solicitudes.Count > 1 ? "s" : "")} pendiente{(solicitudes.Count > 1 ? "s" : "")} de revisión."
                    : $"Tienes {solicitudes.Count} actualización{(solicitudes.Count > 1 ? "es" : "")} sobre tus solicitudes.";

                items.Add(new
                {
                    tipo = "solicitud",
                    cantidad = solicitudes.Count,
                    mensaje,
                    ruta = rutaSolicitudes,
                    ids = solicitudes.Select(n => n.IdNotificacion).ToList(),
                    fecha = solicitudes.First().FechaNotificacion
                });
            }

            if (bonos.Any())
            {
                items.Add(new
                {
                    tipo = "bono",
                    cantidad = bonos.Count,
                    mensaje = $"Tienes {bonos.Count} bono{(bonos.Count > 1 ? "s" : "")} asignado{(bonos.Count > 1 ? "s" : "")}.",
                    ruta = "/Empleado/HistorialBonos", // ← reemplaza con la ruta real del panel de bonos del empleado
                    ids = bonos.Select(n => n.IdNotificacion).ToList(),
                    fecha = bonos.First().FechaNotificacion
                });
            }

            return Json(new
            {
                total = noLeidas.Count,
                items
            });
        }

        // ─────────────────────────────────────────────
        // POST /Notificacion/MarcarLeidas
        // Recibe lista de IDs y los marca como leídos.
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult MarcarLeidas([FromBody] MarcarLeidasRequest request)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
                return Unauthorized();

            if (request?.Ids == null || !request.Ids.Any())
                return BadRequest();

            var notificaciones = _context.Notificacions
                .Where(n => request.Ids.Contains(n.IdNotificacion) && n.IdUsuario == idUsuario)
                .ToList();

            foreach (var n in notificaciones)
                n.Leida = true;

            _context.SaveChanges();

            return Ok(new { success = true });
        }

        // ─────────────────────────────────────────────
        // POST /Notificacion/MarcarTodasLeidas
        // Marca TODAS las notificaciones del usuario como leídas.
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult MarcarTodasLeidas()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
                return Unauthorized();

            var pendientes = _context.Notificacions
                .Where(n => n.IdUsuario == idUsuario && n.Leida == false)
                .ToList();

            foreach (var n in pendientes)
                n.Leida = true;

            _context.SaveChanges();

            return Ok(new { success = true });
        }
    }

    // DTO para recibir la lista de IDs
    public class MarcarLeidasRequest
    {
        public List<int> Ids { get; set; } = new();
    }
}