using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class RecetasTendenciaEst
    {
        public int id_receta { get; set; }
        public string nombre { get; set; }
        public string imagen { get; set; }
        public int vistas_semana { get; set; }
    }
}
