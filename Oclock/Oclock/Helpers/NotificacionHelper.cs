using Oclock.Data;
using Oclock.Models;
using System;

namespace Oclock.Helpers
{
    /// <summary>
    /// Helper estático para crear notificaciones desde cualquier controlador.
    /// Uso: NotificacionHelper.Crear(_context, idUsuario, "[SOLICITUD] mensaje");
    /// </summary>
    public static class NotificacionHelper
    {
        // ── Prefijos estándar ────────────────────────────────────────────
        // [SOLICITUD]       → actualización de estado para el empleado
        // [BONO]            → bono asignado para el empleado
        // [NUEVA_SOLICITUD] → nueva solicitud pendiente para el admin

        public static void Crear(
            By5rqco0trg7fpqgnpvmContext context,
            int idUsuario,
            string mensaje)
        {
            var notificacion = new Notificacion
            {
                IdUsuario = idUsuario,
                Mensaje = mensaje,
                Leida = false,
                FechaNotificacion = DateTime.Now
            };

            context.Notificacions.Add(notificacion);
            context.SaveChanges();
        }

        // ── Helpers con prefijo automático ───────────────────────────────

        /// <summary>Notifica al empleado sobre el cambio de estado de su solicitud.</summary>
        public static void NotificarCambioSolicitud(
            By5rqco0trg7fpqgnpvmContext context,
            int idUsuario,
            string nuevoEstado,
            string? observacion = null)
        {
            string texto = $"[SOLICITUD] Tu solicitud fue {nuevoEstado}.";
            if (!string.IsNullOrWhiteSpace(observacion))
                texto += $" Observación: {observacion}";

            Crear(context, idUsuario, texto);
        }

        /// <summary>Notifica al empleado que se le asignó un bono.</summary>
        public static void NotificarBonoAsignado(
            By5rqco0trg7fpqgnpvmContext context,
            int idUsuario,
            string nombreBono,
            string periodo)
        {
            string texto = $"[BONO] Se te asignó el bono \"{nombreBono}\" para el período {periodo}.";
            Crear(context, idUsuario, texto);
        }

        /// <summary>Notifica al admin que un empleado realizó una nueva solicitud.</summary>
        public static void NotificarNuevaSolicitud(
            By5rqco0trg7fpqgnpvmContext context,
            int idAdmin,
            string nombreEmpleado,
            string tipoSolicitud)
        {
            string texto = $"[NUEVA_SOLICITUD] {nombreEmpleado} realizó una nueva solicitud de tipo \"{tipoSolicitud}\".";
            Crear(context, idAdmin, texto);
        }
    }
}