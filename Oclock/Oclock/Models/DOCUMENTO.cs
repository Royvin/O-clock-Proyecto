using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Documento
{
    public int IdDocumento { get; set; }

    public int IdUsuario { get; set; }

    public int IdTipoDocumento { get; set; }

    public string NombreArchivo { get; set; } = null!;

    public string RutaArchivo { get; set; } = null!;

    public DateTime? FechaSubida { get; set; }

    public virtual TipoDocumento IdTipoDocumentoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
