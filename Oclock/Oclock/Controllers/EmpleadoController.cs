using Microsoft.AspNetCore.Mvc;
using Oclock.Filters;
using Oclock.Data;
using Oclock.Models;
using System;
using System.Linq;

namespace Oclock.Controllers
{
    [AuthorizeRole(2)]
    public class EmpleadoController : Controller
    {
        private readonly By5rqco0trg7fpqgnpvmContext _context;

        public EmpleadoController(By5rqco0trg7fpqgnpvmContext context)
        {
            _context = context;
        }

        public IActionResult Marcas()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Index", "Usuario");
            }

            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var marcas = _context.Marcas
                .Where(m => m.IdUsuario == idUsuario.Value && m.Fecha == hoy)
                .OrderByDescending(m => m.IdMarca)
                .Take(20)
                .ToList();

            return View(marcas);
        }

        [HttpGet]
        public IActionResult ObtenerMarcas()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Json(new { success = false, message = "Sesión no válida. Inicie sesión nuevamente." });
            }

            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var marcas = _context.Marcas
                .Where(m => m.IdUsuario == idUsuario.Value && m.Fecha == hoy)
                .OrderByDescending(m => m.IdMarca)
                .Take(20)
                .ToList();

            return Json(new
            {
                success = true,
                marcas = marcas.Select(m => new
                {
                    tipo = (m.Nombre ?? "").ToLower(),
                    horaEntrada = m.HoraEntrada.HasValue ? m.HoraEntrada.Value.ToString("HH:mm:ss") : null,
                    horaSalida = m.HoraSalida.HasValue ? m.HoraSalida.Value.ToString("HH:mm:ss") : null,
                    ubicancia = m.Ubicancia ?? "San José, Costa Rica",
                    comentario = m.Comentario
                })
            });
        }

        [HttpPost]
        public IActionResult RegistrarMarca(string tipo)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Json(new { success = false, message = "Sesión no válida. Inicie sesión nuevamente." });
            }

            try
            {
                var ahora = DateTime.Now;
                var hoy = DateOnly.FromDateTime(ahora);

                tipo = (tipo ?? "").Trim().ToLower();

                var nuevaMarca = new Marca
                {
                    IdUsuario = idUsuario.Value,
                    Nombre = tipo,
                    Ubicancia = "San José, Costa Rica",
                    Fecha = hoy
                };

                if (tipo == "entrada")
                {
                    nuevaMarca.HoraEntrada = TimeOnly.FromDateTime(ahora);
                }
                else if (tipo == "salida")
                {
                    nuevaMarca.HoraSalida = TimeOnly.FromDateTime(ahora);
                }
                else
                {
                    nuevaMarca.Comentario = tipo;
                    nuevaMarca.HoraEntrada = TimeOnly.FromDateTime(ahora);
                }

                _context.Marcas.Add(nuevaMarca);
                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    id_marca = nuevaMarca.IdMarca,
                    marca = new
                    {
                        tipo = tipo,
                        hora = ahora.ToString("HH:mm:ss")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al guardar la marca en la base de datos.",
                    detail = ex.Message
                });
            }
        }

        // ✅ HU07: historial con filtro por fechas
        // Se usa por querystring: /Empleado/HistorialMarcas?desde=2026-01-01&hasta=2026-01-31
        public IActionResult HistorialMarcas(DateTime? desde, DateTime? hasta)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Index", "Usuario");
            }

            // Si no vienen fechas, por defecto: últimos 30 días (incluyendo hoy)
            DateTime hoyDateTime = DateTime.Today;
            DateTime desdeDT = desde?.Date ?? hoyDateTime.AddDays(-30);
            DateTime hastaDT = hasta?.Date ?? hoyDateTime;

            // Convertimos a DateOnly para comparar con Marca.Fecha
            var desdeDO = DateOnly.FromDateTime(desdeDT);
            var hastaDO = DateOnly.FromDateTime(hastaDT);

            var marcas = _context.Marcas
                .Where(m => m.IdUsuario == idUsuario.Value
                         && m.Fecha >= desdeDO
                         && m.Fecha <= hastaDO)
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.IdMarca)
                .ToList();

            // Para que la vista pueda mantener los filtros en pantalla
            ViewBag.Desde = desdeDT.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hastaDT.ToString("yyyy-MM-dd");

            return View(marcas);
        }

        public IActionResult Solicitudes()
        {
            return View();
        }
    }
}