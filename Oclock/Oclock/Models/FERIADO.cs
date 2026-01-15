using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class FERIADO
{
    public int id_feriado { get; set; }

    public string nombre { get; set; } = null!;

    public DateOnly fecha { get; set; }

    public bool? es_laborable { get; set; }

    public string? descripcion { get; set; }
}
