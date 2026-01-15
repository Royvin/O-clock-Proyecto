using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class HORARIO
{
    public int id_horario { get; set; }

    public string nombre_horario { get; set; } = null!;

    public TimeOnly hora_entrada { get; set; }

    public TimeOnly hora_salida { get; set; }

    public string? dias { get; set; }

    public virtual ICollection<USUARIO_HORARIO> USUARIO_HORARIOs { get; set; } = new List<USUARIO_HORARIO>();
}
