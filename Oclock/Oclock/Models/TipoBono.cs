using System.Collections.Generic;

namespace Oclock.Models
{
    public class TipoBono
    {
        public int IdTipoBono { get; set; }
        public string NombreTipo { get; set; }
        public string? Descripcion { get; set; }
        public string MetricaTipo { get; set; }

        public virtual ICollection<Bono> Bonos { get; set; } = new List<Bono>();
    }
}