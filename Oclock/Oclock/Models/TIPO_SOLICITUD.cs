using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class TIPO_SOLICITUD
{
    public int id_tipo_solicitud { get; set; }

    public string nombre_solicitud { get; set; } = null!;

    public virtual ICollection<SOLICITUD> SOLICITUDs { get; set; } = new List<SOLICITUD>();
}
