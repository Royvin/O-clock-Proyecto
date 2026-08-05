using System;
using Oclock.Data;
using Oclock.Models;

namespace Oclock.Helpers
{
    public static class VacacionesHelper
    {
        private const decimal DiasPorMes = 1.0m;

        public static bool EsTipoVacaciones(string? nombreTipoSolicitud)
        {
            var tipo = (nombreTipoSolicitud ?? "").Trim().ToLower();
            return tipo.Contains("vacacion");
        }

        
        public static decimal AcumularYObtenerSaldo(By5rqco0trg7fpqgnpvmContext context, Usuario usuario, DateTime? ahora = null)
        {
            var hoy = DateOnly.FromDateTime(ahora ?? DateTime.Now);
            var fechaBase = usuario.FechaUltimoAcumuloVacaciones ?? usuario.FechaContratacion;

            if (fechaBase == null)
            {
                return usuario.DiasVacaciones;
            }

            int mesesTranscurridos = ((hoy.Year - fechaBase.Value.Year) * 12) + (hoy.Month - fechaBase.Value.Month);

            if (hoy.Day < fechaBase.Value.Day)
            {
                mesesTranscurridos--;
            }

            if (mesesTranscurridos > 0)
            {
                usuario.DiasVacaciones += mesesTranscurridos * DiasPorMes;
                usuario.FechaUltimoAcumuloVacaciones = fechaBase.Value.AddMonths(mesesTranscurridos);
                context.SaveChanges();
            }

            return usuario.DiasVacaciones;
        }
    }
}