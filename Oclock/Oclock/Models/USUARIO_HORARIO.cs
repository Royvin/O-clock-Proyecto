using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class USUARIO_HORARIO
{
    public int id_usuario_horario { get; set; }

    public int id_usuario { get; set; }

    public int id_horario { get; set; }

    public DateOnly? fecha_inicio { get; set; }

    public DateOnly? fecha_fin { get; set; }

    public virtual HORARIO id_horarioNavigation { get; set; } = null!;

    public virtual USUARIO id_usuarioNavigation { get; set; } = null!;
}
