using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class NOTIFICACION
{
    public int id_notificacion { get; set; }

    public int id_usuario { get; set; }

    public DateTime? fecha_notificacion { get; set; }

    public string mensaje { get; set; } = null!;

    public bool? leida { get; set; }

    public virtual USUARIO id_usuarioNavigation { get; set; } = null!;
}
