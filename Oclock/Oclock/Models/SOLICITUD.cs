using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Solicitud
{
    public int IdSolicitud { get; set; }

    public int IdUsuario { get; set; }

    public int IdTipoSolicitud { get; set; }

    public string? Descripcion { get; set; }

    public DateOnly FechaSolicitud { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? DescripcionEstado { get; set; }

    public string? Estado { get; set; }

    public virtual TipoSolicitud IdTipoSolicitudNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
