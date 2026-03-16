using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Filters;
using Oclock.Models;

namespace Oclock.Controllers
{
    [AuthorizeRole(1)]
    public class FeriadosController : Controller
    {
        private readonly By5rqco0trg7fpqgnpvmContext _context;

        public FeriadosController(By5rqco0trg7fpqgnpvmContext context)
        {
            _context = context;
        }

        public IActionResult FeriadosConfig()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var feriados = await _context.Feriados
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

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] FeriadoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Fecha))
                return BadRequest(new { mensaje = "Nombre y fecha son requeridos." });

            var fecha = DateOnly.Parse(dto.Fecha);

            bool duplicado = await _context.Feriados.AnyAsync(f => f.Fecha == fecha);
            if (duplicado)
                return Conflict(new { mensaje = "Ya existe un feriado registrado en esa fecha." });

            var nuevo = new Feriado
            {
                Nombre = dto.Nombre,
                Fecha = fecha,
                EsLaborable = dto.EsLaborable,
                Descripcion = dto.Descripcion
            };

            _context.Feriados.Add(nuevo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Feriado creado exitosamente.", id = nuevo.IdFeriado });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var f = await _context.Feriados.FindAsync(id);
            if (f == null) return NotFound();

            return Json(new
            {
                f.IdFeriado,
                f.Nombre,
                Fecha = f.Fecha.ToString("yyyy-MM-dd"),
                f.EsLaborable,
                f.Descripcion
            });
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] FeriadoDto dto)
        {
            if (dto.IdFeriado == null)
                return BadRequest(new { mensaje = "ID requerido." });

            var fecha = DateOnly.Parse(dto.Fecha);

            bool duplicado = await _context.Feriados
                .AnyAsync(f => f.Fecha == fecha && f.IdFeriado != dto.IdFeriado);
            if (duplicado)
                return Conflict(new { mensaje = "Ya existe otro feriado registrado en esa fecha." });

            var feriado = await _context.Feriados.FindAsync(dto.IdFeriado);
            if (feriado == null) return NotFound();

            feriado.Nombre = dto.Nombre;
            feriado.Fecha = fecha;
            feriado.EsLaborable = dto.EsLaborable;
            feriado.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Feriado actualizado correctamente." });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            var feriado = await _context.Feriados.FindAsync(id);
            if (feriado == null) return NotFound();

            _context.Feriados.Remove(feriado);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Feriado eliminado correctamente." });
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
}