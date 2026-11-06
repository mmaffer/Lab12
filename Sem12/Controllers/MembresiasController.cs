using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sem12.Data;
using Sem12.Models;

namespace Sem12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembresiasController(GymnasioContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Membresia>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Membresia>>> GetAll([FromQuery] string? plan)
    {
        var query = context.Membresias.AsNoTracking().Include(m => m.Socio).AsQueryable();
        if (!string.IsNullOrWhiteSpace(plan))
        {
            var texto = plan.Trim().ToLower();
            query = query.Where(m => m.Plan.ToLower().Contains(texto));
        }
        return Ok(await query.OrderBy(m => m.IdMembresia).ToListAsync());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Membresia), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Membresia>> GetById(int id)
    {
        var membresia = await context.Membresias.AsNoTracking().Include(m => m.Socio)
            .FirstOrDefaultAsync(m => m.IdMembresia == id);
        return membresia is null ? NotFound() : Ok(membresia);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Membresia), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Membresia>> Create(Membresia membresia)
    {
        if (membresia.IdMembresia != 0) return BadRequest("El ID se genera automáticamente; envíe 0 u omítalo.");
        if (!await context.Socios.AnyAsync(p => p.IdSocio == membresia.IdSocio))
            return BadRequest("El socio indicado no existe.");
        // Solo se usa la FK; evita insertar un socio recibido como objeto anidado.
        membresia.Socio = null;
        context.Membresias.Add(membresia);
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        { return BadRequest("El socio indicado no existe."); }
        return CreatedAtAction(nameof(GetById), new { id = membresia.IdMembresia }, membresia);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, Membresia membresia)
    {
        if (id != membresia.IdMembresia) return BadRequest("El ID de la ruta y del cuerpo deben coincidir.");
        var actual = await context.Membresias.FindAsync(id);
        if (actual is null) return NotFound();
        if (!await context.Socios.AnyAsync(p => p.IdSocio == membresia.IdSocio))
            return BadRequest("El socio indicado no existe.");
        actual.Plan = membresia.Plan;
        actual.TipoAcceso = membresia.TipoAcceso;
        actual.Turno = membresia.Turno;
        actual.DuracionMeses = membresia.DuracionMeses;
        actual.Precio = membresia.Precio;
        actual.IdSocio = membresia.IdSocio;
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        { return BadRequest("El socio indicado no existe."); }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var membresia = await context.Membresias.FindAsync(id);
        if (membresia is null) return NotFound();
        context.Membresias.Remove(membresia);
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        return NoContent();
    }
}
