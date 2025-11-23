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

        public int id_receta { get; set; } // Relación con Receta
        public string nombre { get; set; }
        public double cantidad { get; set; }
        public string unidad { get; set; } // ej: g, ml, pieza
        public double precio { get; set; }
        [Ignore]
        public List<string> Unidades { get; set; } = new List<string> { "kg", "g", "L", "ml", "unidad" };
    }
}

