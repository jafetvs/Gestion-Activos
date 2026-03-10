using System;
using System.ComponentModel.DataAnnotations;

namespace CapaModelo
{
    public class ActivoMovimiento
    {
        public int IdMovimiento { get; set; }

        [Required]
        public int IdActivo { get; set; }

        public DateTime FechaMovimiento { get; set; }

        [Required]
        public MovimientoTipo TipoMovimiento { get; set; }

        [StringLength(300)]
        public string Detalle { get; set; }

        /// <summary>Usuario (INGRESO) que registra el movimiento.</summary>
        [Required]
        public int IdUsuario { get; set; }

        /// <summary>Responsable del movimiento (SOLICITANTE.ID_usuario).</summary>
        [Required]
        public int? IdSolicitante { get; set; }

        // Campos enriquecidos que devuelve usp_Movimiento_ListarPorActivo
        public string ResponsableNombre { get; set; }
        public string ResponsableCedula { get; set; }

        // Navegación (opcional)
        public Activo Activo { get; set; }
        public Usuario Usuario { get; set; }
    }
}
