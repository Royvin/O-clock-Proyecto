using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class TIPO_DOCUMENTO
{
    public int id_tipo_documento { get; set; }

    public string nombre_tipo { get; set; } = null!;

    public string? descripcion { get; set; }

    public bool? obligatorio { get; set; }

    public virtual ICollection<DOCUMENTO> DOCUMENTOs { get; set; } = new List<DOCUMENTO>();
}
