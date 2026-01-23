using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class TipoDocumento
{
    public int IdTipoDocumento { get; set; }

    public string NombreTipo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Obligatorio { get; set; }

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
