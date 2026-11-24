using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ms_cursos.Models;
using ms_cursos.Services;

namespace ms_cursos.Controllers;

[ApiController]
[Route("cursos")]
[Authorize]
public class CursosController : ControllerBase
{
    private readonly ICursosService _service;

    public CursosController(ICursosService service)
    {
        _service = service;
    }

    // GET /cursos
    // GET /cursos?semestre=2026-1
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<Curso>>> Listar(
        [FromQuery] string? semestre = null)
    {
        var cursos = await _service.ListarAsync(semestre);
        return Ok(cursos);
    }

    // GET /cursos/{id}
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Curso>> ObtenerPorId(int id)
    {
        var curso = await _service.ObtenerPorIdAsync(id);
        return Ok(curso);
    }

    // POST /cursos
    [HttpPost]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<Curso>> Crear(
        [FromBody] CrearCursoRequest request)
    {
        var nuevo = await _service.CrearAsync(request);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = nuevo.Id },
            nuevo);
    }

    // PUT /cursos/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "administrador")]
    public async Task<ActionResult<Curso>> Actualizar(
        int id,
        [FromBody] ActualizarCursoRequest request)
    {
        var actualizado = await _service.ActualizarAsync(id, request);
        return Ok(actualizado);
    }

    // DELETE /cursos/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
