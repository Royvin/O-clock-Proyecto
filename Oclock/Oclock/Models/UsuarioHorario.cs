using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class UsuarioHorario
{
    public int IdUsuarioHorario { get; set; }

    public int IdUsuario { get; set; }

    public int IdHorario { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public virtual Horario IdHorarioNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
