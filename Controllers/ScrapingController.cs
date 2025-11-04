using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace MachineLearning.Controllers
{
[Route("api/[controller]")]
[ApiController]
public class ScrapingController : ControllerBase
{
        [HttpGet("tendencias")]
        public async Task<IActionResult> ObtenerTendencias()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "tendencias_google.py",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                    return StatusCode(500, new { mensaje = "Error en Python", detalle = error });

                if (string.IsNullOrWhiteSpace(output))
                    return StatusCode(500, new { mensaje = "Salida Python vacía" });

                object tendencias;
                try
                {
                    tendencias = JsonSerializer.Deserialize<object>(output);
                }
                catch
                {
                    return StatusCode(500, new { mensaje = "Error al parsear JSON de Python", detalle = output });
                }

                return Ok(new
                {
                    Tendencias = tendencias,
                    ReporteResumen = new
                    {
                        Fuente = "Google Trends Perú",
                        FechaConsulta = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Total = (tendencias as IEnumerable<object>)?.Count() ?? 0
                    }
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error general en el backend", detalle = ex.Message });
            }
        }



        [HttpGet("lanapolis")]
        public async Task<IActionResult> ObtenerProductoslanapolis()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "lanapolis.py",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                    return StatusCode(500, new { mensaje = "Error en Python", detalle = error });

                if (string.IsNullOrWhiteSpace(output))
                    return StatusCode(500, new { mensaje = "Salida Python vacía" });

                object productos;
                try
                {
                    productos = JsonSerializer.Deserialize<object>(output);
                }
                catch
                {
                    return StatusCode(500, new { mensaje = "Error al parsear JSON de Python", detalle = output });
                }

                return Ok(new
                {
                    Productos = productos,
                    ReporteResumen = new
                    {
                        Fuente = "LanaPolis",
                        FechaConsulta = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Total = (productos as IEnumerable<object>)?.Count() ?? 0
                    }
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error general en el backend", detalle = ex.Message });
            }
        }



        [HttpGet("entrelanas")]
        public async Task<IActionResult> ObtenerProductosEntreLanas()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "entrelanas.py",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                    return StatusCode(500, new { mensaje = "Error en Python", detalle = error });

                if (string.IsNullOrWhiteSpace(output))
                    return StatusCode(500, new { mensaje = "Salida Python vacía" });

                object productos;
                try
                {
                    productos = JsonSerializer.Deserialize<object>(output);
                }
                catch
                {
                    return StatusCode(500, new { mensaje = "Error al parsear JSON de Python", detalle = output });
                }

                return Ok(new
                {
                    Productos = productos,
                    ReporteResumen = new
                    {
                        Fuente = "LanaPolis",
                        FechaConsulta = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Total = (productos as IEnumerable<object>)?.Count() ?? 0
                    }
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error general en el backend", detalle = ex.Message });
            }
        }


    }
}
