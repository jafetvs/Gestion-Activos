using System;
using System.ComponentModel.DataAnnotations;

namespace CapaModelo
{
    public class ActivoSoftware
    {
        public int IdSoftware { get; set; }

        [Required]
        public int IdActivo { get; set; }

        [Required, StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(150)]
        public string Editor { get; set; }

        public DateTime? FechaInstalacion { get; set; }

        [StringLength(50)]
        public string Tamano { get; set; }   // Ej: "1024 KB", "250 MB", "1.5 GB"

        [StringLength(50)]
        public string Version { get; set; }

        // Auditoría
        [Required]
        public int IdUsuarioCrea { get; set; }  // <- quién crea
        public DateTime FechaRegistro { get; set; }

        // Navegación (opcional)
        public Activo Activo { get; set; }
    }
}
