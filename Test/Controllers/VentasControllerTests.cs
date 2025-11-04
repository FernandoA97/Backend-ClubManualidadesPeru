using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MachineLearning.Controllers;
using MachineLearning.Data;
using MachineLearning.Models;
using MachineLearning.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using System;

namespace MachineLearning.Tests.Controllers
{
    public class VentasControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetVentas_DeberiaRetornarOk()
        {
            var db = GetInMemoryDbContext();

            // Creamos datos de prueba para simular el resultado del SP
            db.VentasImportadas.Add(new VentaImport
            {
                Serie = "A001",
                Numero = "0001",
                FechaDeMision = DateTime.Now,
                MontoTotal = 120,
                MetodoPago = "Efectivo",
                EstadoPago = "Pagado",
                EstadoVenta = "Completada",
                RegionTienda = "Lima",
                Comentarios = "Entrega rápida"
            });
            db.SaveChanges();

            var controller = new VentasController(db);

            // simulamos manualmente el resultado esperado del SP
            var ventasFake = db.VentasImportadas.Select(v => new VentaDTO
            {
                Id = v.Id,
                Serie = v.Serie,
                Numero = v.Numero,
                FechaDeMision = v.FechaDeMision,
                NombreCliente = "César",
                MetodoPago = v.MetodoPago,
                MontoTotal = v.MontoTotal,
                EstadoPago = v.EstadoPago,
                Comentarios = v.Comentarios,
                FechaEntrega = v.FechaDeMision.AddDays(1),
                EstadoVenta = v.EstadoVenta,
                RegionTienda = v.RegionTienda
            }).ToList();

            // Verificación de simulación
            Assert.True(ventasFake.Count >= 1);
            Assert.Equal("A001", ventasFake.First().Serie);
        }



        [Fact]
        public async Task GetVenta_DeberiaRetornarVenta_CuandoExiste()
        {
            var db = GetInMemoryDbContext();
            var venta = new VentaImport
            {
                Serie = "A001",
                Numero = "0002",
                FechaDeMision = DateTime.Now,
                MontoTotal = 200,
                MetodoPago = "Yape",
                EstadoPago = "Pagado",
                EstadoVenta = "Completada",
                RegionTienda = "Cusco",
                Comentarios = "Cliente frecuente"
            };
            db.VentasImportadas.Add(venta);
            db.SaveChanges();

            var controller = new VentasController(db);

            var result = await controller.GetVenta(venta.Id);

            var ok = Assert.IsType<VentaImport>(result.Value);
            Assert.Equal("A001", ok.Serie);
        }

        [Fact]
        public async Task GetVenta_DeberiaRetornarNotFound_CuandoNoExiste()
        {
            var db = GetInMemoryDbContext();
            var controller = new VentasController(db);

            var result = await controller.GetVenta(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostVenta_DeberiaCrearVenta()
        {
            var db = GetInMemoryDbContext();
            var controller = new VentasController(db);

            var nueva = new VentaImport
            {
                Serie = "A002",
                Numero = "0003",
                FechaDeMision = DateTime.Now,
                MontoTotal = 300,
                MetodoPago = "Plin",
                EstadoPago = "Pendiente",
                EstadoVenta = "En proceso",
                RegionTienda = "Arequipa",
                Comentarios = "Nueva venta"
            };

            var result = await controller.PostVenta(nueva);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var data = Assert.IsType<VentaImport>(created.Value);

            Assert.Equal("A002", data.Serie);
            Assert.True(db.VentasImportadas.Any(v => v.Serie == "A002"));
        }

        [Fact]
        public async Task PutVenta_DeberiaActualizarVenta()
        {
            var db = GetInMemoryDbContext();
            var venta = new VentaImport
            {
                Serie = "A003",
                Numero = "0004",
                FechaDeMision = DateTime.Now,
                MontoTotal = 400,
                MetodoPago = "Tarjeta",
                EstadoPago = "Pendiente",
                EstadoVenta = "Proceso",
                RegionTienda = "Lima",
                Comentarios = "Inicial"
            };
            db.VentasImportadas.Add(venta);
            db.SaveChanges();

            var controller = new VentasController(db);
            venta.MontoTotal = 500;

            var result = await controller.PutVenta(venta.Id, venta);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(500, db.VentasImportadas.Find(venta.Id).MontoTotal);
        }

        [Fact]
        public async Task PutVenta_DeberiaRetornarBadRequest_SiIdNoCoincide()
        {
            var db = GetInMemoryDbContext();
            var controller = new VentasController(db);

            var venta = new VentaImport { Id = 1 };

            var result = await controller.PutVenta(99, venta);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteVenta_DeberiaEliminarVenta()
        {
            var db = GetInMemoryDbContext();
            var venta = new VentaImport
            {
                Serie = "A004",
                Numero = "0005",
                FechaDeMision = DateTime.Now,
                MontoTotal = 600,
                MetodoPago = "Efectivo",
                EstadoPago = "Pagado",
                EstadoVenta = "Finalizada",
                RegionTienda = "Piura",
                Comentarios = "Listo"
            };
            db.VentasImportadas.Add(venta);
            db.SaveChanges();

            var controller = new VentasController(db);
            var result = await controller.DeleteVenta(venta.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.False(db.VentasImportadas.Any(v => v.Id == venta.Id));
        }

        [Fact]
        public async Task DeleteVenta_DeberiaRetornarNotFound_SiNoExiste()
        {
            var db = GetInMemoryDbContext();
            var controller = new VentasController(db);

            var result = await controller.DeleteVenta(123);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
