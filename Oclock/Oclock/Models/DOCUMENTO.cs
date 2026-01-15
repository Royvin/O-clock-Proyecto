using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class DOCUMENTO
{
    public int id_documento { get; set; }

    public int id_usuario { get; set; }

    public int id_tipo_documento { get; set; }

    public string nombre_archivo { get; set; } = null!;

    public string ruta_archivo { get; set; } = null!;

    public DateTime? fecha_subida { get; set; }

    public virtual TIPO_DOCUMENTO id_tipo_documentoNavigation { get; set; } = null!;

    public virtual USUARIO id_usuarioNavigation { get; set; } = null!;
}
