using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MachineLearning.Models
{
    [Table("Venta_Detalle")]
    public class VentaDetalle
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Venta")]

        public int IdVenta { get; set; }

        [ForeignKey("Producto")]

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal Subtotal { get; set; }
        public virtual VentaImport Venta { get; set; }
        public virtual Producto Producto { get; set; }
    }
}
