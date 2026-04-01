using System.Collections.Generic;

namespace Oclock.Models;

public class BonoViewModel
{
    public Bono NuevoBono { get; set; } = new Bono();
    public List<Bono> Bonos { get; set; } = new();
    public List<TipoBono> TiposBono { get; set; } = new();
}