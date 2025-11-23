using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class IngredientesEst
    {
        public int id_ingrediente { get; set; }
        public string nombre { get; set; }
        public int total_usos { get; set; }
        public decimal? cantidad_total { get; set; }
    }
}
