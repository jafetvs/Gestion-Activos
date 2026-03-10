using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo
{
    // Clasificación de los activos (columna Clasificacion en la BD)
    public enum ActivoClasificacion
    {
        EnUso,
        EnReparacion,
        Desechado,
        SinDefinir
    }

    // Tipos de movimientos (tabla ACTIVO_MOVIMIENTO)
    public enum MovimientoTipo
    {
        ALTA,
        ASIGNACION,
        REUBICACION,
        REPARACION,
        BAJA
    }

    public enum MantenimientoTipo
    {
        Preventivo = 0,     // Rutinas programadas (limpieza, ajuste, inspección...)
        Correctivo = 1,    // Corrige una falla ya presente
        Instalacion = 3,    // Puesta en marcha / instalación de componente
        Actualizacion = 4,  // Update/upgrade de firmware/driver/OS/versión
        Calibracion = 5,    // Ajuste de parámetros de medida
        Limpieza = 6,    // Limpieza física/lógica específica
        Otro = 99    // Cualquier caso no contemplado arriba
    }
}
