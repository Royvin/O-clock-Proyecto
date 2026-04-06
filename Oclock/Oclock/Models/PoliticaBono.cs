using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class PoliticaBono
{
    public int IdPoliticaBono { get; set; }

    public string NombrePolitica { get; set; } = null!;

    public int IdTipoBono { get; set; }

    public int? DiasAcumulados { get; set; }

    public int? MesesBono { get; set; }

    public int? MaxTardanzas { get; set; }

    public int? HorasMinimas { get; set; }

    public decimal? PorcentajePuntualidad { get; set; }

    public decimal Monto { get; set; }

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public virtual TipoBono IdTipoBonoNavigation { get; set; } = null!;

    public virtual ICollection<Bono> Bonos { get; set; } = new List<Bono>();
}