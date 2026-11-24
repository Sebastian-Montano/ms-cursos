using ms_cursos.Models;

namespace ms_cursos.Repositories;

public interface ICursosRepository
{
    Task<List<Curso>> FindAllAsync(string? semestre = null);
    Task<IReadOnlyList<Curso>> FindByIdsAsync(IReadOnlyCollection<int> ids);
    Task<Curso?> FindByIdAsync(int id);
    Task<Curso?> FindByCodigoAsync(string codigo);
    Task<IReadOnlyList<Curso>> FindBySemestreAsync(string semestre);

    Task<Curso> CreateAsync(Curso curso);
    Task<Curso?> UpdateAsync(int id, Curso curso);
    Task<bool> RemoveAsync(int id);
}
