using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class BONO
{
    public int id_bono { get; set; }

    public int id_usuario { get; set; }

    public int? id_politica_bono { get; set; }

    public string nombre_bono { get; set; } = null!;

    public string? descripcion { get; set; }

    public DateOnly? fecha_otorgado { get; set; }

    public DateOnly? fecha_cumplidos { get; set; }

    public string? periodo { get; set; }

    public virtual POLITICA_BONO? id_politica_bonoNavigation { get; set; }

    public virtual USUARIO id_usuarioNavigation { get; set; } = null!;
}
