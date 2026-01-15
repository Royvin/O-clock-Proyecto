using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class EXPEDIENTE
{
    public int id_expediente { get; set; }

    public int id_usuario { get; set; }

    public string? direccion { get; set; }

    public string? ciudad { get; set; }

    public string? estado { get; set; }

    public string? cedula { get; set; }

    public string? estado_civil { get; set; }

    public string? contacto_emergencia { get; set; }

    public string? telefono_emergencia { get; set; }

    public virtual USUARIO id_usuarioNavigation { get; set; } = null!;
}
