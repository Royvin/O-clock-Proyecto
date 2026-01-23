using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Marca
{
    public int IdMarca { get; set; }

    public string Nombre { get; set; } = null!;

    public TimeOnly? HoraEntrada { get; set; }

    public TimeOnly? HoraSalida { get; set; }

    public string? Ubicancia { get; set; }

    public string? Comentario { get; set; }

    public virtual Usuario IdMarcaNavigation { get; set; } = null!;
}
