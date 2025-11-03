using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MachineLearning.Models;
using MachineLearning.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using MachineLearning.DTO;

namespace MachineLearning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetVentas()
        {
            var ventas = await _context.VentasDTO
                .FromSqlRaw("EXEC sp_ListarVentasConCliente")
                .ToListAsync();

            return Ok(ventas);
        }


        [HttpGet("ventas_historial")]
        public async Task<ActionResult<IEnumerable<VentaImport>>> GetHistorialImportacion()
        {
            var ventas = await _context.HistorialImportaciones
                .FromSqlRaw("EXEC sp_ListarHistorial_Importacion")
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VentaImport>> GetVenta(int id)
        {
            var venta = await _context.VentasImportadas.FindAsync(id);
            if (venta == null) return NotFound();
            return venta;
        }

        [HttpPost]
        public async Task<ActionResult<VentaImport>> PostVenta([FromBody] VentaImport nuevaVenta)
        {
            _context.VentasImportadas.Add(nuevaVenta);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVenta), new { id = nuevaVenta.Id }, nuevaVenta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenta(int id, [FromBody] VentaImport ventaActualizada)
        {
            if (id != ventaActualizada.Id)
                return BadRequest();

            _context.Entry(ventaActualizada).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.VentasImportadas.Any(v => v.Id == id))
                    return NotFound();

                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            var venta = await _context.VentasImportadas.FindAsync(id);
            if (venta == null) return NotFound();

            _context.VentasImportadas.Remove(venta);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
