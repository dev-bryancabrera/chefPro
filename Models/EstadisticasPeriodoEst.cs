using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class EstadisticasPeriodoEst
    {
        public bool success { get; set; }
        public int periodo { get; set; }
        public List<VistaRecetaDiarioEst> vistas_por_dia { get; set; }
    }
}
