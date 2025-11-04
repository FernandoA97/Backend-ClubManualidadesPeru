using ClosedXML.Excel;
using MachineLearning.Controllers;
using MachineLearning.Data;
using MachineLearning.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace MachineLearning.Tests.Controllers
{
    public class MachineLearningControllerTests
    {
        private ApplicationDbContext GetFakeContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // 👈 esta línea evita el error
                .Options;

            return new ApplicationDbContext(options);
        }

        private IFormFile CreateExcelFile(bool conFilasValidas = true)
        {
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Ventas");

            // Cabeceras mínimas necesarias
            ws.Cell(1, 1).Value = "Serie";
            ws.Cell(1, 2).Value = "Numero";
            ws.Cell(1, 3).Value = "FechaMision";
            ws.Cell(1, 5).Value = "NombreCliente";
            ws.Cell(1, 6).Value = "RUC";
            ws.Cell(1, 21).Value = "FechaEntrega";
            ws.Cell(1, 23).Value = "RegionTienda";

            if (conFilasValidas)
            {
                ws.Cell(2, 1).Value = "B001";
                ws.Cell(2, 2).Value = "0001";
                ws.Cell(2, 3).Value = DateTime.Today.ToString("yyyy-MM-dd");
                ws.Cell(2, 5).Value = "Cesar Aguilar";
                ws.Cell(2, 6).Value = "12345678901";
                ws.Cell(2, 21).Value = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
                ws.Cell(2, 23).Value = "Lima";
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return new FormFile(stream, 0, stream.Length, "file", "test.xlsx");
        }

        [Fact]
        public void ImportarYPredecir_DeberiaRetornarBadRequest_SiArchivoEsNulo()
        {
            var db = GetFakeContext();
            var controller = new MachineLearningController(db);
            var emptyStream = new MemoryStream();
            IFormFile emptyFile = new FormFile(emptyStream, 0, 0, "file", "vacio.xlsx");

            var result = controller.ImportarYPredecir(emptyFile);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Archivo no proporcionado", badRequest.Value);
        }

        [Fact]
        public void ImportarYPredecir_DeberiaGuardarVentaYCliente_CuandoExcelEsValido()
        {
            var db = GetFakeContext();
            db.Productos.Add(new Producto { CodigoSKU = "P001", NombreProducto = "Producto 1", PrecioUnitario = 10 });
            db.SaveChanges();

            var file = CreateExcelFile();
            var controller = new MachineLearningController(db);

            var result = controller.ImportarYPredecir(file);

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = okResult.Value;

            Assert.Equal("Importación exitosa (sin predicciones)", (string)data.Mensaje);
            Assert.True(db.Clientes.Any());
            Assert.True(db.VentasImportadas.Any());
            Assert.True(db.HistorialImportaciones.Any());
        }

        [Fact]
        public void ImportarYPredecir_DeberiaRegistrarDuplicado_CuandoVentaYaExiste()
        {
            var db = GetFakeContext();

            var cliente = new Cliente
            {
                NombreCliente = "Cesar Aguilar",
                RUC = "12345678901",
                Direccion = "Lima",
                Telefono = "999999999",
                Email = "test@correo.com",
                TipoCliente = "Regular"
            };

            db.Clientes.Add(cliente);

            db.VentasImportadas.Add(new VentaImport
            {
                Serie = "B001",
                Numero = "0001",
                FechaDeMision = DateTime.Today,
                Cliente = cliente,
                FechaEntrega = DateTime.Today.AddDays(1),
                RegionTienda = "Lima",
                MetodoPago = "Efectivo",
                EstadoPago = "Pagado",
                EstadoVenta = "Completada",
                Comentarios = "Prueba duplicada",
                MontoTotal = 100
            });

            db.SaveChanges();

            var file = CreateExcelFile();
            var controller = new MachineLearningController(db);

            var result = controller.ImportarYPredecir(file);

            var ok = Assert.IsType<OkObjectResult>(result);
            dynamic data = ok.Value;

            int rechazados = (int)data.ReporteResumen.RegistrosRechazados;
            Assert.True(rechazados >= 1);
        }


        [Fact]
        public void ImportarYPredecir_DeberiaRetornarError500_SiExcepcionOcurre()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // 👈 línea clave
                .Options;

            var db = new FailingDbContext(options);
            var controller = new MachineLearningController(db);
            var file = CreateExcelFile();

            // Act
            var result = controller.ImportarYPredecir(file);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Contains("Error en servidor", obj.Value.ToString());
        }

        // Fake DbContext que falla al guardar
        private class FailingDbContext : ApplicationDbContext
        {
            public FailingDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public override int SaveChanges()
            {
                throw new Exception("Error simulado en SaveChanges");
            }
        }






    }
}
