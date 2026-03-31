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

    public int? IdUsuario { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}
