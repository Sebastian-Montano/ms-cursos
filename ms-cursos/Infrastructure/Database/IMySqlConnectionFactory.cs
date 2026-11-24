using MySqlConnector;

namespace ms_cursos.Infrastructure.Database;

public interface IMySqlConnectionFactory
{
    MySqlConnection CreateConnection();
}