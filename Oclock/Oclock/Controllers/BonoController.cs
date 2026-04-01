
using Oclock.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Filters;
using Oclock.Data;
using System;
using System.Linq;
using System.Text;

[AuthorizeRole(1)]
public class BonoController : Controller
{
    private readonly By5rqco0trg7fpqgnpvmContext _context;

    public BonoController(By5rqco0trg7fpqgnpvmContext context)
    {
        _context = context;
    }

    public IActionResult GestionBonos()
    {
        var vm = new BonoViewModel
        {
            NuevoBono = new Bono(),
            Bonos = _context.Bonos
                .Include(b => b.IdTipoBonoNavigation)
                .OrderByDescending(b => b.IdBono)
                .ToList(),
            TiposBono = _context.TipoBonos.ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public IActionResult CrearBono(BonoViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Bonos = _context.Bonos.Include(b => b.IdTipoBonoNavigation).ToList();
            vm.TiposBono = _context.TipoBonos.ToList();
            return View("GestionBonos", vm);
        }

        vm.NuevoBono.Activo = true;
        vm.NuevoBono.FechaCreacion = DateOnly.FromDateTime(DateTime.Now);

        _context.Bonos.Add(vm.NuevoBono);
        _context.SaveChanges();

        return RedirectToAction("GestionBonos");
    }

    [HttpPost]
    public IActionResult EditarBono(Bono model)
    {
        var bono = _context.Bonos.FirstOrDefault(b => b.IdBono == model.IdBono);

        if (bono == null)
            return NotFound();

        bono.NombreBono = model.NombreBono;
        bono.IdTipoBono = model.IdTipoBono;
        bono.Monto = model.Monto;
        bono.Descripcion = model.Descripcion;

        _context.SaveChanges();

        return RedirectToAction("GestionBonos");
    }

    [HttpPost]
    public IActionResult DesactivarBono(int id)
    {
        var bono = _context.Bonos.FirstOrDefault(b => b.IdBono == id);

        if (bono == null)
            return NotFound();

        bono.Activo = false;
        _context.SaveChanges();

        return RedirectToAction("GestionBonos");
    }

    [HttpPost]
    public IActionResult EliminarBono(int id)
    {
        var bono = _context.Bonos.FirstOrDefault(b => b.IdBono == id);

        if (bono == null)
            return NotFound();

        _context.Bonos.Remove(bono);
        _context.SaveChanges();

        return RedirectToAction("GestionBonos");
    }

    [HttpPost]
    public IActionResult ActivarBono(int id)
    {
        var bono = _context.Bonos.FirstOrDefault(b => b.IdBono == id);

        if (bono == null)
            return NotFound();

        bono.Activo = true;
        _context.SaveChanges();

        return RedirectToAction("GestionBonos");
    }


    public IActionResult AsignarBono()
    {
        var empleados = _context.Usuarios
            .Where(u => u.Activo == true && u.IdRol == 2)
            .Select(u => new
            {
                u.IdUsuario,
                Nombre = u.Nombre + " " + u.Apellido
            })
            .ToList();

        ViewBag.Empleados = empleados;

        return View();
    }

    [HttpGet]
    public IActionResult ObtenerBonosAplicables(int idUsuario, string periodo)
    {
        if (string.IsNullOrEmpty(periodo))
            return Json(new { success = false });

        var partes = periodo.Split("-");
        int year = int.Parse(partes[0]);
        int month = int.Parse(partes[1]);

        var marcas = _context.Marcas
            .Where(m => m.IdUsuario == idUsuario &&
                   m.Fecha.Year == year &&
                   m.Fecha.Month == month)
            .ToList();

        if (!marcas.Any())
            return Json(new { success = false, message = "No hay marcas para este periodo." });

        // Agrupar por fecha
        var diasAgrupados = marcas.GroupBy(m => m.Fecha).ToList();

        int tardanzas = 0;
        double horasTotales = 0;
        int diasTrabajados = 0;
        int diasPuntuales = 0;

        foreach (var dia in diasAgrupados)
        {
            var fecha = dia.Key;

            // Obtener primera entrada y última salida del día
            var primeraEntrada = dia
                .Where(m => m.HoraEntrada.HasValue)
                .OrderBy(m => m.HoraEntrada)
                .FirstOrDefault();

            var ultimaSalida = dia
                .Where(m => m.HoraSalida.HasValue)
                .OrderByDescending(m => m.HoraSalida)
                .FirstOrDefault();

            if (primeraEntrada == null)
                continue;

            diasTrabajados++;

            // Verificar puntualidad con horario asignado
            var horario = _context.UsuarioHorarios
                .Include(uh => uh.IdHorarioNavigation)
                .FirstOrDefault(uh => uh.IdUsuario == idUsuario &&
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

            // Calcular horas trabajadas
            if (ultimaSalida != null)
            {
                var horas = (ultimaSalida.HoraSalida.Value.ToTimeSpan() - primeraEntrada.HoraEntrada.Value.ToTimeSpan()).TotalHours;
                if (horas > 0)
                    horasTotales += horas;
            }
        }

        double puntualidad = diasTrabajados > 0
            ? ((double)diasPuntuales / diasTrabajados) * 100
            : 0;

        // Evaluar bonos aplicables
        var bonos = _context.Bonos
            .Include(b => b.IdTipoBonoNavigation)
            .Where(b => b.Activo == true)
            .ToList();

        var bonosAplicables = new List<object>();

        foreach (var bono in bonos)
        {
            var metrica = bono.IdTipoBonoNavigation?.MetricaTipo;
            bool aplica = false;
            double valorActual = 0;

            switch (metrica)
            {
                case "puntualidad":
                    valorActual = puntualidad;
                    aplica = puntualidad >= (double)bono.CondicionMinima;
                    break;
                case "puntualidad_premium":
                    valorActual = puntualidad;
                    aplica = puntualidad >= (double)bono.CondicionMinima;
                    break;
                case "asistencia":
                    valorActual = diasTrabajados;
                    aplica = diasTrabajados >= (double)bono.CondicionMinima;
                    break;
                case "horas":
                    valorActual = horasTotales;
                    aplica = horasTotales >= (double)bono.CondicionMinima;
                    break;
            }

            bool yaAsignado = _context.BonosAsignados.Any(ba =>
                ba.IdUsuario == idUsuario &&
                ba.IdBono == bono.IdBono &&
                ba.Periodo == periodo);

            if (aplica && !yaAsignado)
            {
                bonosAplicables.Add(new
                {
                    bono.IdBono,
                    bono.NombreBono,
                    bono.Monto,
                    bono.Descripcion,
                    tipo = bono.IdTipoBonoNavigation?.NombreTipo,
                    valorActual = Math.Round(valorActual, 2),
                    condicionMinima = bono.CondicionMinima
                });
            }
        }

        return Json(new
        {
            success = true,
            puntualidad = Math.Round(puntualidad, 2),
            diasTrabajados,
            horasTotales = Math.Round(horasTotales, 2),
            tardanzas,
            bonosAplicables
        });
    }

    [HttpPost]
    public IActionResult AsignarBono([FromBody] AsignarBonoRequest request)
    {
        if (string.IsNullOrEmpty(request.Periodo))
            return Json(new { success = false, message = "El periodo es requerido." });

        bool yaAsignado = _context.BonosAsignados.Any(ba =>
            ba.IdUsuario == request.IdUsuario &&
            ba.IdBono == request.IdBono &&
            ba.Periodo == request.Periodo);

        if (yaAsignado)
            return Json(new { success = false, message = "Este bono ya fue asignado al empleado en este periodo." });

        var asignacion = new BonoAsignado
        {
            IdUsuario = request.IdUsuario,
            IdBono = request.IdBono,
            Periodo = request.Periodo,
            FechaAsignado = DateOnly.FromDateTime(DateTime.Now)
        };

        _context.BonosAsignados.Add(asignacion);
        _context.SaveChanges();

        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult ObtenerHistorial()
    {
        var historial = _context.BonosAsignados
            .Include(ba => ba.IdBonoNavigation)
            .Include(ba => ba.IdUsuarioNavigation)
            .OrderByDescending(ba => ba.FechaAsignado)
            .Take(20)
            .Select(ba => new
            {
                empleado = ba.IdUsuarioNavigation.Nombre + " " + ba.IdUsuarioNavigation.Apellido,
                bono = ba.IdBonoNavigation.NombreBono,
                ba.Periodo,
                monto = ba.IdBonoNavigation.Monto,
                fecha = ba.FechaAsignado.ToString()
            })
            .ToList();

        return Json(historial);
    }


}