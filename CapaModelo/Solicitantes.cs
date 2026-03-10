using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo
{
    public class Solicitantes
    {
        public int ID_Proceso { get; set; }
        public int ID_Usuario { get; set; } 
        public string TipoDocumento { get; set; } 
        public string Cedula_Solicitante { get; set; } 
        public string Nombre { get; set; } 
        public string Direccion { get; set; } 
        public int? Telefono { get; set; }
        public int? Telefono2 { get; set; } 
        public int? Fax { get; set; } 
        public string Correo { get; set; } 
        public bool Activo { get; set; } 
        public DateTime FechaRegistro { get; set; } 
    }
}
