using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class ROL
{
    public int id_rol { get; set; }

    public string nombre_rol { get; set; } = null!;

    public string? descripcion { get; set; }
}
