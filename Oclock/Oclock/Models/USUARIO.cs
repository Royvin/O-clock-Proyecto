using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class USUARIO
{
    public int id_usuario { get; set; }

    public string nombre { get; set; } = null!;

    public string apellido { get; set; } = null!;

    public string email { get; set; } = null!;

    public string contraseña { get; set; } = null!;

    public string? telefono { get; set; }

    public DateOnly? fecha_contratacion { get; set; }

    public string? estado { get; set; }

    public bool? activo { get; set; }

    public virtual ICollection<BONO> BONOs { get; set; } = new List<BONO>();

    public virtual ICollection<CAPACITACION> CAPACITACIONs { get; set; } = new List<CAPACITACION>();

    public virtual ICollection<DOCUMENTO> DOCUMENTOs { get; set; } = new List<DOCUMENTO>();

    public virtual ICollection<EXPEDIENTE> EXPEDIENTEs { get; set; } = new List<EXPEDIENTE>();

    public virtual MARCA? MARCA { get; set; }

    public virtual ICollection<NOTIFICACION> NOTIFICACIONs { get; set; } = new List<NOTIFICACION>();

    public virtual ICollection<SOLICITUD> SOLICITUDs { get; set; } = new List<SOLICITUD>();

    public virtual ICollection<USUARIO_HORARIO> USUARIO_HORARIOs { get; set; } = new List<USUARIO_HORARIO>();
}
