using System;
using System.ComponentModel.DataAnnotations;

namespace CapaModelo
{
    public class ActivoAdjunto
    {
        public int IdAdjunto { get; set; }

        [Required]
        public int IdActivo { get; set; }

        [Required, StringLength(200)]
        public string NombreArchivo { get; set; }

        /// <summary>Binario del archivo (solo se llena al descargar/subir).</summary>
        public byte[] Contenido { get; set; }

        /// <summary>MIME type opcional (ej: application/pdf).</summary>
        [StringLength(100)]
        public string ContentType { get; set; }

        /// <summary>Tamaño en bytes (opcional, útil para mostrar o validar).</summary>
        public long? TamanoBytes { get; set; }

        /// <summary>Usuario (INGRESO) que subió el adjunto.</summary>
        [Required]
        public int IdUsuarioCrea { get; set; }

        /// <summary>Fecha y hora de registro.</summary>
        public DateTime FechaRegistro { get; set; }

        // Navegación (opcional)
        public Activo Activo { get; set; }
        public Usuario Usuario { get; set; }
    }
}
