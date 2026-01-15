using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class MARCA
{
    public int id_marca { get; set; }

    public string nombre { get; set; } = null!;

    public TimeOnly? hora_entrada { get; set; }

    public TimeOnly? hora_salida { get; set; }

    public string? ubicancia { get; set; }

    public string? comentario { get; set; }

    public virtual USUARIO id_marcaNavigation { get; set; } = null!;
}
