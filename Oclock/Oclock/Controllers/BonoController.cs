using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oclock.Data;
using Oclock.Filters;
using Oclock.Helpers;
using Oclock.Models;
using System;
using System.Collections.Generic;
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

        if (TempData["SuccessMessage"] != null)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
        }

        if (TempData["ErrorMessage"] != null)
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
        }

        return View(vm);
    }

    [HttpPost]
    public IActionResult CrearBono(BonoViewModel vm)
    {
        if (vm.NuevoBono == null)
        {
            ModelState.AddModelError("", "Debe ingresar la información del bono.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(vm.NuevoBono.NombreBono))
            {
                ModelState.AddModelError("NuevoBono.NombreBono", "El nombre del bono es requerido.");
            }

            if (vm.NuevoBono.IdTipoBono <= 0)
            {
                ModelState.AddModelError("NuevoBono.IdTipoBono", "Debe seleccionar un tipo de bono válido.");
            }

            if (vm.NuevoBono.Monto <= 0)
            {
                ModelState.AddModelError("NuevoBono.Monto", "El monto del bono debe ser mayor a 0.");
            }
        }

        if (!ModelState.IsValid)
        {
            vm.Bonos = _context.Bonos
                .Include(b => b.IdTipoBonoNavigation)
                .OrderByDescending(b => b.IdBono)
                .ToList();

            vm.TiposBono = _context.TipoBonos.ToList();

            ViewBag.ErrorMessage = "No se pudo crear el bono. Revise los datos ingresados.";
            return View("GestionBonos", vm);
        }

        vm.NuevoBono.NombreBono = vm.NuevoBono.NombreBono?.Trim();
        vm.NuevoBono.Descripcion = vm.NuevoBono.Descripcion?.Trim();
        vm.NuevoBono.Activo = true;
        vm.NuevoBono.FechaCreacion = DateOnly.FromDateTime(DateTime.Now);

        _context.Bonos.Add(vm.NuevoBono);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Bono creado correctamente.";
        return RedirectToAction("GestionBonos");
    }

    [HttpPost]
    public IActionResult EditarBono(Bono model)
    {
        var bono = _context.Bonos.FirstOrDefault(b => b.IdBono == model.IdBono);

        if (bono == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(model.NombreBono))
        {
            TempData["ErrorMessage"] = "El nombre del bono es requerido.";
            return RedirectToAction("GestionBonos");
        }

        if (model.IdTipoBono <= 0)
        {
            TempData["ErrorMessage"] = "Debe seleccionar un tipo de bono válido.";
            return RedirectToAction("GestionBonos");
        }

        if (model.Monto <= 0)
        {
            TempData["ErrorMessage"] = "El monto del bono debe ser mayor a 0. No se permiten bonos negativos ni en cero.";
            return RedirectToAction("GestionBonos");
        }

        bono.NombreBono = model.NombreBono.Trim();
        bono.IdTipoBono = model.IdTipoBono;
        bono.Monto = model.Monto;
        bono.Descripcion = model.Descripcion?.Trim();

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Bono actualizado correctamente.";
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

        TempData["SuccessMessage"] = "Bono desactivado correctamente.";
        return RedirectToAction("GestionBonos");
    }

    [HttpPost]
    public IActionResult ActivarBono(int id)
    {
        var bono = _context.Bonos.FirstOrDefault(b => b.IdBono == id);

        if (bono == null)
            return NotFound();

        if (bono.Monto <= 0)
        {
            TempData["ErrorMessage"] = "No se puede activar un bono con monto menor o igual a 0.";
            return RedirectToAction("GestionBonos");
        }

        bono.Activo = true;
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Bono activado correctamente.";
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
        if (idUsuario <= 0)
            return Json(new { success = false, message = "Debe seleccionar un empleado válido." });

        if (string.IsNullOrWhiteSpace(periodo))
            return Json(new { success = false, message = "El periodo es requerido." });

        var partes = periodo.Split("-");

        if (partes.Length != 2 ||
            !int.TryParse(partes[0], out int year) ||
            !int.TryParse(partes[1], out int month) ||
            month < 1 ||
            month > 12)
        {
            return Json(new { success = false, message = "El periodo no tiene un formato válido." });
        }

        var marcas = _context.Marcas
            .Where(m => m.IdUsuario == idUsuario &&
                   m.Fecha.Year == year &&
                   m.Fecha.Month == month)
            .ToList();

        if (!marcas.Any())
            return Json(new { success = false, message = "No hay marcas para este periodo." });

        var diasAgrupados = marcas.GroupBy(m => m.Fecha).ToList();

        int tardanzas = 0;
        double horasTotales = 0;
        int diasTrabajados = 0;
        int diasPuntuales = 0;

        foreach (var dia in diasAgrupados)
        {
            var fecha = dia.Key;

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

        var bonos = _context.Bonos
            .Include(b => b.IdTipoBonoNavigation)
            .Where(b => b.Activo == true && b.Monto > 0)
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
        if (request == null)
            return Json(new { success = false, message = "Solicitud inválida." });

        if (request.IdUsuario <= 0)
            return Json(new { success = false, message = "Debe seleccionar un empleado válido." });

        if (request.IdBono <= 0)
            return Json(new { success = false, message = "Debe seleccionar un bono válido." });

        if (string.IsNullOrWhiteSpace(request.Periodo))
            return Json(new { success = false, message = "El periodo es requerido." });

        var bono = _context.Bonos.FirstOrDefault(b => b.IdBono == request.IdBono);

        if (bono == null)
            return Json(new { success = false, message = "El bono seleccionado no existe." });

        if (bono.Activo != true)
            return Json(new { success = false, message = "El bono seleccionado no está activo." });

        if (bono.Monto <= 0)
            return Json(new { success = false, message = "No se puede asignar un bono con monto menor o igual a 0." });

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

        NotificacionHelper.NotificarBonoAsignado(
            _context,
            request.IdUsuario,
            bono.NombreBono ?? "Bono",
            request.Periodo);

        return Json(new { success = true, message = "Bono asignado correctamente." });
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