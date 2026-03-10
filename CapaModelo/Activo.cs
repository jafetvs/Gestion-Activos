using System;
using System.ComponentModel.DataAnnotations;

namespace CapaModelo
{
    public class Activo
    {
        public int IdActivo { get; set; }

        [Required, StringLength(30)]
        public string Codigo { get; set; } // Identificador único de negocio

        [Required, StringLength(100)]
        public string NombreTipo { get; set; } // Nombre/Tipo (texto libre del activo)

        [StringLength(300)]
        public string Descripcion { get; set; }

        // FK: Categoría
        [Required]
        public int IdCategoria { get; set; }

        // Clasificación
        [Required]
        public ActivoClasificacion Clasificacion { get; set; } = ActivoClasificacion.SinDefinir;

        [StringLength(80)]
        public string Serie { get; set; }

        [StringLength(120)]
        public string Modelo { get; set; }

        [StringLength(120)]
        public string PlacaCodBarras { get; set; }

        [StringLength(120)]
        public string Procesador { get; set; }

        [StringLength(120)]
        public string RAM { get; set; }

        [StringLength(120)]
        public string SistemaOperativo { get; set; }

        [StringLength(120)]
        public string DiscoDuro { get; set; }

        [StringLength(50)]
        public string IP { get; set; }

        // FK: Departamento (opcional)
        public int? Id_Departamento { get; set; }

        // Responsable (SOLICITANTE.ID_usuario) - opcional
        public int? IdSolicitante { get; set; }

        [StringLength(120)]
        public string UsuarioLocal { get; set; }

        [StringLength(300)]
        public string OtraCaracteristica { get; set; }

        public DateTime? FechaEntrega { get; set; }

        // Datos de compra (opcionales)
        [Range(0, 999999999)]
        public decimal? Costo { get; set; }

        [StringLength(60)]
        public string NoFactura { get; set; }

        public DateTime? FechaCompra { get; set; }

        public int? IdProveedor { get; set; }

        // === NUEVOS CAMPOS ===
        [Required]
        public int IdTipo { get; set; }        // FK a ACTIVO_TIPO

        [Required]
        public int IdClase { get; set; }       // FK a ACTIVO_CLASE

        [StringLength(150)]
        public string UbicacionFisica { get; set; } // VARCHAR(150)

        [StringLength(120)]
        public string UsoPrincipal { get; set; }    // VARCHAR(120)

        // Auditoría
        public bool ActivoFlag { get; set; } = true; // Mapea a columna 'Activo' en la BD

        [Required]
        public int IdUsuarioCrea { get; set; }

        public DateTime FechaRegistro { get; set; }

        // Navegación (opcional)
        public ActivoCategoria Categoria { get; set; }
        public Proveedor Proveedor { get; set; }
        public Departamentos Departamento { get; set; }
        public Usuario UsuarioCreador { get; set; }
        public Solicitantes Responsable { get; set; }

        // Navegación nuevos catálogos
        public ActivoTipo Tipo { get; set; }
        public ActivoClase Clase { get; set; }
    }

    // DTOs ligeros por pestaña (pueden ir en /Models o arriba del controller)
    public class ActivoResumenDto
    {
        public int IdActivo { get; set; }
        public string Codigo { get; set; }
        public string NombreTipo { get; set; }
        public string Descripcion { get; set; }
        public int IdCategoria { get; set; }
        public int? Id_Departamento { get; set; }
        public int? IdSolicitante { get; set; }
        public string OtraCaracteristica { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public ActivoClasificacion Clasificacion { get; set; }
    }

    public class ActivoCompraDto
    {
        public int IdActivo { get; set; }
        public int? IdProveedor { get; set; }
        public string NoFactura { get; set; }
        public decimal? Costo { get; set; }
        public DateTime? FechaCompra { get; set; }
    }

    public class ActivoHardwareDto
    {
        public int IdActivo { get; set; }
        public string Serie { get; set; }
        public string Modelo { get; set; }
        public string PlacaCodBarras { get; set; }
        public string IP { get; set; }
        public string Procesador { get; set; }
        public string RAM { get; set; }
        public string SistemaOperativo { get; set; }
        public string DiscoDuro { get; set; }
    }

}
