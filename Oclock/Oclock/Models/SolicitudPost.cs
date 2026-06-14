using Microsoft.AspNetCore.Http;
using System;

namespace Oclock.Models
{
    public class SolicitudPost
    {
        public int IdTipoSolicitud { get; set; }

        public string? Descripcion { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public string? Prioridad { get; set; }

        public IFormFile? Archivo { get; set; }

        public string? ParentescoFamiliar { get; set; }

        public string? NombreFamiliar { get; set; }

        public string? MotivoConstancia { get; set; }

        public string? DetalleOtro { get; set; }
    }
}