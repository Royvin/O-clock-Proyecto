using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Filters;
using Oclock.Models;
using System.Globalization;
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
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var usuarioRol = HttpContext.Session.GetInt32("UsuarioRol");

                if (!usuarioId.HasValue || !usuarioRol.HasValue)
                {
                    return Unauthorized(new
                    {
                        mensaje = "Su sesión expiró. Inicie sesión nuevamente para consultar los feriados."
                    });
                }

                if (usuarioRol != 1 && usuarioRol != 2)
                {
                    return Unauthorized(new
                    {
                        mensaje = "No tiene permisos para consultar el calendario de feriados."
                    });
                }

                int anioConsulta = ObtenerAnioValido(anio);

                await SincronizarFeriadosCostaRica(anioConsulta);

                var feriados = await _context.Feriados
                    .AsNoTracking()
                    .Where(f => f.Fecha.Year == anioConsulta)
                    .OrderBy(f => f.Fecha)
                    .Select(f => new
                    {
                        f.IdFeriado,
                        f.Nombre,
                        Fecha = f.Fecha.ToString("yyyy-MM-dd"),
                        f.EsLaborable,
                        Descripcion = string.IsNullOrWhiteSpace(f.Descripcion)
                            ? "Sin descripción adicional."
                            : f.Descripcion
                    })
                    .ToListAsync();

                return Json(feriados);
            }
            catch
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudieron cargar los feriados en este momento. Intente nuevamente."
                });
            }
        }

        [AuthorizeRole(1)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] FeriadoDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        mensaje = "No se recibieron los datos del feriado."
                    });
                }

                string nombre = LimpiarTexto(dto.Nombre);
                string descripcion = LimpiarTexto(dto.Descripcion);

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return BadRequest(new
                    {
                        mensaje = "Ingrese el nombre del feriado."
                    });
                }

                if (nombre.Length > 100)
                {
                    return BadRequest(new
                    {
                        mensaje = "El nombre del feriado no puede superar los 100 caracteres."
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Fecha))
                {
                    return BadRequest(new
                    {
                        mensaje = "Seleccione la fecha del feriado."
                    });
                }

                if (!FechaValida(dto.Fecha, out DateOnly fecha))
                {
                    return BadRequest(new
                    {
                        mensaje = "La fecha ingresada no es válida."
                    });
                }

                if (!FechaEnRangoPermitido(fecha))
                {
                    return BadRequest(new
                    {
                        mensaje = "La fecha del feriado está fuera del rango permitido."
                    });
                }

                bool duplicado = await _context.Feriados
                    .AnyAsync(f => f.Fecha == fecha);

                if (duplicado)
                {
                    return Conflict(new
                    {
                        mensaje = "Ya existe un feriado registrado en esa fecha."
                    });
                }

                var nuevo = new Feriado
                {
                    Nombre = nombre,
                    Fecha = fecha,
                    EsLaborable = dto.EsLaborable ?? false,
                    Descripcion = string.IsNullOrWhiteSpace(descripcion)
                        ? "Feriado registrado manualmente."
                        : descripcion
                };

                _context.Feriados.Add(nuevo);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Feriado creado correctamente.",
                    id = nuevo.IdFeriado
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo crear el feriado. Intente nuevamente."
                });
            }
        }

        [AuthorizeRole(1)]
        [HttpGet]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje = "El feriado seleccionado no es válido."
                    });
                }

                var feriado = await _context.Feriados
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.IdFeriado == id);

                if (feriado == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No se encontró el feriado solicitado."
                    });
                }

                return Json(new
                {
                    feriado.IdFeriado,
                    feriado.Nombre,
                    Fecha = feriado.Fecha.ToString("yyyy-MM-dd"),
                    feriado.EsLaborable,
                    Descripcion = string.IsNullOrWhiteSpace(feriado.Descripcion)
                        ? ""
                        : feriado.Descripcion
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo consultar el feriado. Intente nuevamente."
                });
            }
        }

        [AuthorizeRole(1)]
        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] FeriadoDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        mensaje = "No se recibieron los datos del feriado."
                    });
                }

                if (!dto.IdFeriado.HasValue || dto.IdFeriado.Value <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje = "Seleccione un feriado válido para editar."
                    });
                }

                string nombre = LimpiarTexto(dto.Nombre);
                string descripcion = LimpiarTexto(dto.Descripcion);

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return BadRequest(new
                    {
                        mensaje = "Ingrese el nombre del feriado."
                    });
                }

                if (nombre.Length > 100)
                {
                    return BadRequest(new
                    {
                        mensaje = "El nombre del feriado no puede superar los 100 caracteres."
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Fecha))
                {
                    return BadRequest(new
                    {
                        mensaje = "Seleccione la fecha del feriado."
                    });
                }

                if (!FechaValida(dto.Fecha, out DateOnly fecha))
                {
                    return BadRequest(new
                    {
                        mensaje = "La fecha ingresada no es válida."
                    });
                }

                if (!FechaEnRangoPermitido(fecha))
                {
                    return BadRequest(new
                    {
                        mensaje = "La fecha del feriado está fuera del rango permitido."
                    });
                }

                bool duplicado = await _context.Feriados
                    .AnyAsync(f => f.Fecha == fecha && f.IdFeriado != dto.IdFeriado.Value);

                if (duplicado)
                {
                    return Conflict(new
                    {
                        mensaje = "Ya existe otro feriado registrado en esa fecha."
                    });
                }

                var feriado = await _context.Feriados
                    .FirstOrDefaultAsync(f => f.IdFeriado == dto.IdFeriado.Value);

                if (feriado == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No se encontró el feriado solicitado."
                    });
                }

                feriado.Nombre = nombre;
                feriado.Fecha = fecha;
                feriado.EsLaborable = dto.EsLaborable ?? false;
                feriado.Descripcion = string.IsNullOrWhiteSpace(descripcion)
                    ? "Sin descripción adicional."
                    : descripcion;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Feriado actualizado correctamente."
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo actualizar el feriado. Intente nuevamente."
                });
            }
        }

        [AuthorizeRole(1)]
        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje = "Seleccione un feriado válido para eliminar."
                    });
                }

                var feriado = await _context.Feriados
                    .FirstOrDefaultAsync(f => f.IdFeriado == id);

                if (feriado == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No se encontró el feriado solicitado."
                    });
                }

                _context.Feriados.Remove(feriado);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Feriado eliminado correctamente."
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo eliminar el feriado. Intente nuevamente."
                });
            }
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

                    if (!FechaValida(item.Date, out DateOnly fecha))
                    {
                        continue;
                    }

                    bool existe = await _context.Feriados
                        .AnyAsync(f => f.Fecha == fecha);

                    if (existe)
                    {
                        continue;
                    }

                    string nombre = !string.IsNullOrWhiteSpace(item.LocalName)
                        ? item.LocalName
                        : item.Name ?? "Feriado Nacional";

                    var nuevo = new Feriado
                    {
                        Nombre = LimpiarTexto(nombre),
                        Fecha = fecha,
                        EsLaborable = false,
                        Descripcion = "Feriado nacional de Costa Rica."
                    };

                    _context.Feriados.Add(nuevo);
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                // No se muestra error técnico al usuario.
                // Si la sincronización externa falla, el sistema sigue mostrando los feriados guardados localmente.
            }
        }

        private static int ObtenerAnioValido(int? anio)
        {
            int anioActual = AhoraCostaRica().Year;
            int anioConsulta = anio ?? anioActual;

            if (anioConsulta < 1900 || anioConsulta > 2200)
            {
                return anioActual;
            }

            return anioConsulta;
        }

        private static bool FechaValida(string fechaTexto, out DateOnly fecha)
        {
            fecha = default;

            if (string.IsNullOrWhiteSpace(fechaTexto))
            {
                return false;
            }

            return DateOnly.TryParseExact(
                fechaTexto.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out fecha);
        }

        private static bool FechaEnRangoPermitido(DateOnly fecha)
        {
            var fechaMinima = new DateOnly(1900, 1, 1);
            var fechaMaxima = new DateOnly(2200, 12, 31);

            return fecha >= fechaMinima && fecha <= fechaMaxima;
        }

        private static string LimpiarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            return texto.Trim();
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