using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Filters;
using Oclock.Helpers;
using System;
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

            // 🔹 TAB LOGIC
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

            // 🔹 FILTROS
            if (idUsuario.HasValue)
                query = query.Where(s => s.IdUsuario == idUsuario.Value);

            if (desde.HasValue)
                query = query.Where(s => s.FechaInicio >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(s => s.FechaFin <= hasta.Value);

            // 🔹 PAGINACIÓN
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
                    Estado = char.ToUpper(s.Estado[0]) + s.Estado.Substring(1),
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


        // ── CambiarEstadoSolicitud ── ★ SE AÑADE NOTIFICACIÓN ────────────
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

            // ★ Notificar al empleado dueño de la solicitud
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
            // 1. Obtener todos los empleados activos
            var empleados = _context.Usuarios
                .Where(u => u.IdRol == 2 && u.Activo == true)
                .Select(u => new { u.IdUsuario, Nombre = u.Nombre + " " + u.Apellido })
                .ToList();

            var resultado = new List<object>();

            foreach (var emp in empleados)
            {
                // 2. Marcas del período
                var marcas = _context.Marcas
                    .Where(m => m.IdUsuario == emp.IdUsuario
                             && m.Fecha >= desde
                             && m.Fecha <= hasta)
                    .ToList();

                if (!marcas.Any()) continue;

                // 3. Agrupar por fecha
                var diasAgrupados = marcas.GroupBy(m => m.Fecha).ToList();

                int diasTrabajados = 0;
                int diasPuntuales = 0;
                int tardanzas = 0;
                var minutosEntrada = new List<int>(); // para hora promedio

                foreach (var dia in diasAgrupados)
                {
                    var fecha = dia.Key;

                    // Primera entrada del día
                    var primeraEntrada = dia
                        .Where(m => m.HoraEntrada.HasValue && (m.Nombre ?? "").ToLower() == "entrada")
                        .OrderBy(m => m.HoraEntrada)
                        .FirstOrDefault();

                    if (primeraEntrada == null) continue;

                    diasTrabajados++;

                    // Hora promedio de entrada
                    var te = primeraEntrada.HoraEntrada.Value;
                    minutosEntrada.Add(te.Hour * 60 + te.Minute);

                    // Horario asignado para ese día
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
                        // Sin horario asignado → se considera puntual
                        diasPuntuales++;
                    }
                }

                if (diasTrabajados == 0) continue;

                double puntualidad = Math.Round((double)diasPuntuales / diasTrabajados * 100, 1);

                // Hora promedio de entrada
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

            // Ordenar por puntualidad descendente por defecto
            resultado = resultado
                .OrderByDescending(r => ((dynamic)r).puntualidad)
                .ToList<object>();

            return Json(new { success = true, ranking = resultado });
        }


    }
}