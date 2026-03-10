using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo
{
    public class ActivoTipo
    {
        public int IdTipo { get; set; }

        [Required, StringLength(40)]
        public string Nombre { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; }
    }
}
