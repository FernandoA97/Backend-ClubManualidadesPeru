using DocumentFormat.OpenXml.InkML;
using MachineLearning.DTO;
using MachineLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace MachineLearning.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options) { }

        public DbSet<VentaImport> VentasImportadas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }

        public DbSet<HistorialImportacion> HistorialImportaciones { get; set; }

        public DbSet<VentaDTO> VentasDTO { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VentaDTO>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }
}
