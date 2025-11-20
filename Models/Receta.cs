using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class Receta
    {
        public int id_receta { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public double tiempo_preparacion { get; set; }
        public double peso_total { get; set; }
        public double porciones { get; set; }
        public double peso_porcion { get; set; }
        public double valor_venta { get; set; }
        public double costo_receta { get; set; }
        public double precio_unidad { get; set; }
        public double porcentaje_ganancia { get; set; }
        public string foto_url { get; set; }
        public string fecha_creacion { get; set; }
        public int id_usuario { get; set; }
    }
}
