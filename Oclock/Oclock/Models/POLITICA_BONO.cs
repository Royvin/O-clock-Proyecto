using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class POLITICA_BONO
{
    public int id_politica_bono { get; set; }

    public string nombre_politica { get; set; } = null!;

    public int? dias_acumulados { get; set; }

    public int? meses_bono { get; set; }

    public string? descripcion { get; set; }

    public bool? activo { get; set; }

    public virtual ICollection<BONO> BONOs { get; set; } = new List<BONO>();
}
