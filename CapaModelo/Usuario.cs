using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Cedula_Usuario { get; set; }
        public string TipoDocumento { get; set; }
        public string Nom_Rol { get; set; }
        public string Nom_Completo { get; set; }
        public string Nom_User { get; set; }
        public string Direccion { get; set; }
        public int? Telefono1 { get; set; }
        public int? Telefono2 { get; set; }
        public int? Fax { get; set; }
        public string Correo { get; set; }
        public string Clave { get; set; }
        public int IdRol { get; set; }
        public Rol oRol { get; set; }
        public List<Menu> oListaMenu { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Codigo_Recuperacion { get; set; }
        public string NewPassword { get; set; }
        public DateTime Fecha_Expiracion_Codigo { get; set; }







    }
}
