using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Bono
{
    public int IdBono { get; set; }

    public int IdUsuario { get; set; }

    public int? IdPoliticaBono { get; set; }

    public string NombreBono { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateOnly? FechaOtorgado { get; set; }

    public DateOnly? FechaCumplidos { get; set; }

    public string? Periodo { get; set; }

    public virtual PoliticaBono? IdPoliticaBonoNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
