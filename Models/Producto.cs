using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MachineLearning.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        public string CodigoSKU { get; set; }
        public string NombreProducto { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
