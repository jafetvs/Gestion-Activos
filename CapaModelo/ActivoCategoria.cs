using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CapaModelo
{
    public class ActivoCategoria
    {
        public int IdCategoria { get; set; }

        [Required, StringLength(80)]
        public string Nombre { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; }
    }
}
