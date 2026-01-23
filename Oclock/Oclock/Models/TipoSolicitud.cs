using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class TipoSolicitud
{
    public int IdTipoSolicitud { get; set; }

    public string NombreSolicitud { get; set; } = null!;

    public virtual ICollection<Solicitud> Solicituds { get; set; } = new List<Solicitud>();
}
