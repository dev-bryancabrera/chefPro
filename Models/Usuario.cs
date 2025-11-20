using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chefPro.Models
{
    public class Usuario
    {
        public int id_usuario { get; set; }
        public string nombres { get; set; }
        public string email { get; set; }
        public string password_hash { get; set; }
        public string tipo_login { get; set; }
        public string fecha_registro { get; set; }
    }
}
