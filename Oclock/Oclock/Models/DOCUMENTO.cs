using System;
using System.Collections.Generic;

namespace Oclock.Models;

public partial class Documento
{
<<<<<<< HEAD
    public int IdDocumento { get; set; }

    public int IdUsuario { get; set; }


    //  NUEVA FK
    public int IdSolicitud { get; set; }

    public string NombreArchivo { get; set; } = null!;

    public string RutaArchivo { get; set; } = null!;

    public DateTime? FechaSubida { get; set; }

    //  Nueva navegación
    public virtual Solicitud IdSolicitudNavigation { get; set; }


    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
=======
        public int IdDocumento { get; set; }
        public int IdUsuario { get; set; }
        public string NombreArchivo { get; set; } = null!;
        public string? Categoria { get; set; }
        public byte[]? ContenidoArchivo { get; set; }
        public string? TipoMime { get; set; }
        public DateTime? FechaSubida { get; set; }
        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
>>>>>>> origin/dev
}

