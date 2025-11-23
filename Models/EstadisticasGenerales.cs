using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class EstadisticasGenerales
    {
        public bool success { get; set; }
        public int total_vistas { get; set; }
        public List<RecetasEst> recetas_top { get; set; }
        public List<IngredientesEst> ingredientes_top { get; set; }
        public List<TecnicasEst> tecnicas_top { get; set; }
        public decimal promedio_ingredientes { get; set; }
    }
}
