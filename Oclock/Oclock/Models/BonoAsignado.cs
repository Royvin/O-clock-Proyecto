namespace Oclock.Models;

public class BonoAsignado
{
    public int IdBonoAsignado { get; set; }
    public int IdBono { get; set; }
    public int IdUsuario { get; set; }
    public string Periodo { get; set; }
    public DateOnly FechaAsignado { get; set; }

    public virtual Bono? IdBonoNavigation { get; set; }
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}