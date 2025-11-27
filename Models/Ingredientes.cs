using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class Ingrediente
    {
        [PrimaryKey, AutoIncrement]
        public int id_ingrediente { get; set; }
        public int id_receta { get; set; }
        public string nombre { get; set; }
        public double cantidad { get; set; }
        public double peso { get; set; }
        public string unidad { get; set; }
        public string unidad_medida { get; set; }
        public double precio { get; set; }
        public double costo_unidad { get; set; }
        public double id_usuario { get; set; }

        public double precio_unitario_total => costo_unidad * cantidad;
    }
}

