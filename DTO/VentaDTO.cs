using Microsoft.EntityFrameworkCore;

namespace MachineLearning.DTO
{
    [Keyless]
    public class VentaDTO
    {
        public int Id { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public DateTime FechaDeMision { get; set; }
        public string NombreCliente { get; set; }  
        public string MetodoPago { get; set; }
        public decimal MontoTotal { get; set; }
        public string EstadoPago { get; set; }
        public string Comentarios { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string EstadoVenta { get; set; }
        public string RegionTienda { get; set; }
    }
}
