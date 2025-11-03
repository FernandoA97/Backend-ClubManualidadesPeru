using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MachineLearning.Data;

namespace MachineLearning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrediccionVentasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrediccionVentasController(ApplicationDbContext context)
        {
            _context = context;
        }

 
        public class FiltrosRequest
        {
            public string HorizontePrediccion { get; set; } 
            public string Region { get; set; }            
            public string MetodoPago { get; set; }       
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerPredicciones([FromBody] FiltrosRequest filtros)
        {
           
            var ventasQuery = _context.VentasImportadas.AsQueryable();

          
            if (!string.IsNullOrEmpty(filtros.Region))
                ventasQuery = ventasQuery.Where(v => v.RegionTienda == filtros.Region);

            if (!string.IsNullOrEmpty(filtros.MetodoPago))
                ventasQuery = ventasQuery.Where(v => v.MetodoPago == filtros.MetodoPago);

          
            var listaVentas = await ventasQuery
                .Select(v => new {
                    v.FechaDeMision,
                    v.MontoTotal,
                    v.RegionTienda,
                    v.MetodoPago
                })
                .ToListAsync();

            if (!listaVentas.Any())
                return Ok(new { mensaje = "No hay ventas para los filtros aplicados", Predicciones = new List<object>() });

         
            var tempFile = Path.GetTempFileName();
            var jsonVentas = JsonSerializer.Serialize(listaVentas);
            System.IO.File.WriteAllText(tempFile, jsonVentas);

       
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"predict_ventas.py \"{tempFile}\" \"{filtros.HorizontePrediccion}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
                return StatusCode(500, new { mensaje = "Error en Python", detalle = error });

            if (string.IsNullOrWhiteSpace(output))
                return StatusCode(500, new { mensaje = "Salida Python vacía" });

           
            object predicciones;
            try
            {
                predicciones = JsonSerializer.Deserialize<object>(output);
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Error al parsear JSON de Python", detalle = output });
            }

          
            return Ok(new
            {
                Predicciones = predicciones,
                ReporteResumen = new
                {
                    RegistrosValidos = listaVentas.Count
                }
            });
        }
    }
}
