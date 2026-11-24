using ms_cursos.Models;
using ms_cursos.Infrastructure.Database;
using MySqlConnector;

namespace ms_cursos.Repositories;

public sealed class CursosRepository : ICursosRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public CursosRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static Curso MapCurso(MySqlDataReader reader)
    {
        return new Curso
        {
            Id = reader.GetInt32("id"),
            Codigo = reader.GetString("codigo"),
            Nombre = reader.GetString("nombre"),
            Creditos = reader.GetInt32("creditos"),
            Cupo = reader.GetInt32("cupo"),
            Semestre = reader.GetString("semestre")
        };
    }

    // Obtiene todos los cursos, o filtra por semestre si se indica.
    public async Task<List<Curso>> FindAllAsync(string? semestre = null)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            SELECT id, codigo, nombre, creditos, cupo, semestre
            FROM cursos
            """;

        if (!string.IsNullOrWhiteSpace(semestre))
            sql += " WHERE semestre = @semestre";

        sql += " ORDER BY id;";

        await using var command = new MySqlCommand(sql, connection);

        if (!string.IsNullOrWhiteSpace(semestre))
            command.Parameters.AddWithValue("@semestre", semestre);

        await using var reader = await command.ExecuteReaderAsync();

        var cursos = new List<Curso>();

        while (await reader.ReadAsync())
            cursos.Add(MapCurso(reader));

        return cursos;
    }

    // Obtiene cursos cuyos IDs estén dentro de la colección.
    public async Task<IReadOnlyList<Curso>> FindByIdsAsync(
        IReadOnlyCollection<int> ids)
    {
        if (ids.Count == 0)
            return Array.Empty<Curso>();

        var idsUnicos = ids.Distinct().ToList();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var parameterNames = idsUnicos
            .Select((_, index) => $"@id{index}")
            .ToList();

        var sql = $"""
            SELECT id, codigo, nombre, creditos, cupo, semestre
            FROM cursos
            WHERE id IN ({string.Join(", ", parameterNames)})
            ORDER BY id;
            """;

        await using var command = new MySqlCommand(sql, connection);

        for (var i = 0; i < idsUnicos.Count; i++)
            command.Parameters.AddWithValue(parameterNames[i], idsUnicos[i]);

        await using var reader = await command.ExecuteReaderAsync();

        var cursos = new List<Curso>();

        while (await reader.ReadAsync())
            cursos.Add(MapCurso(reader));

        return cursos;
    }

    public async Task<Curso?> FindByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT id, codigo, nombre, creditos, cupo, semestre
            FROM cursos
            WHERE id = @id;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapCurso(reader) : null;
    }

    public async Task<Curso?> FindByCodigoAsync(string codigo)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT id, codigo, nombre, creditos, cupo, semestre
            FROM cursos
            WHERE codigo = @codigo;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@codigo", codigo);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapCurso(reader) : null;
    }

    public async Task<IReadOnlyList<Curso>> FindBySemestreAsync(string semestre)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT id, codigo, nombre, creditos, cupo, semestre
            FROM cursos
            WHERE semestre = @semestre
            ORDER BY id;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@semestre", semestre);

        await using var reader = await command.ExecuteReaderAsync();

        var cursos = new List<Curso>();

        while (await reader.ReadAsync())
            cursos.Add(MapCurso(reader));

        return cursos;
    }

    public async Task<Curso> CreateAsync(Curso curso)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            INSERT INTO cursos (codigo, nombre, creditos, cupo, semestre)
            VALUES (@codigo, @nombre, @creditos, @cupo, @semestre);
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@codigo", curso.Codigo);
        command.Parameters.AddWithValue("@nombre", curso.Nombre);
        command.Parameters.AddWithValue("@creditos", curso.Creditos);
        command.Parameters.AddWithValue("@cupo", curso.Cupo);
        command.Parameters.AddWithValue("@semestre", curso.Semestre);

        await command.ExecuteNonQueryAsync();

        return new Curso
        {
            Id = checked((int)command.LastInsertedId),
            Codigo = curso.Codigo,
            Nombre = curso.Nombre,
            Creditos = curso.Creditos,
            Cupo = curso.Cupo,
            Semestre = curso.Semestre
        };
    }

    public async Task<Curso?> UpdateAsync(int id, Curso curso)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            UPDATE cursos
            SET codigo = @codigo,
                nombre = @nombre,
                creditos = @creditos,
                cupo = @cupo,
                semestre = @semestre
            WHERE id = @id;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@codigo", curso.Codigo);
        command.Parameters.AddWithValue("@nombre", curso.Nombre);
        command.Parameters.AddWithValue("@creditos", curso.Creditos);
        command.Parameters.AddWithValue("@cupo", curso.Cupo);
        command.Parameters.AddWithValue("@semestre", curso.Semestre);
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();

        const string selectSql = """
            SELECT id, codigo, nombre, creditos, cupo, semestre
            FROM cursos
            WHERE id = @id;
            """;

        await using var selectCommand = new MySqlCommand(selectSql, connection);
        selectCommand.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCommand.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapCurso(reader) : null;
    }

    public async Task<bool> RemoveAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            DELETE FROM cursos
            WHERE id = @id;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }
}
