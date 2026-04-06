using System;
using System.Collections.Generic;

namespace Oclock.Models;

public class Bono
{
    public int IdBono { get; set; }
    public int IdTipoBono { get; set; }
    public string NombreBono { get; set; }
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public DateOnly FechaCreacion { get; set; }
    public decimal CondicionMinima { get; set; }
    public virtual ICollection<BonoAsignado> BonosAsignados { get; set; } = new List<BonoAsignado>();
    public virtual TipoBono? IdTipoBonoNavigation { get; set; }
}