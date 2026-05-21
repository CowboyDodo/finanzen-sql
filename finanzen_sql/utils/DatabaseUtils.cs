using MySqlConnector;
using System.IO;
using System.Text.Json;
using finanzen_sql.classes;
using System.Data;

namespace finanzen_sql.utils;
/// <summary>
/// Stores the database connection settings that are loaded from the auth.json file
/// </summary>
public class DatabaseConfig
{
    public string server { get; set; } = "";
    public string database { get; set; } = "";
    public string user { get; set; } = "";
    public string password { get; set; } = "";
}

/// <summary>
/// Database Logic and interface between frontend and database
/// </summary>
public class DatabaseUtils {
    protected MySqlConnection? connection;

    public MySqlConnection InitDatabase()
    {
        string json = File.ReadAllText("Auth/Auth.json");
        DatabaseConfig? config = JsonSerializer.Deserialize<DatabaseConfig>(json);

        if (config == null) throw new Exception("Invalid database configuration JSON.");

        try
        {
            connection = new MySqlConnection(
                $"SERVER={config.server};" +
                $"DATABASE={config.database};" +
                $"UID={config.user};" +
                $"PASSWORD={config.password}"
            );

            connection.Open();
        }
        catch (Exception error)
        {
            throw new Exception("Failed to connect to the database. Please check your configuration.", error);
        }

        return connection;
    }

    /// <summary>
    /// Creates the database tables and closes the provided connection
    /// </summary>
    /// <param name="connection">An open MySqlConnection used to execute the command</param>
    public void CreateTables(MySqlConnection connection)
    {
        DatabaseTables dbTables = new();

        ExecuteCreateQueries(dbTables.CreateTableUser(), connection);
        ExecuteCreateQueries(dbTables.CreateTableCategory(), connection);
        ExecuteCreateQueries(dbTables.CreateTableTransaction(), connection);

        connection.Close();
    }

    /// <summary>
    /// Executes the provided SQL CREATE statement
    /// </summary>
    /// <param name="sqlQuery">SQL statement containing the querie to execute</param>
    /// <param name="connection">Open MySqlConnection used to execute the command</param>
    public void ExecuteCreateQueries(string sqlQuery, MySqlConnection connection)
    {
        using MySqlCommand command = new(sqlQuery, connection);

        command.ExecuteNonQuery();
    }
}

