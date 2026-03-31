using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Feriado
{
    public int IdFeriado { get; set; }

    public string Nombre { get; set; } = null!;

    public DateOnly Fecha { get; set; }

    public bool? EsLaborable { get; set; }

    public string? Descripcion { get; set; }
}
