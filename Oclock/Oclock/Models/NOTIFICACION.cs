using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Notificacion
{
    public int IdNotificacion { get; set; }

    public int IdUsuario { get; set; }

    public DateTime? FechaNotificacion { get; set; }

    public string Mensaje { get; set; } = null!;

    public bool? Leida { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
