using ms_cursos.Exceptions;
using ms_cursos.Models;
using ms_cursos.Repositories;

namespace ms_cursos.Services;

public sealed class CursosService : ICursosService
{
    private readonly ICursosRepository _repo;

    public CursosService(ICursosRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<Curso>> ListarAsync(string? semestre = null)
    {
        if (string.IsNullOrWhiteSpace(semestre))
            return await _repo.FindAllAsync();

        var semestreNormalizado = semestre.Trim();
        ValidarSemestre(semestreNormalizado);

        return await _repo.FindBySemestreAsync(semestreNormalizado);
    }

    public async Task<Curso> ObtenerPorIdAsync(int id)
    {
        ValidarId(id);

        return await _repo.FindByIdAsync(id)
            ?? throw new ApiException(404, $"No existe el curso con id {id}");
    }

    public async Task<Curso> CrearAsync(CrearCursoRequest request)
    {
        var codigo = request.Codigo.Trim();

        if (await _repo.FindByCodigoAsync(codigo) is not null)
        {
            throw new ApiException(
                409,
                $"Ya existe un curso con el código {codigo}");
        }

        ValidarDatos(request.Creditos, request.Cupo, request.Semestre);

        var curso = new Curso
        {
            Codigo = codigo,
            Nombre = request.Nombre.Trim(),
            Creditos = request.Creditos,
            Cupo = request.Cupo,
            Semestre = request.Semestre.Trim()
        };

        return await _repo.CreateAsync(curso);
    }

    public async Task<Curso> ActualizarAsync(
        int id,
        ActualizarCursoRequest request)
    {
        ValidarId(id);

        var cursoExistente = await _repo.FindByIdAsync(id)
            ?? throw new ApiException(404, $"No existe el curso con id {id}");

        var codigo = !string.IsNullOrWhiteSpace(request.Codigo)
            ? request.Codigo.Trim()
            : cursoExistente.Codigo;

        if (!string.IsNullOrWhiteSpace(request.Codigo))
        {
            var otroConMismoCodigo = await _repo.FindByCodigoAsync(codigo);

            if (otroConMismoCodigo is not null && otroConMismoCodigo.Id != id)
            {
                throw new ApiException(
                    409,
                    $"Ya existe un curso con el código {codigo}");
            }
        }

        var nombre = !string.IsNullOrWhiteSpace(request.Nombre)
            ? request.Nombre.Trim()
            : cursoExistente.Nombre;

        var creditos = request.Creditos ?? cursoExistente.Creditos;
        var cupo = request.Cupo ?? cursoExistente.Cupo;
        var semestre = !string.IsNullOrWhiteSpace(request.Semestre)
            ? request.Semestre.Trim()
            : cursoExistente.Semestre;

        ValidarDatos(creditos, cupo, semestre);

        var curso = new Curso
        {
            Id = cursoExistente.Id,
            Codigo = codigo,
            Nombre = nombre,
            Creditos = creditos,
            Cupo = cupo,
            Semestre = semestre
        };

        return await _repo.UpdateAsync(id, curso)
            ?? throw new ApiException(
                500,
                $"Error interno: no se pudo actualizar el curso con id {id}");
    }

    public async Task<bool> EliminarAsync(int id)
    {
        ValidarId(id);

        var eliminado = await _repo.RemoveAsync(id);

        if (!eliminado)
            throw new ApiException(404, $"No existe el curso con id {id}");

        return eliminado;
    }

    private static void ValidarDatos(int creditos, int cupo, string semestre)
    {
        if (creditos is < 1 or > 6)
            throw new ApiException(400, "Los créditos deben estar entre 1 y 6.");

        if (cupo <= 0)
            throw new ApiException(400, "El cupo debe ser mayor que 0.");

        ValidarSemestre(semestre.Trim());
    }

    private static void ValidarSemestre(string semestre)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(
                semestre, @"^\d{4}-[12]$"))
        {
            throw new ApiException(
                400,
                "El semestre debe tener el formato AAAA-1 o AAAA-2.");
        }
    }

    private static void ValidarId(int id)
    {
        if (id <= 0)
        {
            throw new ApiException(
                400,
                $"El id '{id}' debe ser un entero positivo");
        }
    }
}
