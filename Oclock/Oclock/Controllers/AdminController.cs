using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Filters;
using Oclock.Data;
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

        public IActionResult FeriadosConfig()
        {
            return View();
        }

        public IActionResult GestionDocumentos()
        {
            return View();
        }

        public IActionResult HorariosConfig()
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

        [HttpGet]
        public IActionResult VerMarcas(int? idUsuario, DateOnly? desde, DateOnly? hasta, string? tipo)
        {
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
            ViewBag.FiltroTipo = tipo;

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
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.IdMarca)
                .Take(300)
                .ToList();

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
    }
}