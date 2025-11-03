using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;
using MachineLearning.Models;
using MachineLearning.Data;
using System;
using Microsoft.EntityFrameworkCore;

namespace MachineLearning.Controllers
{
    [ApiController]
    [Route("api/ml")]
    public class MachineLearningController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MachineLearningController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("importar-y-predecir")]
        public IActionResult ImportarYPredecir(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Archivo no proporcionado");

            var listaVentas = new List<VentaImport>();
            var listaVentaDetalles = new List<VentaDetalle>();
            var listaClientesNuevos = new List<Cliente>();

            var registrosDuplicados = new List<string>();
            int registrosValidos = 0;
            int registrosRechazados = 0;

            using var stream = new MemoryStream();
            file.CopyTo(stream);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

            foreach (var row in rows)
            {

                DateTime? fechaMision = null;
                var fechaMisionCelda = row.Cell(3).GetValue<string>();

                if (!string.IsNullOrEmpty(fechaMisionCelda) &&
                    DateTime.TryParse(fechaMisionCelda, out DateTime tempFechaM))
                    fechaMision = tempFechaM;

                DateTime? fechaEntrega = null;
                var fechaEntregaCelda = row.Cell(21).GetValue<string>();

                if (!string.IsNullOrEmpty(fechaEntregaCelda) &&
                    DateTime.TryParse(fechaEntregaCelda, out DateTime tempFechaE))
                    fechaEntrega = tempFechaE;

                string nombreCliente = row.Cell(5).GetString();
                string ruc = row.Cell(6).GetString();

                var cliente = _context.Clientes
                    .FirstOrDefault(c => c.NombreCliente == nombreCliente && c.RUC == ruc);

                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        NombreCliente = nombreCliente,
                        RUC = ruc,
                        Telefono = row.Cell(7).GetString(),
                        Email = row.Cell(8).GetString(),
                        Direccion = row.Cell(9).GetString(),
                        TipoCliente = row.Cell(10).GetString()
                    };

                    listaClientesNuevos.Add(cliente);
                }

                string serie = row.Cell(1).GetString();
                string numero = row.Cell(2).GetString();

                if (string.IsNullOrEmpty(serie) || string.IsNullOrEmpty(numero))
                {
                    registrosRechazados++;
                    continue;
                }

                var ventaExistente = _context.VentasImportadas
                    .FirstOrDefault(v => v.Serie == serie && v.Numero == numero);

                if (ventaExistente != null)
                {
                    registrosDuplicados.Add($"Venta con Serie={serie} y Número={numero} ya existe.");
                    registrosRechazados++;
                    continue;
                }

                if (!fechaMision.HasValue || !fechaEntrega.HasValue)
                {
                    registrosRechazados++;
                    continue;
                }

                var venta = new VentaImport
                {
                    Serie = serie,
                    Numero = numero,
                    FechaDeMision = fechaMision.Value,
                    Cliente = cliente,
                    MetodoPago = row.Cell(17).GetString(),
                    MontoTotal = row.Cell(18).TryGetValue<decimal>(out decimal monto) ? monto : 0m,
                    EstadoPago = row.Cell(19).GetString(),
                    Comentarios = row.Cell(20).GetString(),
                    FechaEntrega = fechaEntrega,
                    EstadoVenta = row.Cell(22).GetString(),
                    RegionTienda = row.Cell(23).GetString()
                };

                listaVentas.Add(venta);
                registrosValidos++;

                var producto = _context.Productos
                    .FirstOrDefault(p => p.CodigoSKU == row.Cell(11).GetString());

                if (producto != null)
                {
                    listaVentaDetalles.Add(new VentaDetalle
                    {
                        Venta = venta,
                        Producto = producto,
                        Cantidad = row.Cell(13).GetValue<int>(),
                        PrecioUnitario = row.Cell(14).TryGetValue<decimal>(out decimal precio) ? precio : 0m,
                        Subtotal = row.Cell(16).TryGetValue<decimal>(out decimal subtotal) ? subtotal : 0m
                    });
                }
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                if (listaClientesNuevos.Any())
                    _context.Clientes.AddRange(listaClientesNuevos);

                if (listaVentas.Any())
                    _context.VentasImportadas.AddRange(listaVentas);

                if (listaVentaDetalles.Any())
                    _context.VentaDetalles.AddRange(listaVentaDetalles);

                _context.SaveChanges(); 

                string estadoImportacion = registrosValidos > 0 ? "EXT" : "ERR";

                var logImportacion = new HistorialImportacion
                {
                    FechaImportacion = DateTime.Now,
                    Usuario = "Admin",
                    RegistrosValidos = registrosValidos,
                    RegistrosRechazados = registrosRechazados,
                    RegistrosDuplicados = registrosDuplicados.Count,
                    NombreArchivo = file.FileName,
                    Estado = estadoImportacion
                };

                _context.HistorialImportaciones.Add(logImportacion);
                _context.SaveChanges();

                transaction.Commit();

                return Ok(new
                {
                    Mensaje = "Importación exitosa (sin predicciones)",
                    ReporteResumen = new
                    {
                        RegistrosValidos = registrosValidos,
                        RegistrosRechazados = registrosRechazados,
                        RegistrosDuplicados = registrosDuplicados
                    }
                });

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, new { mensaje = "Error en servidor", detalle = ex.Message });
            }
        }
    }
}
