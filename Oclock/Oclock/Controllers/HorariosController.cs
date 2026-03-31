using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Models;

namespace Oclock.Controllers
{
    public class HorariosController : Controller
    {
        private readonly By5rqco0trg7fpqgnpvmContext _context;

        public HorariosController(By5rqco0trg7fpqgnpvmContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> HorariosConfig(int? editarId, int? asignarId)
        {
            var horarios = await _context.Horarios
                .Include(h => h.UsuarioHorarios)
                    .ThenInclude(uh => uh.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdRolNavigation)
                .ToListAsync();

            var usuarios = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Where(u => u.Activo == true)
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();

            ViewBag.Horarios = horarios;
            ViewBag.Usuarios = usuarios;
            ViewBag.TotalAsignados = await _context.UsuarioHorarios.Select(uh => uh.IdUsuario).Distinct().CountAsync();
            ViewBag.EditarId = editarId;
            ViewBag.AsignarId = asignarId;
            ViewBag.HorarioEditando = horarios.FirstOrDefault(h => h.IdHorario == editarId);
            ViewBag.HorarioAsignando = horarios.FirstOrDefault(h => h.IdHorario == asignarId);
            ViewBag.UsuariosYaAsignados = asignarId.HasValue
                ? await _context.UsuarioHorarios
                    .Where(uh => uh.IdHorario == asignarId.Value)
                    .Select(uh => uh.IdUsuario)
                    .ToListAsync()
                : new List<int>();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearHorario(string nombreHorario, string horaEntrada, string horaSalida, string dias)
        {
            if (string.IsNullOrWhiteSpace(nombreHorario))
            {
                TempData["Error"] = "El nombre del horario es requerido.";
                return RedirectToAction(nameof(HorariosConfig));
            }

            _context.Horarios.Add(new Horario
            {
                NombreHorario = nombreHorario,
                HoraEntrada = TimeOnly.Parse(horaEntrada),
                HoraSalida = TimeOnly.Parse(horaSalida),
                Dias = dias
            });

            await _context.SaveChangesAsync();
            TempData["Exito"] = $"Horario '{nombreHorario}' creado exitosamente.";
            return RedirectToAction(nameof(HorariosConfig));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarHorario(int idHorario, string nombreHorario, string horaEntrada, string horaSalida, string dias)
        {
            var horario = await _context.Horarios.FindAsync(idHorario);
            if (horario == null)
            {
                TempData["Error"] = "Horario no encontrado.";
                return RedirectToAction(nameof(HorariosConfig));
            }

            horario.NombreHorario = nombreHorario;
            horario.HoraEntrada = TimeOnly.Parse(horaEntrada);
            horario.HoraSalida = TimeOnly.Parse(horaSalida);
            horario.Dias = dias;

            await _context.SaveChangesAsync();
            TempData["Exito"] = $"Horario '{nombreHorario}' actualizado exitosamente.";
            return RedirectToAction(nameof(HorariosConfig));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarHorario(int idHorario)
        {
            var horario = await _context.Horarios
                .Include(h => h.UsuarioHorarios)
                .FirstOrDefaultAsync(h => h.IdHorario == idHorario);

            if (horario == null)
            {
                TempData["Error"] = "Horario no encontrado.";
                return RedirectToAction(nameof(HorariosConfig));
            }

            if (horario.UsuarioHorarios.Any())
            {
                TempData["Error"] = $"No se puede eliminar '{horario.NombreHorario}' porque tiene {horario.UsuarioHorarios.Count} empleado(s) asignado(s). Desasígnalos primero.";
                return RedirectToAction(nameof(HorariosConfig));
            }

            _context.Horarios.Remove(horario);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Horario eliminado exitosamente.";
            return RedirectToAction(nameof(HorariosConfig));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarUsuarios(int idHorario, List<int> idsUsuarios)
        {
            var horario = await _context.Horarios.FindAsync(idHorario);
            if (horario == null)
            {
                TempData["Error"] = "Horario no encontrado.";
                return RedirectToAction(nameof(HorariosConfig));
            }

            var actuales = await _context.UsuarioHorarios
                .Where(uh => uh.IdHorario == idHorario)
                .ToListAsync();

            _context.UsuarioHorarios.RemoveRange(actuales);

            foreach (var idUsuario in idsUsuarios)
                _context.UsuarioHorarios.Add(new UsuarioHorario { IdHorario = idHorario, IdUsuario = idUsuario });

            await _context.SaveChangesAsync();
            TempData["Exito"] = $"Se asignaron {idsUsuarios.Count} empleado(s) al horario '{horario.NombreHorario}'.";
            return RedirectToAction(nameof(HorariosConfig));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesasignarUsuario(int idHorario, int idUsuario)
        {
            var asignacion = await _context.UsuarioHorarios
                .FirstOrDefaultAsync(uh => uh.IdHorario == idHorario && uh.IdUsuario == idUsuario);

            if (asignacion != null)
            {
                _context.UsuarioHorarios.Remove(asignacion);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Empleado desasignado exitosamente.";
            }

            return RedirectToAction(nameof(HorariosConfig));
        }
    }
}