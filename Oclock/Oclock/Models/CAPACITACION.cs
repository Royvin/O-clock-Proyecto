using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class CAPACITACION
{
    public int id_capacitacion { get; set; }

    public int id_usuario { get; set; }

    public string nombre_curso { get; set; } = null!;

    public string? institucion { get; set; }

    public DateOnly? fecha_inicio { get; set; }

    public DateOnly? fecha_fin { get; set; }

    public bool? certificado { get; set; }

    public int? horas { get; set; }

    public virtual USUARIO id_usuarioNavigation { get; set; } = null!;
}
