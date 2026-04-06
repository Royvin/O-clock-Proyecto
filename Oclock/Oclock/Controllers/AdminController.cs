using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Filters;
using Oclock.Helpers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Oclock.Controllers
{
    [AuthorizeRole(1)]
    public class AdminController : Controller
    {
        private readonly By5rqco0trg7fpqgnpvmContext _context;

        public AdminController(By5rqco0trg7fpqgnpvmContext context)
        {
            _context = context;
        }

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

        public IActionResult GestionSolicitudes()
        {
            return View();
        }

        [HttpGet]
        public IActionResult VerMarcas(int? idUsuario, DateOnly? desde, DateOnly? hasta, string? tipo, int page = 1)
        {
            const int pageSize = 10;

            if (page < 1)
                page = 1;

            ViewBag.Empleados = _context.Usuarios
                .Where(u => u.IdRol == 2 && u.Activo == true)
                .OrderBy(u => u.Nombre)
                .Select(u => new
                {
                    u.IdUsuario,
                    NombreCompleto = (u.Nombre ?? "") + " " + (u.Apellido ?? "")
                })
                .ToList();

            ViewBag.FiltroIdUsuario = idUsuario;
            ViewBag.FiltroDesde = desde;
            ViewBag.FiltroHasta = hasta;
            ViewBag.FiltroTipo = tipo ?? "";

            var query = _context.Marcas
                .Include(m => m.IdUsuarioNavigation)
                .AsQueryable();

            if (idUsuario.HasValue)
                query = query.Where(m => m.IdUsuario == idUsuario.Value);

            if (desde.HasValue)
                query = query.Where(m => m.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(m => m.Fecha <= hasta.Value);

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var t = tipo.Trim().ToLower();
                query = query.Where(m => m.Nombre == t);
            }

            int totalRegistros = query.Count();
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);

            if (totalPaginas > 0 && page > totalPaginas)
                page = totalPaginas;

            var marcas = query
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.IdMarca)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPaginas;
            ViewBag.TotalRecords = totalRegistros;

            return View(marcas);
        }

        [HttpGet]
        public IActionResult ExportarCsv(int? idUsuario, DateOnly? desde, DateOnly? hasta, string? tipo)
        {
            var query = _context.Marcas
                .Include(m => m.IdUsuarioNavigation)
                .AsQueryable();

            if (idUsuario.HasValue)
                query = query.Where(m => m.IdUsuario == idUsuario.Value);

            if (desde.HasValue)
                query = query.Where(m => m.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(m => m.Fecha <= hasta.Value);

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var t = tipo.Trim().ToLower();
                query = query.Where(m => m.Nombre == t);
            }

            var marcas = query
                .OrderBy(m => m.Fecha)
                .ThenBy(m => m.IdMarca)
                .ToList();

            string CsvEscape(string? value)
            {
                value ??= "";
                if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
                {
                    value = value.Replace("\"", "\"\"");
                    return $"\"{value}\"";
                }
                return value;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Empleado,Fecha,Tipo,Hora,Ubicancia,Comentario");

            foreach (var m in marcas)
            {
                var tipoMarca = (m.Nombre ?? "").ToLower();

                string labelTipo = m.Nombre ?? "";
                if (tipoMarca == "entrada") labelTipo = "Entrada";
                else if (tipoMarca == "salida") labelTipo = "Salida";
                else if (tipoMarca == "almuerzo") labelTipo = "Almuerzo";
                else if (tipoMarca == "descanso") labelTipo = "Descanso";

                string hora = "";
                if (m.HoraEntrada.HasValue) hora = m.HoraEntrada.Value.ToString("HH:mm:ss");
                else if (m.HoraSalida.HasValue) hora = m.HoraSalida.Value.ToString("HH:mm:ss");

                string empleado = m.IdUsuarioNavigation != null
                    ? (m.IdUsuarioNavigation.Nombre ?? "") + " " + (m.IdUsuarioNavigation.Apellido ?? "")
                    : "Usuario " + (m.IdUsuario.HasValue ? m.IdUsuario.Value.ToString() : "");

                sb.Append(CsvEscape(empleado));
                sb.Append(",");
                sb.Append(CsvEscape(m.Fecha.ToString("yyyy-MM-dd")));
                sb.Append(",");
                sb.Append(CsvEscape(labelTipo));
                sb.Append(",");
                sb.Append(CsvEscape(hora));
                sb.Append(",");
                sb.Append(CsvEscape(m.Ubicancia ?? ""));
                sb.Append(",");
                sb.Append(CsvEscape(m.Comentario ?? ""));
                sb.AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "marcas.csv");
        }

        [HttpGet]
        public IActionResult ObtenerSolicitudes(
            string tab = "pendientes",
            string? estado = null,
            int? idUsuario = null,
            DateOnly? desde = null,
            DateOnly? hasta = null,
            int page = 1,
            int pageSize = 5)
        {
            if (page < 1) page = 1;

            var query = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .AsQueryable();

            if (tab == "pendientes")
            {
                query = query.Where(s => s.Estado == "pendiente");
            }
            else if (tab == "historial")
            {
                query = query.Where(s => s.Estado != "pendiente");

                if (!string.IsNullOrEmpty(estado))
                {
                    estado = estado.ToLower();
                    query = query.Where(s => s.Estado == estado);
                }
            }

            if (idUsuario.HasValue)
                query = query.Where(s => s.IdUsuario == idUsuario.Value);

            if (desde.HasValue)
                query = query.Where(s => s.FechaInicio >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(s => s.FechaFin <= hasta.Value);

            int totalRegistros = query.Count();
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);

            if (totalPaginas > 0 && page > totalPaginas)
                page = totalPaginas;

            var solicitudes = query
                .OrderByDescending(s => s.FechaSolicitud)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.IdSolicitud,
                    Colaborador = s.IdUsuarioNavigation.Nombre + " " + s.IdUsuarioNavigation.Apellido,
                    Tipo = s.IdTipoSolicitudNavigation.NombreSolicitud,
                    s.FechaInicio,
                    s.FechaFin,
                    s.FechaSolicitud,
                    Estado = char.ToUpper(s.Estado![0]) + s.Estado.Substring(1),
                    Observaciones = s.DescripcionEstado,
                    rutaArchivo = s.RutaArchivo,
                    nombreArchivo = s.NombreArchivo
                })
                .ToList();

            return Json(new
            {
                data = solicitudes,
                currentPage = page,
                totalPages = totalPaginas,
                totalRegistros
            });
        }

        [HttpPost]
        public IActionResult CambiarEstadoSolicitud(int idSolicitud, string nuevoEstado, string? observacion)
        {
            if (string.IsNullOrEmpty(nuevoEstado))
                return BadRequest(new { success = false, message = "Estado inválido." });

            nuevoEstado = nuevoEstado.ToLower();

            if (nuevoEstado != "aprobada" && nuevoEstado != "rechazada")
                return BadRequest(new { success = false, message = "Estado no permitido." });

            var solicitud = _context.Solicituds.FirstOrDefault(s => s.IdSolicitud == idSolicitud);

            if (solicitud == null)
                return NotFound(new { success = false, message = "Solicitud no encontrada." });

            if (solicitud.Estado != "pendiente")
                return BadRequest(new { success = false, message = "La solicitud ya fue gestionada." });

            if (nuevoEstado == "rechazada" && string.IsNullOrWhiteSpace(observacion))
                return BadRequest(new { success = false, message = "Debe indicar una observación para rechazar." });

            solicitud.Estado = nuevoEstado;
            solicitud.DescripcionEstado = observacion;
            _context.SaveChanges();

            if (solicitud.IdUsuario > 0)
            {
                NotificacionHelper.NotificarCambioSolicitud(
                    _context,
                    solicitud.IdUsuario,
                    nuevoEstado,
                    observacion);
            }

            return Ok(new { success = true, message = $"Solicitud {nuevoEstado} correctamente." });
        }

        [HttpGet]
        public IActionResult ObtenerColaboradores()
        {
            var colaboradores = _context.Usuarios
                .Where(u => u.IdRol == 2)
                .Select(u => new
                {
                    u.IdUsuario,
                    NombreCompleto = u.Nombre + " " + u.Apellido
                })
                .ToList();

            return Json(colaboradores);
        }

        [HttpGet]
        public IActionResult ObtenerEstadisticasSolicitudes()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);

            var query = _context.Solicituds.AsQueryable();

            int pendientes = query.Count(s => s.Estado == "pendiente");

            int aprobadasMes = query.Count(s =>
                s.Estado == "aprobada" &&
                s.FechaSolicitud >= inicioMes);

            int rechazadas = query.Count(s => s.Estado == "rechazada");

            int total = query.Count();

            return Json(new
            {
                pendientes,
                aprobadasMes,
                rechazadas,
                total
            });
        }

        [HttpGet]
        public IActionResult ObtenerRankingPuntualidad(DateOnly desde, DateOnly hasta)
        {
            var empleados = _context.Usuarios
                .Where(u => u.IdRol == 2 && u.Activo == true)
                .Select(u => new { u.IdUsuario, Nombre = u.Nombre + " " + u.Apellido })
                .ToList();

            var resultado = new List<object>();

            foreach (var emp in empleados)
            {
                var marcas = _context.Marcas
                    .Where(m => m.IdUsuario == emp.IdUsuario
                             && m.Fecha >= desde
                             && m.Fecha <= hasta)
                    .ToList();

                if (!marcas.Any()) continue;

                var diasAgrupados = marcas.GroupBy(m => m.Fecha).ToList();

                int diasTrabajados = 0;
                int diasPuntuales = 0;
                int tardanzas = 0;
                var minutosEntrada = new List<int>();

                foreach (var dia in diasAgrupados)
                {
                    var fecha = dia.Key;

                    var primeraEntrada = dia
                        .Where(m => m.HoraEntrada.HasValue && (m.Nombre ?? "").ToLower() == "entrada")
                        .OrderBy(m => m.HoraEntrada)
                        .FirstOrDefault();

                    if (primeraEntrada == null) continue;

                    diasTrabajados++;

                    var te = primeraEntrada.HoraEntrada!.Value;
                    minutosEntrada.Add(te.Hour * 60 + te.Minute);

                    var horario = _context.UsuarioHorarios
                        .Include(uh => uh.IdHorarioNavigation)
                        .FirstOrDefault(uh =>
                            uh.IdUsuario == emp.IdUsuario &&
                            (uh.FechaInicio == null || uh.FechaInicio <= fecha) &&
                            (uh.FechaFin == null || uh.FechaFin >= fecha));

                    if (horario != null)
                    {
                        var horaEsperada = horario.IdHorarioNavigation.HoraEntrada;

                        if (primeraEntrada.HoraEntrada.Value > horaEsperada)
                            tardanzas++;
                        else
                            diasPuntuales++;
                    }
                    else
                    {
                        diasPuntuales++;
                    }
                }

                if (diasTrabajados == 0) continue;

                double puntualidad = Math.Round((double)diasPuntuales / diasTrabajados * 100, 1);

                string horaPromedioEntrada = "—";
                if (minutosEntrada.Any())
                {
                    int promMinutos = (int)minutosEntrada.Average();
                    int h = promMinutos / 60;
                    int m = promMinutos % 60;
                    horaPromedioEntrada = $"{h:D2}:{m:D2}";
                }

                resultado.Add(new
                {
                    nombre = emp.Nombre,
                    diasTrabajados,
                    diasPuntuales,
                    tardanzas,
                    puntualidad,
                    horaPromedioEntrada
                });
            }

            resultado = resultado
                .OrderByDescending(r => ((dynamic)r).puntualidad)
                .ToList<object>();

            return Json(new { success = true, ranking = resultado });
        }

        [HttpGet]
        public IActionResult ObtenerReporteGeneral(DateOnly? desde, DateOnly? hasta)
        {
            var fechaHasta = hasta ?? DateOnly.FromDateTime(DateTime.Today);
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            if (fechaDesde > fechaHasta)
                return BadRequest(new { success = false, message = "El rango de fechas no es válido." });

            var solicitudes = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .Where(s => s.FechaSolicitud >= fechaDesde && s.FechaSolicitud <= fechaHasta)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            var totalEmpleadosActivos = _context.Usuarios.Count(u => u.IdRol == 2 && u.Activo == true);
            var totalMarcas = _context.Marcas.Count(m => m.Fecha >= fechaDesde && m.Fecha <= fechaHasta);

            var detalle = solicitudes.Select(s => new
            {
                IdSolicitud = s.IdSolicitud,
                Colaborador = (s.IdUsuarioNavigation.Nombre ?? "") + " " + (s.IdUsuarioNavigation.Apellido ?? ""),
                TipoSolicitud = s.IdTipoSolicitudNavigation.NombreSolicitud,
                FechaSolicitud = FormatearFecha(s.FechaSolicitud),
                FechaInicio = FormatearFecha(s.FechaInicio),
                FechaFin = FormatearFecha(s.FechaFin),
                Estado = FormatearEstado(s.Estado),
                Observacion = s.DescripcionEstado ?? ""
            }).ToList();

            return Json(new
            {
                success = true,
                resumen = new
                {
                    totalEmpleadosActivos,
                    totalMarcas,
                    totalSolicitudes = solicitudes.Count,
                    pendientes = solicitudes.Count(s => (s.Estado ?? "").ToLower() == "pendiente"),
                    aprobadas = solicitudes.Count(s => (s.Estado ?? "").ToLower() == "aprobada"),
                    rechazadas = solicitudes.Count(s => (s.Estado ?? "").ToLower() == "rechazada"),
                    canceladas = solicitudes.Count(s => (s.Estado ?? "").ToLower() == "cancelada"),
                    desde = FormatearFecha(fechaDesde),
                    hasta = FormatearFecha(fechaHasta)
                },
                detalle
            });
        }

        [HttpGet]
        public IActionResult ExportarReporteGeneralPdf(DateOnly? desde, DateOnly? hasta)
        {
            var fechaHasta = hasta ?? DateOnly.FromDateTime(DateTime.Today);
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            if (fechaDesde > fechaHasta)
                return BadRequest("El rango de fechas no es válido.");

            var solicitudes = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .Where(s => s.FechaSolicitud >= fechaDesde && s.FechaSolicitud <= fechaHasta)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            var totalEmpleadosActivos = _context.Usuarios.Count(u => u.IdRol == 2 && u.Activo == true);
            var totalMarcas = _context.Marcas.Count(m => m.Fecha >= fechaDesde && m.Fecha <= fechaHasta);

            var resumen = new Dictionary<string, string>
            {
                { "Desde", FormatearFecha(fechaDesde) },
                { "Hasta", FormatearFecha(fechaHasta) },
                { "Total empleados activos", totalEmpleadosActivos.ToString() },
                { "Total marcas", totalMarcas.ToString() },
                { "Total solicitudes", solicitudes.Count.ToString() },
                { "Pendientes", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "pendiente").ToString() },
                { "Aprobadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "aprobada").ToString() },
                { "Rechazadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "rechazada").ToString() },
                { "Canceladas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "cancelada").ToString() }
            };

            var columnas = new List<string>
            {
                "Colaborador",
                "Tipo",
                "Fecha Solicitud",
                "Fecha Inicio",
                "Fecha Fin",
                "Estado"
            };

            var filas = solicitudes.Select(s => new List<string>
            {
                (s.IdUsuarioNavigation.Nombre ?? "") + " " + (s.IdUsuarioNavigation.Apellido ?? ""),
                s.IdTipoSolicitudNavigation.NombreSolicitud,
                FormatearFecha(s.FechaSolicitud),
                FormatearFecha(s.FechaInicio),
                FormatearFecha(s.FechaFin),
                FormatearEstado(s.Estado)
            }).ToList();

            var bytes = GenerarPdfGenerico(
                "Reporte General",
                $"Período del {FormatearFecha(fechaDesde)} al {FormatearFecha(fechaHasta)}",
                resumen,
                columnas,
                filas);

            return File(bytes, "application/pdf", $"reporte_general_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public IActionResult ExportarReporteGeneralExcel(DateOnly? desde, DateOnly? hasta)
        {
            var fechaHasta = hasta ?? DateOnly.FromDateTime(DateTime.Today);
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            if (fechaDesde > fechaHasta)
                return BadRequest("El rango de fechas no es válido.");

            var solicitudes = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .Where(s => s.FechaSolicitud >= fechaDesde && s.FechaSolicitud <= fechaHasta)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            var totalEmpleadosActivos = _context.Usuarios.Count(u => u.IdRol == 2 && u.Activo == true);
            var totalMarcas = _context.Marcas.Count(m => m.Fecha >= fechaDesde && m.Fecha <= fechaHasta);

            var resumen = new Dictionary<string, string>
            {
                { "Desde", FormatearFecha(fechaDesde) },
                { "Hasta", FormatearFecha(fechaHasta) },
                { "Total empleados activos", totalEmpleadosActivos.ToString() },
                { "Total marcas", totalMarcas.ToString() },
                { "Total solicitudes", solicitudes.Count.ToString() },
                { "Pendientes", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "pendiente").ToString() },
                { "Aprobadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "aprobada").ToString() },
                { "Rechazadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "rechazada").ToString() },
                { "Canceladas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "cancelada").ToString() }
            };

            var columnas = new List<string>
            {
                "Colaborador",
                "Tipo",
                "Fecha Solicitud",
                "Fecha Inicio",
                "Fecha Fin",
                "Estado",
                "Observación"
            };

            var filas = solicitudes.Select(s => new List<string>
            {
                (s.IdUsuarioNavigation.Nombre ?? "") + " " + (s.IdUsuarioNavigation.Apellido ?? ""),
                s.IdTipoSolicitudNavigation.NombreSolicitud,
                FormatearFecha(s.FechaSolicitud),
                FormatearFecha(s.FechaInicio),
                FormatearFecha(s.FechaFin),
                FormatearEstado(s.Estado),
                s.DescripcionEstado ?? ""
            }).ToList();

            var bytes = GenerarExcelGenerico("Reporte General", resumen, columnas, filas);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"reporte_general_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public IActionResult ObtenerReporteAsistenciaEmpleado(int idUsuario, DateOnly? desde, DateOnly? hasta)
        {
            var fechaHasta = hasta ?? DateOnly.FromDateTime(DateTime.Today);
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            if (fechaDesde > fechaHasta)
                return BadRequest(new { success = false, message = "El rango de fechas no es válido." });

            var empleado = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == idUsuario && u.IdRol == 2);

            if (empleado == null)
                return NotFound(new { success = false, message = "Empleado no encontrado." });

            var marcas = _context.Marcas
                .Where(m => m.IdUsuario == idUsuario && m.Fecha >= fechaDesde && m.Fecha <= fechaHasta)
                .OrderBy(m => m.Fecha)
                .ThenBy(m => m.HoraEntrada)
                .ThenBy(m => m.HoraSalida)
                .ToList();

            var entradas = marcas
                .Where(m => (m.Nombre ?? "").ToLower() == "entrada" && m.HoraEntrada.HasValue)
                .Select(m => m.HoraEntrada!.Value)
                .ToList();

            var detalle = marcas.Select(m => new
            {
                Fecha = FormatearFecha(m.Fecha),
                Tipo = FormatearTipoMarca(m.Nombre),
                Hora = ObtenerHoraMarca(m),
                Ubicacion = m.Ubicancia ?? "",
                Comentario = m.Comentario ?? ""
            }).ToList();

            return Json(new
            {
                success = true,
                empleado = (empleado.Nombre ?? "") + " " + (empleado.Apellido ?? ""),
                resumen = new
                {
                    desde = FormatearFecha(fechaDesde),
                    hasta = FormatearFecha(fechaHasta),
                    totalMarcas = marcas.Count,
                    diasConMarca = marcas.Select(m => m.Fecha).Distinct().Count(),
                    totalEntradas = marcas.Count(m => (m.Nombre ?? "").ToLower() == "entrada"),
                    totalSalidas = marcas.Count(m => (m.Nombre ?? "").ToLower() == "salida"),
                    horaPromedioEntrada = FormatearHoraPromedio(entradas)
                },
                detalle
            });
        }

        [HttpGet]
        public IActionResult ExportarReporteAsistenciaEmpleadoPdf(int idUsuario, DateOnly? desde, DateOnly? hasta)
        {
            var fechaHasta = hasta ?? DateOnly.FromDateTime(DateTime.Today);
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            if (fechaDesde > fechaHasta)
                return BadRequest("El rango de fechas no es válido.");

            var empleado = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == idUsuario && u.IdRol == 2);

            if (empleado == null)
                return NotFound("Empleado no encontrado.");

            var marcas = _context.Marcas
                .Where(m => m.IdUsuario == idUsuario && m.Fecha >= fechaDesde && m.Fecha <= fechaHasta)
                .OrderBy(m => m.Fecha)
                .ThenBy(m => m.HoraEntrada)
                .ThenBy(m => m.HoraSalida)
                .ToList();

            var entradas = marcas
                .Where(m => (m.Nombre ?? "").ToLower() == "entrada" && m.HoraEntrada.HasValue)
                .Select(m => m.HoraEntrada!.Value)
                .ToList();

            var nombreEmpleado = (empleado.Nombre ?? "") + " " + (empleado.Apellido ?? "");

            var resumen = new Dictionary<string, string>
            {
                { "Empleado", nombreEmpleado },
                { "Desde", FormatearFecha(fechaDesde) },
                { "Hasta", FormatearFecha(fechaHasta) },
                { "Total marcas", marcas.Count.ToString() },
                { "Días con marca", marcas.Select(m => m.Fecha).Distinct().Count().ToString() },
                { "Total entradas", marcas.Count(m => (m.Nombre ?? "").ToLower() == "entrada").ToString() },
                { "Total salidas", marcas.Count(m => (m.Nombre ?? "").ToLower() == "salida").ToString() },
                { "Hora promedio de entrada", FormatearHoraPromedio(entradas) }
            };

            var columnas = new List<string>
            {
                "Fecha",
                "Tipo",
                "Hora",
                "Ubicación",
                "Comentario"
            };

            var filas = marcas.Select(m => new List<string>
            {
                FormatearFecha(m.Fecha),
                FormatearTipoMarca(m.Nombre),
                ObtenerHoraMarca(m),
                m.Ubicancia ?? "",
                m.Comentario ?? ""
            }).ToList();

            var bytes = GenerarPdfGenerico(
                "Asistencia por Empleado",
                $"Empleado: {nombreEmpleado}",
                resumen,
                columnas,
                filas);

            return File(bytes, "application/pdf", $"asistencia_empleado_{idUsuario}_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public IActionResult ExportarReporteAsistenciaEmpleadoExcel(int idUsuario, DateOnly? desde, DateOnly? hasta)
        {
            var fechaHasta = hasta ?? DateOnly.FromDateTime(DateTime.Today);
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            if (fechaDesde > fechaHasta)
                return BadRequest("El rango de fechas no es válido.");

            var empleado = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == idUsuario && u.IdRol == 2);

            if (empleado == null)
                return NotFound("Empleado no encontrado.");

            var marcas = _context.Marcas
                .Where(m => m.IdUsuario == idUsuario && m.Fecha >= fechaDesde && m.Fecha <= fechaHasta)
                .OrderBy(m => m.Fecha)
                .ThenBy(m => m.HoraEntrada)
                .ThenBy(m => m.HoraSalida)
                .ToList();

            var entradas = marcas
                .Where(m => (m.Nombre ?? "").ToLower() == "entrada" && m.HoraEntrada.HasValue)
                .Select(m => m.HoraEntrada!.Value)
                .ToList();

            var nombreEmpleado = (empleado.Nombre ?? "") + " " + (empleado.Apellido ?? "");

            var resumen = new Dictionary<string, string>
            {
                { "Empleado", nombreEmpleado },
                { "Desde", FormatearFecha(fechaDesde) },
                { "Hasta", FormatearFecha(fechaHasta) },
                { "Total marcas", marcas.Count.ToString() },
                { "Días con marca", marcas.Select(m => m.Fecha).Distinct().Count().ToString() },
                { "Total entradas", marcas.Count(m => (m.Nombre ?? "").ToLower() == "entrada").ToString() },
                { "Total salidas", marcas.Count(m => (m.Nombre ?? "").ToLower() == "salida").ToString() },
                { "Hora promedio de entrada", FormatearHoraPromedio(entradas) }
            };

            var columnas = new List<string>
            {
                "Fecha",
                "Tipo",
                "Hora",
                "Ubicación",
                "Comentario"
            };

            var filas = marcas.Select(m => new List<string>
            {
                FormatearFecha(m.Fecha),
                FormatearTipoMarca(m.Nombre),
                ObtenerHoraMarca(m),
                m.Ubicancia ?? "",
                m.Comentario ?? ""
            }).ToList();

            var bytes = GenerarExcelGenerico("Asistencia por Empleado", resumen, columnas, filas);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"asistencia_empleado_{idUsuario}_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public IActionResult ObtenerReporteAusenciasMensuales(int anio, int mes, string? tipoAusencia = null)
        {
            if (anio < 2000 || anio > 2100 || mes < 1 || mes > 12)
                return BadRequest(new { success = false, message = "Mes o año inválidos." });

            var inicioMes = new DateOnly(anio, mes, 1);
            var finMes = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));
            var filtroTipo = NormalizarTexto(tipoAusencia);

            var solicitudesBase = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .ToList()
                .Where(s =>
                {
                    var nombreTipo = s.IdTipoSolicitudNavigation?.NombreSolicitud ?? "";

                    if (!EsTipoAusenciaPermitido(nombreTipo, filtroTipo))
                        return false;

                    var fechaInicio = s.FechaInicio ?? s.FechaSolicitud;
                    var fechaFin = s.FechaFin ?? s.FechaInicio ?? s.FechaSolicitud;

                    return fechaInicio <= finMes && fechaFin >= inicioMes;
                })
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            var detalle = solicitudesBase.Select(s =>
            {
                var fechaInicio = s.FechaInicio ?? s.FechaSolicitud;
                var fechaFin = s.FechaFin ?? s.FechaInicio ?? s.FechaSolicitud;
                var inicioCruce = fechaInicio > inicioMes ? fechaInicio : inicioMes;
                var finCruce = fechaFin < finMes ? fechaFin : finMes;
                var diasDentroDelMes = finCruce.DayNumber - inicioCruce.DayNumber + 1;

                return new
                {
                    IdSolicitud = s.IdSolicitud,
                    Colaborador = (s.IdUsuarioNavigation?.Nombre ?? "") + " " + (s.IdUsuarioNavigation?.Apellido ?? ""),
                    TipoAusencia = ObtenerEtiquetaTipoAusencia(s.IdTipoSolicitudNavigation?.NombreSolicitud),
                    FechaInicio = FormatearFecha(fechaInicio),
                    FechaFin = FormatearFecha(fechaFin),
                    DiasEnMes = diasDentroDelMes,
                    Estado = FormatearEstado(s.Estado),
                    Observacion = s.DescripcionEstado ?? ""
                };
            }).ToList();

            var nombreMes = ObtenerNombreMes(anio, mes);

            return Json(new
            {
                success = true,
                resumen = new
                {
                    mes = nombreMes,
                    totalSolicitudes = detalle.Count,
                    empleadosImpactados = detalle.Select(d => d.Colaborador).Distinct().Count(),
                    totalDias = detalle.Sum(d => d.DiasEnMes),
                    pendientes = detalle.Count(d => d.Estado.ToLower() == "pendiente"),
                    aprobadas = detalle.Count(d => d.Estado.ToLower() == "aprobada"),
                    rechazadas = detalle.Count(d => d.Estado.ToLower() == "rechazada"),
                    canceladas = detalle.Count(d => d.Estado.ToLower() == "cancelada")
                },
                detalle
            });
        }

        [HttpGet]
        public IActionResult ExportarReporteAusenciasMensualesPdf(int anio, int mes, string? tipoAusencia = null)
        {
            if (anio < 2000 || anio > 2100 || mes < 1 || mes > 12)
                return BadRequest("Mes o año inválidos.");

            var inicioMes = new DateOnly(anio, mes, 1);
            var finMes = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));
            var filtroTipo = NormalizarTexto(tipoAusencia);

            var solicitudes = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .ToList()
                .Where(s =>
                {
                    var nombreTipo = s.IdTipoSolicitudNavigation?.NombreSolicitud ?? "";

                    if (!EsTipoAusenciaPermitido(nombreTipo, filtroTipo))
                        return false;

                    var fechaInicio = s.FechaInicio ?? s.FechaSolicitud;
                    var fechaFin = s.FechaFin ?? s.FechaInicio ?? s.FechaSolicitud;

                    return fechaInicio <= finMes && fechaFin >= inicioMes;
                })
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            var filas = solicitudes.Select(s =>
            {
                var fechaInicio = s.FechaInicio ?? s.FechaSolicitud;
                var fechaFin = s.FechaFin ?? s.FechaInicio ?? s.FechaSolicitud;
                var inicioCruce = fechaInicio > inicioMes ? fechaInicio : inicioMes;
                var finCruce = fechaFin < finMes ? fechaFin : finMes;
                var diasDentroDelMes = finCruce.DayNumber - inicioCruce.DayNumber + 1;

                return new List<string>
                {
                    (s.IdUsuarioNavigation?.Nombre ?? "") + " " + (s.IdUsuarioNavigation?.Apellido ?? ""),
                    ObtenerEtiquetaTipoAusencia(s.IdTipoSolicitudNavigation?.NombreSolicitud),
                    FormatearFecha(fechaInicio),
                    FormatearFecha(fechaFin),
                    diasDentroDelMes.ToString(),
                    FormatearEstado(s.Estado)
                };
            }).ToList();

            var resumen = new Dictionary<string, string>
            {
                { "Mes", ObtenerNombreMes(anio, mes) },
                { "Tipo", string.IsNullOrWhiteSpace(tipoAusencia) || NormalizarTexto(tipoAusencia) == "todos" ? "Todos" : tipoAusencia! },
                { "Total solicitudes", solicitudes.Count.ToString() },
                { "Empleados impactados", filas.Select(f => f[0]).Distinct().Count().ToString() },
                { "Total días", filas.Sum(f => int.Parse(f[4])).ToString() },
                { "Pendientes", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "pendiente").ToString() },
                { "Aprobadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "aprobada").ToString() },
                { "Rechazadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "rechazada").ToString() },
                { "Canceladas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "cancelada").ToString() }
            };

            var columnas = new List<string>
            {
                "Colaborador",
                "Tipo",
                "Fecha Inicio",
                "Fecha Fin",
                "Días en Mes",
                "Estado"
            };

            var bytes = GenerarPdfGenerico(
                "Ausencias Mensuales",
                $"Período: {ObtenerNombreMes(anio, mes)}",
                resumen,
                columnas,
                filas);

            return File(bytes, "application/pdf", $"ausencias_mensuales_{anio}_{mes:D2}.pdf");
        }

        [HttpGet]
        public IActionResult ExportarReporteAusenciasMensualesExcel(int anio, int mes, string? tipoAusencia = null)
        {
            if (anio < 2000 || anio > 2100 || mes < 1 || mes > 12)
                return BadRequest("Mes o año inválidos.");

            var inicioMes = new DateOnly(anio, mes, 1);
            var finMes = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));
            var filtroTipo = NormalizarTexto(tipoAusencia);

            var solicitudes = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .ToList()
                .Where(s =>
                {
                    var nombreTipo = s.IdTipoSolicitudNavigation?.NombreSolicitud ?? "";

                    if (!EsTipoAusenciaPermitido(nombreTipo, filtroTipo))
                        return false;

                    var fechaInicio = s.FechaInicio ?? s.FechaSolicitud;
                    var fechaFin = s.FechaFin ?? s.FechaInicio ?? s.FechaSolicitud;

                    return fechaInicio <= finMes && fechaFin >= inicioMes;
                })
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            var filas = solicitudes.Select(s =>
            {
                var fechaInicio = s.FechaInicio ?? s.FechaSolicitud;
                var fechaFin = s.FechaFin ?? s.FechaInicio ?? s.FechaSolicitud;
                var inicioCruce = fechaInicio > inicioMes ? fechaInicio : inicioMes;
                var finCruce = fechaFin < finMes ? fechaFin : finMes;
                var diasDentroDelMes = finCruce.DayNumber - inicioCruce.DayNumber + 1;

                return new List<string>
                {
                    (s.IdUsuarioNavigation?.Nombre ?? "") + " " + (s.IdUsuarioNavigation?.Apellido ?? ""),
                    ObtenerEtiquetaTipoAusencia(s.IdTipoSolicitudNavigation?.NombreSolicitud),
                    FormatearFecha(fechaInicio),
                    FormatearFecha(fechaFin),
                    diasDentroDelMes.ToString(),
                    FormatearEstado(s.Estado),
                    s.DescripcionEstado ?? ""
                };
            }).ToList();

            var resumen = new Dictionary<string, string>
            {
                { "Mes", ObtenerNombreMes(anio, mes) },
                { "Tipo", string.IsNullOrWhiteSpace(tipoAusencia) || NormalizarTexto(tipoAusencia) == "todos" ? "Todos" : tipoAusencia! },
                { "Total solicitudes", solicitudes.Count.ToString() },
                { "Empleados impactados", filas.Select(f => f[0]).Distinct().Count().ToString() },
                { "Total días", filas.Sum(f => int.Parse(f[4])).ToString() },
                { "Pendientes", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "pendiente").ToString() },
                { "Aprobadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "aprobada").ToString() },
                { "Rechazadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "rechazada").ToString() },
                { "Canceladas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "cancelada").ToString() }
            };

            var columnas = new List<string>
            {
                "Colaborador",
                "Tipo",
                "Fecha Inicio",
                "Fecha Fin",
                "Días en Mes",
                "Estado",
                "Observación"
            };

            var bytes = GenerarExcelGenerico("Ausencias Mensuales", resumen, columnas, filas);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ausencias_mensuales_{anio}_{mes:D2}.xlsx");
        }

        [HttpGet]
        public IActionResult ObtenerReporteSeguimientoSolicitudes(string? estado = null, DateOnly? desde = null, DateOnly? hasta = null)
        {
            var fechaHasta = hasta ?? DateOnly.FromDateTime(DateTime.Today);
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            if (fechaDesde > fechaHasta)
                return BadRequest(new { success = false, message = "El rango de fechas no es válido." });

            var filtroEstado = (estado ?? "").Trim().ToLower();

            var query = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .Where(s => s.FechaSolicitud >= fechaDesde && s.FechaSolicitud <= fechaHasta);

            if (!string.IsNullOrWhiteSpace(filtroEstado) && filtroEstado != "todos")
                query = query.Where(s => (s.Estado ?? "").ToLower() == filtroEstado);

            var solicitudes = query
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            var detalle = solicitudes.Select(s => new
            {
                IdSolicitud = s.IdSolicitud,
                Colaborador = (s.IdUsuarioNavigation.Nombre ?? "") + " " + (s.IdUsuarioNavigation.Apellido ?? ""),
                TipoSolicitud = s.IdTipoSolicitudNavigation.NombreSolicitud,
                FechaSolicitud = FormatearFecha(s.FechaSolicitud),
                FechaInicio = FormatearFecha(s.FechaInicio),
                FechaFin = FormatearFecha(s.FechaFin),
                Estado = FormatearEstado(s.Estado),
                Observacion = s.DescripcionEstado ?? "",
                Archivo = s.NombreArchivo ?? ""
            }).ToList();

            return Json(new
            {
                success = true,
                resumen = new
                {
                    desde = FormatearFecha(fechaDesde),
                    hasta = FormatearFecha(fechaHasta),
                    totalSolicitudes = solicitudes.Count,
                    pendientes = solicitudes.Count(s => (s.Estado ?? "").ToLower() == "pendiente"),
                    aprobadas = solicitudes.Count(s => (s.Estado ?? "").ToLower() == "aprobada"),
                    rechazadas = solicitudes.Count(s => (s.Estado ?? "").ToLower() == "rechazada"),
                    canceladas = solicitudes.Count(s => (s.Estado ?? "").ToLower() == "cancelada")
                },
                detalle
            });
        }

        [HttpGet]
        public IActionResult ExportarReporteSeguimientoSolicitudesPdf(string? estado = null, DateOnly? desde = null, DateOnly? hasta = null)
        {
            var fechaHasta = hasta ?? DateOnly.FromDateTime(DateTime.Today);
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            if (fechaDesde > fechaHasta)
                return BadRequest("El rango de fechas no es válido.");

            var filtroEstado = (estado ?? "").Trim().ToLower();

            var query = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .Where(s => s.FechaSolicitud >= fechaDesde && s.FechaSolicitud <= fechaHasta);

            if (!string.IsNullOrWhiteSpace(filtroEstado) && filtroEstado != "todos")
                query = query.Where(s => (s.Estado ?? "").ToLower() == filtroEstado);

            var solicitudes = query
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            var resumen = new Dictionary<string, string>
            {
                { "Desde", FormatearFecha(fechaDesde) },
                { "Hasta", FormatearFecha(fechaHasta) },
                { "Estado", string.IsNullOrWhiteSpace(estado) ? "Todos" : FormatearEstado(estado) },
                { "Total solicitudes", solicitudes.Count.ToString() },
                { "Pendientes", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "pendiente").ToString() },
                { "Aprobadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "aprobada").ToString() },
                { "Rechazadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "rechazada").ToString() },
                { "Canceladas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "cancelada").ToString() }
            };

            var columnas = new List<string>
            {
                "Colaborador",
                "Tipo",
                "Fecha Solicitud",
                "Fecha Inicio",
                "Fecha Fin",
                "Estado"
            };

            var filas = solicitudes.Select(s => new List<string>
            {
                (s.IdUsuarioNavigation.Nombre ?? "") + " " + (s.IdUsuarioNavigation.Apellido ?? ""),
                s.IdTipoSolicitudNavigation.NombreSolicitud,
                FormatearFecha(s.FechaSolicitud),
                FormatearFecha(s.FechaInicio),
                FormatearFecha(s.FechaFin),
                FormatearEstado(s.Estado)
            }).ToList();

            var bytes = GenerarPdfGenerico(
                "Seguimiento de Solicitudes",
                $"Período del {FormatearFecha(fechaDesde)} al {FormatearFecha(fechaHasta)}",
                resumen,
                columnas,
                filas);

            return File(bytes, "application/pdf", $"seguimiento_solicitudes_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public IActionResult ExportarReporteSeguimientoSolicitudesExcel(string? estado = null, DateOnly? desde = null, DateOnly? hasta = null)
        {
            var fechaHasta = hasta ?? DateOnly.FromDateTime(DateTime.Today);
            var fechaDesde = desde ?? fechaHasta.AddDays(-30);

            if (fechaDesde > fechaHasta)
                return BadRequest("El rango de fechas no es válido.");

            var filtroEstado = (estado ?? "").Trim().ToLower();

            var query = _context.Solicituds
                .Include(s => s.IdUsuarioNavigation)
                .Include(s => s.IdTipoSolicitudNavigation)
                .Where(s => s.FechaSolicitud >= fechaDesde && s.FechaSolicitud <= fechaHasta);

            if (!string.IsNullOrWhiteSpace(filtroEstado) && filtroEstado != "todos")
                query = query.Where(s => (s.Estado ?? "").ToLower() == filtroEstado);

            var solicitudes = query
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            var resumen = new Dictionary<string, string>
            {
                { "Desde", FormatearFecha(fechaDesde) },
                { "Hasta", FormatearFecha(fechaHasta) },
                { "Estado", string.IsNullOrWhiteSpace(estado) ? "Todos" : FormatearEstado(estado) },
                { "Total solicitudes", solicitudes.Count.ToString() },
                { "Pendientes", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "pendiente").ToString() },
                { "Aprobadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "aprobada").ToString() },
                { "Rechazadas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "rechazada").ToString() },
                { "Canceladas", solicitudes.Count(s => (s.Estado ?? "").ToLower() == "cancelada").ToString() }
            };

            var columnas = new List<string>
            {
                "Colaborador",
                "Tipo",
                "Fecha Solicitud",
                "Fecha Inicio",
                "Fecha Fin",
                "Estado",
                "Observación",
                "Archivo"
            };

            var filas = solicitudes.Select(s => new List<string>
            {
                (s.IdUsuarioNavigation.Nombre ?? "") + " " + (s.IdUsuarioNavigation.Apellido ?? ""),
                s.IdTipoSolicitudNavigation.NombreSolicitud,
                FormatearFecha(s.FechaSolicitud),
                FormatearFecha(s.FechaInicio),
                FormatearFecha(s.FechaFin),
                FormatearEstado(s.Estado),
                s.DescripcionEstado ?? "",
                s.NombreArchivo ?? ""
            }).ToList();

            var bytes = GenerarExcelGenerico("Seguimiento de Solicitudes", resumen, columnas, filas);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"seguimiento_solicitudes_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.xlsx");
        }

        private static string FormatearFecha(DateOnly? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy") : "—";
        }

        private static string FormatearEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return "—";

            estado = estado.Trim().ToLower();
            return char.ToUpper(estado[0]) + estado.Substring(1);
        }

        private static string FormatearTipoMarca(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return "—";

            tipo = tipo.Trim().ToLower();

            if (tipo == "entrada") return "Entrada";
            if (tipo == "salida") return "Salida";
            if (tipo == "almuerzo") return "Almuerzo";
            if (tipo == "descanso") return "Descanso";

            return char.ToUpper(tipo[0]) + tipo.Substring(1);
        }

        private static string ObtenerHoraMarca(dynamic marca)
        {
            if (marca.HoraEntrada != null)
                return ((TimeOnly)marca.HoraEntrada).ToString("HH:mm");

            if (marca.HoraSalida != null)
                return ((TimeOnly)marca.HoraSalida).ToString("HH:mm");

            return "—";
        }

        private static string FormatearHoraPromedio(List<TimeOnly> horas)
        {
            if (horas == null || !horas.Any())
                return "—";

            var promedioMinutos = (int)horas.Average(h => h.Hour * 60 + h.Minute);
            var hora = promedioMinutos / 60;
            var minuto = promedioMinutos % 60;

            return $"{hora:D2}:{minuto:D2}";
        }

        private static string ObtenerNombreMes(int anio, int mes)
        {
            var cultura = new CultureInfo("es-CR");
            var nombre = new DateTime(anio, mes, 1).ToString("MMMM yyyy", cultura);
            return cultura.TextInfo.ToTitleCase(nombre);
        }

        private static string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            var normalizado = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalizado)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static bool EsTipoAusenciaPermitido(string? nombreTipo, string? filtroTipo)
        {
            var tipo = NormalizarTexto(nombreTipo);
            var filtro = NormalizarTexto(filtroTipo);

            var categoriaDetectada = ObtenerCategoriaAusencia(tipo);

            if (string.IsNullOrWhiteSpace(categoriaDetectada))
                return false;

            if (string.IsNullOrWhiteSpace(filtro) || filtro == "todos")
                return true;

            return categoriaDetectada == filtro;
        }

        private static string ObtenerEtiquetaTipoAusencia(string? nombreTipo)
        {
            var categoria = ObtenerCategoriaAusencia(NormalizarTexto(nombreTipo));

            if (categoria == "vacaciones") return "Vacaciones";
            if (categoria == "permiso") return "Permiso";
            if (categoria == "incapacidad") return "Incapacidad";
            if (categoria == "licencia") return "Licencia";

            return nombreTipo ?? "—";
        }

        private static string ObtenerCategoriaAusencia(string? tipoNormalizado)
        {
            if (string.IsNullOrWhiteSpace(tipoNormalizado))
                return "";

            if (tipoNormalizado.Contains("vacacion"))
                return "vacaciones";

            if (tipoNormalizado.Contains("permiso"))
                return "permiso";

            if (tipoNormalizado.Contains("incapacidad"))
                return "incapacidad";

            if (tipoNormalizado.Contains("licencia"))
                return "licencia";

            return "";
        }

        private byte[] GenerarExcelGenerico(string titulo, Dictionary<string, string> resumen, List<string> columnas, List<List<string>> filas)
        {
            using var workbook = new XLWorkbook();

            var wsResumen = workbook.Worksheets.Add("Resumen");
            wsResumen.Cell(1, 1).Value = titulo;
            wsResumen.Cell(1, 1).Style.Font.Bold = true;
            wsResumen.Cell(1, 1).Style.Font.FontSize = 16;

            int filaResumen = 3;
            foreach (var item in resumen)
            {
                wsResumen.Cell(filaResumen, 1).Value = item.Key;
                wsResumen.Cell(filaResumen, 2).Value = item.Value;
                wsResumen.Cell(filaResumen, 1).Style.Font.Bold = true;
                filaResumen++;
            }

            wsResumen.Columns().AdjustToContents();

            var wsDetalle = workbook.Worksheets.Add("Detalle");

            for (int i = 0; i < columnas.Count; i++)
            {
                wsDetalle.Cell(1, i + 1).Value = columnas[i];
                wsDetalle.Cell(1, i + 1).Style.Font.Bold = true;
                wsDetalle.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2F6FED");
                wsDetalle.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            for (int i = 0; i < filas.Count; i++)
            {
                for (int j = 0; j < filas[i].Count; j++)
                {
                    wsDetalle.Cell(i + 2, j + 1).Value = filas[i][j];
                }
            }

            wsDetalle.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private byte[] GenerarPdfGenerico(string titulo, string subtitulo, Dictionary<string, string> resumen, List<string> columnas, List<List<string>> filas)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Text(titulo).FontSize(18).Bold().FontColor("#2F6FED");
                        column.Item().Text(subtitulo).FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().Column(column =>
                    {
                        column.Item().PaddingVertical(10).Text("Resumen").Bold().FontSize(12);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columnsDef =>
                            {
                                columnsDef.RelativeColumn(2);
                                columnsDef.RelativeColumn(3);
                            });

                            foreach (var item in resumen)
                            {
                                table.Cell().Element(CellStyleResumenTitulo).Text(item.Key).SemiBold();
                                table.Cell().Element(CellStyleResumenValor).Text(item.Value);
                            }
                        });

                        column.Item().PaddingTop(15).PaddingBottom(8).Text("Detalle").Bold().FontSize(12);

                        if (filas.Any())
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columnsDef =>
                                {
                                    for (int i = 0; i < columnas.Count; i++)
                                        columnsDef.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    foreach (var columna in columnas)
                                    {
                                        header.Cell().Element(CellStyleHeader).Text(columna).FontColor(Colors.White).SemiBold();
                                    }
                                });

                                foreach (var fila in filas)
                                {
                                    foreach (var valor in fila)
                                    {
                                        table.Cell().Element(CellStyleBody).Text(valor ?? "");
                                    }
                                }
                            });
                        }
                        else
                        {
                            column.Item().PaddingTop(10).Text("No hay datos disponibles para este reporte.");
                        }
                    });

                    page.Footer().AlignCenter().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            }).GeneratePdf();
        }

        private static IContainer CellStyleHeader(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background("#2F6FED")
                .Padding(5);
        }

        private static IContainer CellStyleBody(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }

        private static IContainer CellStyleResumenTitulo(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4)
                .Padding(5);
        }

        private static IContainer CellStyleResumenValor(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }
    }
}