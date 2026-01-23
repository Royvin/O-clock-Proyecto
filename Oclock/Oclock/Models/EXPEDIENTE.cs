using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Expediente
{
    public int IdExpediente { get; set; }

    public int IdUsuario { get; set; }

    public string? Direccion { get; set; }

    public string? Ciudad { get; set; }

    public string? Estado { get; set; }

    public string? Cedula { get; set; }

    public string? EstadoCivil { get; set; }

    public string? ContactoEmergencia { get; set; }

    public string? TelefonoEmergencia { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
