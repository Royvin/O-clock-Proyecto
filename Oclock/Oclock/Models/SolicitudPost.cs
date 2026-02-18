using System;
using System.Collections.Generic;


namespace Oclock.Models
{
    public class SolicitudPost
    {

        
        public int IdTipoSolicitud { get; set; }

        
        public string Descripcion { get; set; }

      
        public DateTime FechaInicio { get; set; }

       
        public DateTime FechaFin { get; set; }

        
        public string Prioridad { get; set; }


    }
}
