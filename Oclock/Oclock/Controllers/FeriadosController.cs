using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Filters;
using Oclock.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Oclock.Controllers
{
    public class FeriadosController : Controller
    {
        private readonly By5rqco0trg7fpqgnpvmContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public FeriadosController(By5rqco0trg7fpqgnpvmContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [AuthorizeRole(1)]
        [HttpGet]
        public IActionResult FeriadosConfig()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int? anio)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuarioRol = HttpContext.Session.GetInt32("UsuarioRol");

            if (!usuarioId.HasValue || !usuarioRol.HasValue)
            {
                return Unauthorized(new { mensaje = "Debe iniciar sesión para consultar los feriados." });
            }

            if (usuarioRol != 1 && usuarioRol != 2)
            {
                return Unauthorized(new { mensaje = "No tiene permisos para consultar los feriados." });
            }

            int anioConsulta = anio ?? DateTime.Now.Year;

            if (anioConsulta < 1900 || anioConsulta > 2200)
            {
                anioConsulta = DateTime.Now.Year;
            }

            await SincronizarFeriadosCostaRica(anioConsulta);

            var feriados = await _context.Feriados
                .Where(f => f.Fecha.Year == anioConsulta)
                .OrderBy(f => f.Fecha)
                .Select(f => new
                {
                    f.IdFeriado,
                    f.Nombre,
                    Fecha = f.Fecha.ToString("yyyy-MM-dd"),
                    f.EsLaborable,
                    f.Descripcion
                })
                .ToListAsync();

            return Json(feriados);
        }

        [AuthorizeRole(1)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] FeriadoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Fecha))
            {
                return BadRequest(new { mensaje = "Nombre y fecha son requeridos." });
            }

            if (!DateOnly.TryParse(dto.Fecha, out DateOnly fecha))
            {
                return BadRequest(new { mensaje = "La fecha ingresada no es válida." });
            }

            bool duplicado = await _context.Feriados.AnyAsync(f => f.Fecha == fecha);

            if (duplicado)
            {
                return Conflict(new { mensaje = "Ya existe un feriado registrado en esa fecha." });
            }

            var nuevo = new Feriado
            {
                Nombre = dto.Nombre.Trim(),
                Fecha = fecha,
                EsLaborable = dto.EsLaborable ?? false,
                Descripcion = dto.Descripcion
            };

            _context.Feriados.Add(nuevo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Feriado creado exitosamente.", id = nuevo.IdFeriado });
        }

        [AuthorizeRole(1)]
        [HttpGet]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var feriado = await _context.Feriados.FindAsync(id);

            if (feriado == null)
            {
                return NotFound(new { mensaje = "No se encontró el feriado solicitado." });
            }

            return Json(new
            {
                feriado.IdFeriado,
                feriado.Nombre,
                Fecha = feriado.Fecha.ToString("yyyy-MM-dd"),
                feriado.EsLaborable,
                feriado.Descripcion
            });
        }

        [AuthorizeRole(1)]
        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] FeriadoDto dto)
        {
            if (!dto.IdFeriado.HasValue)
            {
                return BadRequest(new { mensaje = "ID requerido." });
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Fecha))
            {
                return BadRequest(new { mensaje = "Nombre y fecha son requeridos." });
            }

            if (!DateOnly.TryParse(dto.Fecha, out DateOnly fecha))
            {
                return BadRequest(new { mensaje = "La fecha ingresada no es válida." });
            }

            bool duplicado = await _context.Feriados
                .AnyAsync(f => f.Fecha == fecha && f.IdFeriado != dto.IdFeriado.Value);

            if (duplicado)
            {
                return Conflict(new { mensaje = "Ya existe otro feriado registrado en esa fecha." });
            }

            var feriado = await _context.Feriados.FindAsync(dto.IdFeriado.Value);

            if (feriado == null)
            {
                return NotFound(new { mensaje = "No se encontró el feriado solicitado." });
            }

            feriado.Nombre = dto.Nombre.Trim();
            feriado.Fecha = fecha;
            feriado.EsLaborable = dto.EsLaborable ?? false;
            feriado.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Feriado actualizado correctamente." });
        }

        [AuthorizeRole(1)]
        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            var feriado = await _context.Feriados.FindAsync(id);

            if (feriado == null)
            {
                return NotFound(new { mensaje = "No se encontró el feriado solicitado." });
            }

            _context.Feriados.Remove(feriado);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Feriado eliminado correctamente." });
        }

        private async Task SincronizarFeriadosCostaRica(int year)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                string url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/CR";

                var feriadosApi = await client.GetFromJsonAsync<List<NagerHolidayDto>>(url);

                if (feriadosApi == null || feriadosApi.Count == 0)
                {
                    return;
                }

                foreach (var item in feriadosApi)
                {
                    if (string.IsNullOrWhiteSpace(item.Date))
                    {
                        continue;
                    }

                    if (!DateOnly.TryParse(item.Date, out DateOnly fecha))
                    {
                        continue;
                    }

                    bool existe = await _context.Feriados.AnyAsync(f => f.Fecha == fecha);

                    if (existe)
                    {
                        continue;
                    }

                    string nombre = !string.IsNullOrWhiteSpace(item.LocalName)
                        ? item.LocalName
                        : item.Name ?? "Feriado Nacional";

                    var nuevo = new Feriado
                    {
                        Nombre = nombre.Trim(),
                        Fecha = fecha,
                        EsLaborable = false,
                        Descripcion = "Feriado nacional de Costa Rica cargado automáticamente desde API."
                    };

                    _context.Feriados.Add(nuevo);
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
            }
        }
    }

    public class FeriadoDto
    {
        public int? IdFeriado { get; set; }
        public string Nombre { get; set; } = "";
        public string Fecha { get; set; } = "";
        public bool? EsLaborable { get; set; }
        public string? Descripcion { get; set; }
    }

    public class NagerHolidayDto
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("localName")]
        public string? LocalName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }
    }
}