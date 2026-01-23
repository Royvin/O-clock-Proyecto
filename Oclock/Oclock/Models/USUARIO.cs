using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public string? Telefono { get; set; }

    public DateOnly? FechaContratacion { get; set; }

    public string? Estado { get; set; }

    public bool? Activo { get; set; }

    public int IdRol { get; set; }

    public virtual ICollection<Bono> Bonos { get; set; } = new List<Bono>();

    public virtual ICollection<Capacitacion> Capacitacions { get; set; } = new List<Capacitacion>();

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    public virtual ICollection<Expediente> Expedientes { get; set; } = new List<Expediente>();

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual Marca? Marca { get; set; }

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual ICollection<Solicitud> Solicituds { get; set; } = new List<Solicitud>();

    public virtual ICollection<UsuarioHorario> UsuarioHorarios { get; set; } = new List<UsuarioHorario>();
}
