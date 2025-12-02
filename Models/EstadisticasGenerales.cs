using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class EstadisticasGenerales
    {
        public int total_vistas { get; set; }
        public double promedio_ingredientes { get; set; }
        public int total_recetas { get; set; }
        public double vistas_por_receta { get; set; }
    }
}
