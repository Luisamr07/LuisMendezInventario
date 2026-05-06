using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacion
{
    // Producto.cs
    public class Producto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int Stock { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioAlquiler { get; set; }
        public decimal PrecioVenta { get; set; } // Agregado para manejar el PrecioVenta si es necesario
    }


}
