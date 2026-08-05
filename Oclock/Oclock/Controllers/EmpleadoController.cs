using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Filters;
using Oclock.Helpers;
using Oclock.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        private static DateTime AhoraCostaRica()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        private static string NormalizarTexto(string? texto)
        {
            return (texto ?? "").Trim().ToLower();
        }

        private static bool TipoRequiereFechas(string tipoSolicitud)
        {
            tipoSolicitud = NormalizarTexto(tipoSolicitud);

            return tipoSolicitud.Contains("vacaciones")
                || tipoSolicitud.Contains("permiso personal")
                || tipoSolicitud == "otro";
        }

        private static bool TipoRequiereDocumento(string tipoSolicitud)
        {
            tipoSolicitud = NormalizarTexto(tipoSolicitud);

            return tipoSolicitud.Contains("incapacidad")
                || tipoSolicitud.Contains("maternidad")
                || tipoSolicitud.Contains("fallecimiento");
        }

        private static bool TipoGestionAdministrativa(string tipoSolicitud)
        {
            tipoSolicitud = NormalizarTexto(tipoSolicitud);

            return tipoSolicitud.Contains("incapacidad")
                || tipoSolicitud.Contains("maternidad")
                || tipoSolicitud.Contains("fallecimiento")
                || tipoSolicitud.Contains("constancia salarial");
        }

        private static bool TipoMarcaValido(string tipo)
        {
            tipo = NormalizarTexto(tipo);

            return tipo == "entrada"
                || tipo == "salida"
                || tipo == "almuerzo"
                || tipo == "descanso";
        }

        private static string ObtenerEtiquetaTipoMarca(string tipo)
        {
            tipo = NormalizarTexto(tipo);

            if (tipo == "entrada") return "Entrada";
            if (tipo == "salida") return "Salida";
            if (tipo == "almuerzo") return "Almuerzo";
            if (tipo == "descanso") return "Descanso";

            return "Marca";
        }

        private static string ObtenerComentarioMarca(string tipo)
        {
            tipo = NormalizarTexto(tipo);

            if (tipo == "entrada") return "Inicio de jornada laboral";
            if (tipo == "salida") return "Fin de jornada laboral";
            if (tipo == "almuerzo") return "Registro de almuerzo";
            if (tipo == "descanso") return "Registro de descanso";

            return "Registro de marca";
        }

        private static string? ObtenerHoraMarca(Marca marca)
        {
            if (marca.HoraEntrada.HasValue)
            {
                return marca.HoraEntrada.Value.ToString("HH:mm:ss");
            }

            if (marca.HoraSalida.HasValue)
            {
                return marca.HoraSalida.Value.ToString("HH:mm:ss");
            }

            return null;
        }

        private static string CalcularEstadoActual(List<Marca> marcas)
        {
            if (marcas == null || !marcas.Any())
            {
                return "Sin jornada iniciada";
            }

            bool tieneEntrada = marcas.Any(m => NormalizarTexto(m.Nombre) == "entrada");
            bool tieneSalida = marcas.Any(m => NormalizarTexto(m.Nombre) == "salida");

            if (!tieneEntrada)
            {
                return "Sin jornada iniciada";
            }

            if (tieneSalida)
            {
                return "Jornada finalizada";
            }

            var ultimaMarca = marcas
                .OrderByDescending(m => m.IdMarca)
                .FirstOrDefault();

            string ultimoTipo = NormalizarTexto(ultimaMarca?.Nombre);

            if (ultimoTipo == "almuerzo")
            {
                return "En almuerzo";
            }

            if (ultimoTipo == "descanso")
            {
                return "En descanso";
            }

            return "En jornada";
        }

        private static object ObtenerAccionesDisponibles(List<Marca> marcas)
        {
            bool tieneEntrada = marcas.Any(m => NormalizarTexto(m.Nombre) == "entrada");
            bool tieneSalida = marcas.Any(m => NormalizarTexto(m.Nombre) == "salida");
            bool tieneAlmuerzo = marcas.Any(m => NormalizarTexto(m.Nombre) == "almuerzo");
            bool tieneDescanso = marcas.Any(m => NormalizarTexto(m.Nombre) == "descanso");

            return new
            {
                entrada = !tieneEntrada,
                almuerzo = tieneEntrada && !tieneSalida && !tieneAlmuerzo,
                descanso = tieneEntrada && !tieneSalida && !tieneDescanso,
                salida = tieneEntrada && !tieneSalida
            };
        }

        private static string ConstruirDescripcionSolicitud(SolicitudPost model, string tipoSolicitud)
        {
            var partes = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.Descripcion))
            {
                partes.Add(model.Descripcion.Trim());
            }

            if (!string.IsNullOrWhiteSpace(model.ParentescoFamiliar))
            {
                partes.Add("Parentesco familiar: " + model.ParentescoFamiliar.Trim());
            }

            if (!string.IsNullOrWhiteSpace(model.NombreFamiliar))
            {
                partes.Add("Nombre del familiar: " + model.NombreFamiliar.Trim());
            }

            if (!string.IsNullOrWhiteSpace(model.MotivoConstancia))
            {
                partes.Add("Motivo de constancia salarial: " + model.MotivoConstancia.Trim());
            }

            if (!string.IsNullOrWhiteSpace(model.DetalleOtro))
            {
                partes.Add("Detalle adicional: " + model.DetalleOtro.Trim());
            }

            if (TipoGestionAdministrativa(tipoSolicitud))
            {
                partes.Add("Nota: solicitud sujeta a revisión administrativa para determinar días otorgados y fechas aplicables.");
            }

            return partes.Count > 0 ? string.Join("\n", partes) : "";
        }

        private static string ConstruirDescripcionSolicitud(SolicitudPut model, string tipoSolicitud)
        {
            var partes = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.Descripcion))
            {
                partes.Add(model.Descripcion.Trim());
            }

            if (!string.IsNullOrWhiteSpace(model.ParentescoFamiliar))
            {
                partes.Add("Parentesco familiar: " + model.ParentescoFamiliar.Trim());
            }

            if (!string.IsNullOrWhiteSpace(model.NombreFamiliar))
            {
                partes.Add("Nombre del familiar: " + model.NombreFamiliar.Trim());
            }

            if (!string.IsNullOrWhiteSpace(model.MotivoConstancia))
            {
                partes.Add("Motivo de constancia salarial: " + model.MotivoConstancia.Trim());
            }

            if (!string.IsNullOrWhiteSpace(model.DetalleOtro))
            {
                partes.Add("Detalle adicional: " + model.DetalleOtro.Trim());
            }

            if (TipoGestionAdministrativa(tipoSolicitud))
            {
                partes.Add("Nota: solicitud sujeta a revisión administrativa para determinar días otorgados y fechas aplicables.");
            }

            return partes.Count > 0 ? string.Join("\n", partes) : "";
        }

        private static bool ArchivoPermitido(IFormFile archivo)
        {
            var extension = Path.GetExtension(archivo.FileName).ToLower();

            return extension == ".pdf"
                || extension == ".jpg"
                || extension == ".jpeg"
                || extension == ".png";
        }

        private static bool ArchivoPesoValido(IFormFile archivo)
        {
            const long maxSize = 5 * 1024 * 1024;
            return archivo.Length <= maxSize;
        }

        private async Task GuardarArchivoSolicitud(Solicitud solicitud, IFormFile archivo)
        {
            string carpetaBase = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/uploads/solicitudes",
                solicitud.IdSolicitud.ToString());

            if (!Directory.Exists(carpetaBase))
            {
                Directory.CreateDirectory(carpetaBase);
            }

            string nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
            string rutaCompleta = Path.Combine(carpetaBase, nombreUnico);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            solicitud.RutaArchivo = $"/uploads/solicitudes/{solicitud.IdSolicitud}/{nombreUnico}";
            solicitud.NombreArchivo = archivo.FileName;
        }

        private static void EliminarArchivoFisico(string? rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
            {
                return;
            }

            string rutaFisica = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                rutaArchivo.TrimStart('/')
            );

            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }
        }

        public IActionResult Marcas()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Index", "Usuario");
            }

            var hoy = DateOnly.FromDateTime(AhoraCostaRica());

            var marcas = _context.Marcas
                .Where(m => m.IdUsuario == idUsuario.Value && m.Fecha == hoy)
                .OrderByDescending(m => m.IdMarca)
                .Take(20)
                .ToList();

            ViewBag.EstadoActual = CalcularEstadoActual(marcas);
            ViewBag.AccionesDisponibles = ObtenerAccionesDisponibles(marcas);

            return View(marcas);
        }

        [HttpGet]
        public IActionResult ObtenerMarcas()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Sesión no válida. Inicie sesión nuevamente."
                });
            }

            var hoy = DateOnly.FromDateTime(AhoraCostaRica());

            var marcas = _context.Marcas
                .Where(m => m.IdUsuario == idUsuario.Value && m.Fecha == hoy)
                .OrderByDescending(m => m.IdMarca)
                .Take(20)
                .ToList();

            return Json(new
            {
                success = true,
                estadoActual = CalcularEstadoActual(marcas),
                accionesDisponibles = ObtenerAccionesDisponibles(marcas),
                totalMarcas = marcas.Count,
                marcas = marcas.Select(m => new
                {
                    idMarca = m.IdMarca,
                    tipo = NormalizarTexto(m.Nombre),
                    tipoTexto = ObtenerEtiquetaTipoMarca(m.Nombre ?? ""),
                    horaEntrada = m.HoraEntrada.HasValue ? m.HoraEntrada.Value.ToString("HH:mm:ss") : null,
                    horaSalida = m.HoraSalida.HasValue ? m.HoraSalida.Value.ToString("HH:mm:ss") : null,
                    hora = ObtenerHoraMarca(m),
                    ubicancia = m.Ubicancia ?? "San José, Costa Rica",
                    comentario = string.IsNullOrWhiteSpace(m.Comentario)
                        ? ObtenerComentarioMarca(m.Nombre ?? "")
                        : m.Comentario
                })
            });
        }

        [HttpPost]
        public IActionResult RegistrarMarca(string tipo)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Sesión no válida. Inicie sesión nuevamente."
                });
            }

            try
            {
                var ahora = AhoraCostaRica();
                var hoy = DateOnly.FromDateTime(ahora);

                tipo = NormalizarTexto(tipo);

                if (!TipoMarcaValido(tipo))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tipo de marca inválido."
                    });
                }

                var marcasHoy = _context.Marcas
                    .Where(m => m.IdUsuario == idUsuario.Value && m.Fecha == hoy)
                    .OrderByDescending(m => m.IdMarca)
                    .ToList();

                bool tieneEntrada = marcasHoy.Any(m => NormalizarTexto(m.Nombre) == "entrada");
                bool tieneSalida = marcasHoy.Any(m => NormalizarTexto(m.Nombre) == "salida");
                bool yaExisteTipo = marcasHoy.Any(m => NormalizarTexto(m.Nombre) == tipo);

                if (yaExisteTipo)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Ya existe una marca de {ObtenerEtiquetaTipoMarca(tipo).ToLower()} registrada para hoy."
                    });
                }

                if (tipo != "entrada" && !tieneEntrada)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Primero debe registrar la entrada antes de marcar almuerzo, descanso o salida."
                    });
                }

                if ((tipo == "almuerzo" || tipo == "descanso") && tieneSalida)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No puede registrar esta marca porque la jornada ya fue finalizada."
                    });
                }

                if (tipo == "salida" && tieneSalida)
                {
                    return Json(new
                    {
                        success = false,
                        message = "La salida ya fue registrada para hoy."
                    });
                }

                var nuevaMarca = new Marca
                {
                    IdUsuario = idUsuario.Value,
                    Nombre = tipo,
                    Ubicancia = "San José, Costa Rica",
                    Fecha = hoy,
                    Comentario = ObtenerComentarioMarca(tipo)
                };

                if (tipo == "salida")
                {
                    nuevaMarca.HoraSalida = TimeOnly.FromDateTime(ahora);
                }
                else
                {
                    nuevaMarca.HoraEntrada = TimeOnly.FromDateTime(ahora);
                }

                _context.Marcas.Add(nuevaMarca);
                _context.SaveChanges();

                marcasHoy.Insert(0, nuevaMarca);

                return Json(new
                {
                    success = true,
                    message = $"Marca de {ObtenerEtiquetaTipoMarca(tipo).ToLower()} registrada correctamente a las {ahora:HH:mm:ss}.",
                    estadoActual = CalcularEstadoActual(marcasHoy),
                    accionesDisponibles = ObtenerAccionesDisponibles(marcasHoy),
                    marca = new
                    {
                        idMarca = nuevaMarca.IdMarca,
                        tipo = tipo,
                        tipoTexto = ObtenerEtiquetaTipoMarca(tipo),
                        hora = ahora.ToString("HH:mm:ss"),
                        ubicancia = nuevaMarca.Ubicancia,
                        comentario = nuevaMarca.Comentario
                    }
                });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "No se pudo guardar la marca. Intente nuevamente."
                });
            }
        }

        public IActionResult HistorialMarcas(DateTime? desde, DateTime? hasta)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Index", "Usuario");
            }

            DateTime hoyDateTime = AhoraCostaRica().Date;
            DateTime desdeDT = desde?.Date ?? hoyDateTime.AddDays(-30);
            DateTime hastaDT = hasta?.Date ?? hoyDateTime;

            var desdeDO = DateOnly.FromDateTime(desdeDT);
            var hastaDO = DateOnly.FromDateTime(hastaDT);

            var marcas = _context.Marcas
                .Where(m => m.IdUsuario == idUsuario.Value
                         && m.Fecha >= desdeDO
                         && m.Fecha <= hastaDO)
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.IdMarca)
                .ToList();

            foreach (var marca in marcas)
            {
                if (string.IsNullOrWhiteSpace(marca.Comentario))
                {
                    marca.Comentario = ObtenerComentarioMarca(marca.Nombre ?? "");
                }
            }

            ViewBag.Desde = desdeDT.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hastaDT.ToString("yyyy-MM-dd");

            return View(marcas);
        }

        public IActionResult Solicitudes()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Index", "Usuario");
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idUsuario.Value);

            if (usuario == null)
            {
                return RedirectToAction("Index", "Usuario");
            }

            ViewBag.DiasVacaciones = VacacionesHelper.AcumularYObtenerSaldo(_context, usuario);

            return View();
        }

        [HttpGet]
        public IActionResult ObtenerSaldoVacaciones()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");
            if (idUsuario == null)
                return Json(new { success = false });

            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idUsuario.Value);
            if (usuario == null)
                return Json(new { success = false });

            var saldo = VacacionesHelper.AcumularYObtenerSaldo(_context, usuario);
            return Json(new { success = true, diasVacaciones = saldo });
        }


        [HttpGet]
        public IActionResult ObtenerTiposSolicitud()
        {
            var tipos = _context.TipoSolicituds
                .OrderBy(t => t.NombreSolicitud)
                .Select(t => new
                {
                    idTipoSolicitud = t.IdTipoSolicitud,
                    nombreSolicitud = t.NombreSolicitud
                })
                .ToList();

            return Json(tipos);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarSolicitud(SolicitudPost model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Json(new { success = false, message = "Sesión no válida." });
            }

            var tipoSolicitud = await _context.TipoSolicituds
                .FirstOrDefaultAsync(t => t.IdTipoSolicitud == model.IdTipoSolicitud);

            if (tipoSolicitud == null)
            {
                return Json(new { success = false, message = "Seleccione un tipo de solicitud válido." });
            }

            string nombreTipo = tipoSolicitud.NombreSolicitud ?? "";
            bool requiereFechas = TipoRequiereFechas(nombreTipo);
            bool requiereDocumento = TipoRequiereDocumento(nombreTipo);

            if (requiereFechas)
            {
                if (!model.FechaInicio.HasValue || !model.FechaFin.HasValue)
                {
                    return Json(new { success = false, message = "Debe indicar la fecha de inicio y la fecha fin para este tipo de solicitud." });
                }

                if (model.FechaFin.Value.Date < model.FechaInicio.Value.Date)
                {
                    return Json(new { success = false, message = "El rango de fechas es inválido." });
                }

                if (model.FechaInicio.Value.Date < AhoraCostaRica().Date)
                {
                    return Json(new { success = false, message = "No puede registrar fechas pasadas." });
                }
            }

            if (requiereDocumento && (model.Archivo == null || model.Archivo.Length == 0))
            {
                return Json(new { success = false, message = "Este tipo de solicitud requiere adjuntar un documento de respaldo." });
            }

            if (model.Archivo != null && model.Archivo.Length > 0)
            {
                if (!ArchivoPermitido(model.Archivo))
                {
                    return Json(new { success = false, message = "Formato de archivo no permitido. Use PDF, JPG o PNG." });
                }

                if (!ArchivoPesoValido(model.Archivo))
                {
                    return Json(new { success = false, message = "El archivo no puede superar los 5MB." });
                }
            }

            var nuevaSolicitud = new Solicitud
            {
                IdUsuario = idUsuario.Value,
                IdTipoSolicitud = model.IdTipoSolicitud,
                Descripcion = ConstruirDescripcionSolicitud(model, nombreTipo),
                FechaSolicitud = DateOnly.FromDateTime(AhoraCostaRica()),
                FechaInicio = requiereFechas && model.FechaInicio.HasValue ? DateOnly.FromDateTime(model.FechaInicio.Value) : null,
                FechaFin = requiereFechas && model.FechaFin.HasValue ? DateOnly.FromDateTime(model.FechaFin.Value) : null,
                Estado = "pendiente",
                DescripcionEstado = TipoGestionAdministrativa(nombreTipo)
                    ? "Pendiente de revisión administrativa"
                    : "Pendiente de aprobación"
            };

            _context.Solicituds.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            if (model.Archivo != null && model.Archivo.Length > 0)
            {
                await GuardarArchivoSolicitud(nuevaSolicitud, model.Archivo);
                await _context.SaveChangesAsync();
            }

            var admins = _context.Usuarios
                .Where(u => u.IdRol == 1 && u.Activo == true)
                .Select(u => u.IdUsuario)
                .ToList();

            var empleado = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == idUsuario.Value);

            string nombreEmpleado = empleado != null
                ? $"{empleado.Nombre} {empleado.Apellido}"
                : $"Empleado #{idUsuario.Value}";

            foreach (var adminId in admins)
            {
                NotificacionHelper.NotificarNuevaSolicitud(
                    _context,
                    adminId,
                    nombreEmpleado,
                    nombreTipo);
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
                .Where(s => s.IdUsuario == idUsuario.Value)
                .Select(s => new
                {
                    id = s.IdSolicitud,
                    tipo = s.IdTipoSolicitud,
                    tipoNombre = s.IdTipoSolicitudNavigation.NombreSolicitud.ToLower(),
                    fechaInicio = s.FechaInicio.HasValue ? s.FechaInicio.Value.ToString("yyyy-MM-dd") : null,
                    fechaFin = s.FechaFin.HasValue ? s.FechaFin.Value.ToString("yyyy-MM-dd") : null,
                    fechaInicioAprobada = s.FechaInicioAprobada.HasValue ? s.FechaInicioAprobada.Value.ToString("yyyy-MM-dd") : null,
                    fechaFinAprobada = s.FechaFinAprobada.HasValue ? s.FechaFinAprobada.Value.ToString("yyyy-MM-dd") : null,
                    diasOtorgados = s.DiasOtorgados,
                    diasOtorgadosDetalle = s.DiasOtorgadosDetalle,
                    estado = (s.Estado ?? "").ToLower(),
                    fechaSolicitud = s.FechaSolicitud.ToString("yyyy-MM-dd"),
                    motivo = s.Descripcion,
                    prioridad = "normal",
                    archivos = new List<string>(),
                    rutaArchivo = s.RutaArchivo,
                    nombreArchivo = s.NombreArchivo,
                    descripcionEstado = s.DescripcionEstado
                })
                .ToList();

            return Json(solicitudes);
        }

        [HttpPut]
        public async Task<IActionResult> EditarSolicitud(SolicitudPut model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Unauthorized();
            }

            var solicitud = await _context.Solicituds
                .Include(s => s.IdTipoSolicitudNavigation)
                .FirstOrDefaultAsync(s => s.IdSolicitud == model.IdSolicitud
                                       && s.IdUsuario == idUsuario.Value);

            if (solicitud == null)
            {
                return NotFound();
            }

            if ((solicitud.Estado ?? "").ToLower() != "pendiente")
            {
                return BadRequest("Solo se pueden editar solicitudes pendientes.");
            }

            var tipoSolicitud = await _context.TipoSolicituds
                .FirstOrDefaultAsync(t => t.IdTipoSolicitud == model.IdTipoSolicitud);

            if (tipoSolicitud == null)
            {
                return BadRequest("Seleccione un tipo de solicitud válido.");
            }

            string nombreTipo = tipoSolicitud.NombreSolicitud ?? "";
            bool requiereFechas = TipoRequiereFechas(nombreTipo);
            bool requiereDocumento = TipoRequiereDocumento(nombreTipo);

            if (requiereFechas)
            {
                if (!model.FechaInicio.HasValue || !model.FechaFin.HasValue)
                {
                    return BadRequest("Debe indicar la fecha de inicio y la fecha fin para este tipo de solicitud.");
                }

                if (model.FechaFin.Value.Date < model.FechaInicio.Value.Date)
                {
                    return BadRequest("Rango de fechas inválido.");
                }

                if (model.FechaInicio.Value.Date < AhoraCostaRica().Date)
                {
                    return BadRequest("No puede registrar fechas pasadas.");
                }
            }

            if (model.Archivo != null && model.Archivo.Length > 0)
            {
                if (!ArchivoPermitido(model.Archivo))
                {
                    return BadRequest("Formato de archivo no permitido. Use PDF, JPG o PNG.");
                }

                if (!ArchivoPesoValido(model.Archivo))
                {
                    return BadRequest("El archivo no puede superar los 5MB.");
                }
            }

            string? rutaArchivoFinal = solicitud.RutaArchivo;
            string? nombreArchivoFinal = solicitud.NombreArchivo;

            if (model.EliminarArchivo && !string.IsNullOrEmpty(solicitud.RutaArchivo))
            {
                EliminarArchivoFisico(solicitud.RutaArchivo);
                rutaArchivoFinal = null;
                nombreArchivoFinal = null;
            }

            if (requiereDocumento && string.IsNullOrEmpty(rutaArchivoFinal) && (model.Archivo == null || model.Archivo.Length == 0))
            {
                return BadRequest("Este tipo de solicitud requiere adjuntar un documento de respaldo.");
            }

            solicitud.IdTipoSolicitud = model.IdTipoSolicitud;
            solicitud.FechaInicio = requiereFechas && model.FechaInicio.HasValue ? DateOnly.FromDateTime(model.FechaInicio.Value) : null;
            solicitud.FechaFin = requiereFechas && model.FechaFin.HasValue ? DateOnly.FromDateTime(model.FechaFin.Value) : null;
            solicitud.Descripcion = ConstruirDescripcionSolicitud(model, nombreTipo);
            solicitud.RutaArchivo = rutaArchivoFinal;
            solicitud.NombreArchivo = nombreArchivoFinal;
            solicitud.DescripcionEstado = TipoGestionAdministrativa(nombreTipo)
                ? "Pendiente de revisión administrativa"
                : "Pendiente de aprobación";

            if (model.Archivo != null && model.Archivo.Length > 0)
            {
                if (!string.IsNullOrEmpty(solicitud.RutaArchivo))
                {
                    EliminarArchivoFisico(solicitud.RutaArchivo);
                }

                await GuardarArchivoSolicitud(solicitud, model.Archivo);
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Solicitud actualizada correctamente"
            });
        }

        [HttpPut]
        public IActionResult CancelarSolicitud(int id)
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Unauthorized();
            }

            var solicitud = _context.Solicituds
                .FirstOrDefault(s => s.IdSolicitud == id
                                  && s.IdUsuario == idUsuario.Value);

            if (solicitud == null)
            {
                return NotFound();
            }

            if ((solicitud.Estado ?? "").ToLower() != "pendiente")
            {
                return BadRequest("Solo se pueden cancelar solicitudes pendientes.");
            }

            if (!string.IsNullOrEmpty(solicitud.RutaArchivo))
            {
                EliminarArchivoFisico(solicitud.RutaArchivo);
                solicitud.RutaArchivo = null;
                solicitud.NombreArchivo = null;
            }

            solicitud.Estado = "cancelado";
            solicitud.DescripcionEstado = "Cancelada por el usuario";

            _context.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult HistorialBonos()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Index", "Usuario");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ObtenerHistorialBonos()
        {
            int? idUsuario = HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return Json(new { success = false, message = "Sesión no válida." });
            }

            var bonos = _context.BonosAsignados
                .Include(ba => ba.IdBonoNavigation)
                .Where(ba => ba.IdUsuario == idUsuario.Value)
                .OrderByDescending(ba => ba.Periodo)
                .ThenByDescending(ba => ba.FechaAsignado)
                .Select(ba => new
                {
                    nombreBono = ba.IdBonoNavigation.NombreBono,
                    monto = ba.IdBonoNavigation.Monto,
                    periodo = ba.Periodo,
                    fechaAsignado = ba.FechaAsignado.ToString()
                })
                .ToList();

            return Json(new { success = true, bonos });
        }

        [HttpGet]
        public IActionResult CalendarioFeriados()
        {
            return View();
        }
    }
}