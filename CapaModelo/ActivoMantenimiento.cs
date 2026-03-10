using System;
using System.ComponentModel.DataAnnotations;

namespace CapaModelo
{
    public class ActivoMantenimiento
    {
        public int IdMantenimiento { get; set; }

        [Required]
        public int IdActivo { get; set; }

        /// <summary>Responsable del mantenimiento (SOLICITANTE.ID_usuario).</summary>
        [Required]
        public int IdSolicitante { get; set; }

        /// <summary>Fecha del mantenimiento (solo fecha, no hora).</summary>
        [Required]
        public DateTime FechaMantenimiento { get; set; }

        /// <summary>Tipo de mantenimiento: Preventivo, Correctivo, etc.</summary>
        [Required, StringLength(50)]
        public string Tipo { get; set; }

        /// <summary>Detalle o descripción del mantenimiento realizado.</summary>
        [StringLength(300)]
        public string Detalle { get; set; }

        /// <summary>Usuario (INGRESO) que registró el mantenimiento.</summary>
        [Required]
        public int IdUsuarioCrea { get; set; }

        /// <summary>Fecha y hora de registro en el sistema.</summary>
        public DateTime FechaRegistro { get; set; }

        // Campos enriquecidos que devuelve usp_Mantenimiento_ListarPorActivo
        public string ResponsableNombre { get; set; }
        public string ResponsableCedula { get; set; }

        // Navegación (opcional)
        public Activo Activo { get; set; }
        public Usuario Usuario { get; set; }
    }
}

