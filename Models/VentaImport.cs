using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MachineLearning.Models
{
    [Table("Ventas")]
    public class VentaImport
    {
        [Key]
        public int Id { get; set; }

        public string Serie { get; set; }
        public string Numero { get; set; }
        public DateTime FechaDeMision { get; set; }

        [ForeignKey("Cliente")] 
        public int IdCliente { get; set; }

        public string MetodoPago { get; set; }
        public decimal MontoTotal { get; set; }
        public string EstadoPago { get; set; }
        public string Comentarios { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string EstadoVenta { get; set; }
        public string RegionTienda { get; set; }
        public virtual Cliente Cliente { get; set; }
    }
}
