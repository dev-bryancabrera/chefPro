using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class RecetasRentablesEst
    {
        public int id_receta { get; set; }
        public string nombre { get; set; }
        public string imagen { get; set; }
        public double costo_receta { get; set; }
        public double valor_venta { get; set; }
        public double porcentaje_ganancia { get; set; }
        public double ganancia_neta { get; set; }
    }
}
