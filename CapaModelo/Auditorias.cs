using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo
{
    public class Auditorias
    {
        public int Cod_Auditoria { get; set; }
        public string Cedula_Usuario { get; set; }
        public string Nom_Rol { get; set; }
        public string Tabla { get; set; }
        public string Procedimiento { get; set; }
        public int? ID_ColumAfectada { get; set; }
        public string Modificacion_JSON { get; set; }
        public DateTime Fecha_Modificacion { get; set; } 
    }
}
