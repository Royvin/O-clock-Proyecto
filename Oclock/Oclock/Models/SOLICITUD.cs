using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class SOLICITUD
{
    public int id_solicitud { get; set; }

    public int id_usuario { get; set; }

    public int id_tipo_solicitud { get; set; }

    public string? descripcion { get; set; }

    public DateOnly fecha_solicitud { get; set; }

    public DateOnly? fecha_inicio { get; set; }

    public DateOnly? fecha_fin { get; set; }

    public string? descripcion_estado { get; set; }

    public string? estado { get; set; }

    public virtual TIPO_SOLICITUD id_tipo_solicitudNavigation { get; set; } = null!;

    public virtual USUARIO id_usuarioNavigation { get; set; } = null!;
}
