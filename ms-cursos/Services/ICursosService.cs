using ms_cursos.Models;

namespace ms_cursos.Services;

public interface ICursosService
{
    Task<IReadOnlyList<Curso>> ListarAsync(string? semestre = null);

    Task<Curso> ObtenerPorIdAsync(int id);

    Task<Curso> CrearAsync(CrearCursoRequest request);

    Task<Curso> ActualizarAsync(int id, ActualizarCursoRequest request);

    Task<bool> EliminarAsync(int id);
}
