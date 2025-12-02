using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class FinancieroEst
    {
        public int total_recetas_costeadas { get; set; }
        public double costo_promedio { get; set; }
        public double venta_promedio { get; set; }
        public double ganancia_promedio { get; set; }
        public double costo_total { get; set; }
        public double ventas_potenciales { get; set; }
    }
}
