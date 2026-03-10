using System;
using System.ComponentModel.DataAnnotations;

namespace CapaModelo
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }

        [Required, StringLength(150)]
        public string NombreEmpresa { get; set; }

        [StringLength(150)]
        public string Servicio { get; set; }

        [StringLength(50)]
        public string NoContrato { get; set; }

        [StringLength(50)]
        public string NoProcedimiento { get; set; }

        public bool RenovacionContrato { get; set; } = false;

        public DateTime? FechaVencimientoContrato { get; set; }

        [StringLength(100)]
        public string Contacto1_Nombre { get; set; }

        [StringLength(30)]
        public string Contacto1_Telefono { get; set; }

        [EmailAddress, StringLength(120)]
        public string Contacto1_Correo { get; set; }

        [StringLength(100)]
        public string Contacto2_Nombre { get; set; }

        [StringLength(30)]
        public string Contacto2_Telefono { get; set; }

        [EmailAddress, StringLength(120)]
        public string Contacto2_Correo { get; set; }

        [StringLength(200)]
        public string PaginaWeb { get; set; }

        public bool Activo { get; set; } = true;

        // === Auditoría ===
        [Required]
        public int IdUsuarioCrea { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
