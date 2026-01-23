using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Horario
{
    public int IdHorario { get; set; }

    public string NombreHorario { get; set; } = null!;

    public TimeOnly HoraEntrada { get; set; }

    public TimeOnly HoraSalida { get; set; }

    public string? Dias { get; set; }

    public virtual ICollection<UsuarioHorario> UsuarioHorarios { get; set; } = new List<UsuarioHorario>();
}
