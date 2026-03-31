namespace Oclock.Models
{
    public class SolicitudPut
    {

        public int IdSolicitud { get; set; }
        public int IdTipoSolicitud { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Descripcion { get; set; }

        public IFormFile? Archivo { get; set; }

    
        public bool EliminarArchivo { get; set; }

    }
}
