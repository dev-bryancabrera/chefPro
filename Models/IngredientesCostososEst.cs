using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class IngredientesCostososEst
    {
        public int id_ingrediente { get; set; }
        public string nombre { get; set; }
        public double costo_unidad { get; set; }
        public string unidad_medida { get; set; }
        public int veces_usado { get; set; }
        public double costo_total_acumulado { get; set; }
    }
}
