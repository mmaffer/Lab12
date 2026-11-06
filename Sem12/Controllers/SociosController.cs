using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sem12.Data;
using Sem12.Models;

namespace Sem12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SociosController(GymnasioContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Socio>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Socio>>> GetAll([FromQuery] string? buscar)
    {
        var query = context.Socios.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(texto) ||
                p.Dni.Contains(texto) || p.Email.ToLower().Contains(texto));
        }
        return Ok(await query.OrderBy(p => p.IdSocio).ToListAsync());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Socio), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Socio>> GetById(int id)
    {
        var socio = await context.Socios.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdSocio == id);
        return socio is null ? NotFound() : Ok(socio);
    }

    [HttpGet("{id:int}/membresias")]
    [ProducesResponseType(typeof(IEnumerable<Membresia>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Membresia>>> GetMembresias(int id)
    {
        if (!await context.Socios.AnyAsync(p => p.IdSocio == id)) return NotFound();
        return Ok(await context.Membresias.AsNoTracking().Where(m => m.IdSocio == id)
            .OrderBy(m => m.IdMembresia).ToListAsync());
    }

    [HttpPost]
    [ProducesResponseType(typeof(Socio), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Socio>> Create(Socio socio)
    {
        if (socio.IdSocio != 0) return BadRequest("El ID se genera automáticamente; envíe 0 u omítalo.");
        Normalizar(socio);
        if (await Duplicado(socio)) return Conflict("Ya existe un socio con ese DNI o email.");
        socio.Membresias = new List<Membresia>();
        context.Socios.Add(socio);
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Conflict("Ya existe un socio con ese DNI o email."); }
        return CreatedAtAction(nameof(GetById), new { id = socio.IdSocio }, socio);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, Socio socio)
    {
        if (id != socio.IdSocio) return BadRequest("El ID de la ruta y del cuerpo deben coincidir.");
        var actual = await context.Socios.FindAsync(id);
        if (actual is null) return NotFound();
        Normalizar(socio);
        if (await Duplicado(socio)) return Conflict("Ya existe un socio con ese DNI o email.");
        actual.Nombre = socio.Nombre;
        actual.Dni = socio.Dni;
        actual.Telefono = socio.Telefono;
        actual.Email = socio.Email;
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Conflict("Ya existe un socio con ese DNI o email."); }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var socio = await context.Socios.FindAsync(id);
        if (socio is null) return NotFound();
        if (await context.Membresias.AnyAsync(m => m.IdSocio == id))
            return Conflict("No se puede eliminar un socio con membresias vinculadas.");
        context.Socios.Remove(socio);
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        { return Conflict("No se puede eliminar un socio con membresias vinculadas."); }
        return NoContent();
    }

    private Task<bool> Duplicado(Socio p) => context.Socios.AnyAsync(x =>
        x.IdSocio != p.IdSocio && (x.Dni == p.Dni || x.Email.ToLower() == p.Email));

    private static void Normalizar(Socio p)
    {
        p.Nombre = p.Nombre.Trim();
        p.Dni = p.Dni.Trim();
        p.Telefono = p.Telefono.Trim();
        p.Email = p.Email.Trim().ToLowerInvariant();
    }
}
