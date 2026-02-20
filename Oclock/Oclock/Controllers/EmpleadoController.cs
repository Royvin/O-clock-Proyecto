using Microsoft.AspNetCore.Mvc;
using Oclock.Filters;
using Oclock.Data;
using Oclock.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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


        [HttpGet]
        public IActionResult ObtenerTiposSolicitud()
        {
            var tipos = _context.TipoSolicituds
                .Select(t => new
                {
                    idTipoSolicitud = t.IdTipoSolicitud,
                    nombreSolicitud = t.NombreSolicitud
                })
                .ToList();

            return Json(tipos);
        }




        [HttpPost]
        public async Task<IActionResult> RegistrarSolicitud(SolicitudPost model, IFormFile archivo)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Json(new { success = false, message = "Sesión no válida." });
            }

            if (model.FechaFin < model.FechaInicio)
            {
                return Json(new { success = false, message = "El rango de fechas es inválido." });
            }

            if (model.FechaInicio.Date < DateTime.Today)
            {
                return Json(new { success = false, message = "No puede registrar fechas pasadas." });
            }

            // 1️⃣ Crear solicitud
            var nuevaSolicitud = new Solicitud
            {
                IdUsuario = idUsuario.Value,
                IdTipoSolicitud = model.IdTipoSolicitud,
                Descripcion = model.Descripcion,
                FechaSolicitud = DateOnly.FromDateTime(DateTime.Now),
                FechaInicio = DateOnly.FromDateTime(model.FechaInicio),
                FechaFin = DateOnly.FromDateTime(model.FechaFin),
                Estado = "pendiente",
                DescripcionEstado = "Pendiente de aprobación"
            };

            _context.Solicituds.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            // 2️⃣ Ahora ya tenemos el ID generado
            int idSolicitud = nuevaSolicitud.IdSolicitud;

            // 3️⃣ Si viene archivo
            if (archivo != null && archivo.Length > 0)
            {
                string carpetaBase = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/solicitudes",
                    idSolicitud.ToString()
                );

                if (!Directory.Exists(carpetaBase))
                {
                    Directory.CreateDirectory(carpetaBase);
                }

                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
                string rutaCompleta = Path.Combine(carpetaBase, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                var documento = new Documento
                {
                    IdUsuario = idUsuario.Value,
                    NombreArchivo = archivo.FileName,
                    RutaArchivo = $"/uploads/solicitudes/{idSolicitud}/{nombreArchivo}",
                    FechaSubida = DateTime.Now,
                    IdSolicitud = idSolicitud
                };

                _context.Documentos.Add(documento);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Solicitud registrada correctamente." });
        }

        [HttpGet]
        public IActionResult SolicitudesEmpleado()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Json(new { success = false });
            }

            var solicitudes = _context.Solicituds
       .Include(s => s.IdTipoSolicitudNavigation)
       .Where(s => s.IdUsuario == idUsuario)
       .Select(s => new
       {
           id = s.IdSolicitud,
           tipo = s.IdTipoSolicitud,
           tipoNombre = s.IdTipoSolicitudNavigation.NombreSolicitud.ToLower(),
           fechaInicio = s.FechaInicio.Value.ToString("yyyy-MM-dd"),
           fechaFin = s.FechaFin.Value.ToString("yyyy-MM-dd"),
           estado = s.Estado.ToLower(),
           fechaSolicitud = s.FechaSolicitud.ToString("yyyy-MM-dd"),
           motivo = s.Descripcion,
           prioridad = "normal",
           archivos = new List<string>()
       })
       .ToList();

            return Json(solicitudes);
        }


        [HttpPut]
        public IActionResult EditarSolicitud([FromBody] SolicitudPut model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
                return Unauthorized();

            var solicitud = _context.Solicituds
                .FirstOrDefault(s => s.IdSolicitud == model.IdSolicitud
                                  && s.IdUsuario == idUsuario.Value);

            if (solicitud == null)
                return NotFound();

            if (solicitud.Estado.ToLower() != "pendiente")
                return BadRequest("Solo se pueden editar solicitudes pendientes.");

            if (model.FechaFin < model.FechaInicio)
                return BadRequest("Rango de fechas inválido.");

            solicitud.IdTipoSolicitud = model.IdTipoSolicitud;
            solicitud.FechaInicio = DateOnly.FromDateTime(model.FechaInicio);
            solicitud.FechaFin = DateOnly.FromDateTime(model.FechaFin);
            solicitud.Descripcion = model.Descripcion;

            _context.SaveChanges();

            return Json(new { success = true });
        }



        [HttpPut]
        public IActionResult CancelarSolicitud(int id)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
                return Unauthorized();

            var solicitud = _context.Solicituds
                .FirstOrDefault(s => s.IdSolicitud == id
                                  && s.IdUsuario == idUsuario.Value);

            if (solicitud == null)
                return NotFound();

            if (solicitud.Estado.ToLower() != "pendiente")
                return BadRequest("Solo se pueden cancelar solicitudes pendientes.");

            solicitud.Estado = "cancelado";
            solicitud.DescripcionEstado = "Cancelada por el usuario";

            _context.SaveChanges();

            return Json(new { success = true });
        }



    }
}