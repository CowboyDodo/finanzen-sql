using MySqlConnector;
using System.IO;
using System.Text.Json;
using finanzen_sql.classes;
using System.Data;

namespace finanzen_sql.utils;
public class DatabaseConfig
{
    public string server { get; set; } = "";
    public string database { get; set; } = "";
    public string user { get; set; } = "";
    public string password { get; set; } = "";
}
public class DatabaseUtils {
    protected MySqlConnection? connection;

    public MySqlConnection InitDatabase()
    {
        string json = File.ReadAllText("auth/auth.json");
        DatabaseConfig? config = JsonSerializer.Deserialize<DatabaseConfig>(json);

        if (config == null) throw new Exception("Invalid database configuration JSON.");

        connection = new MySqlConnection(
            $"SERVER={config.server};" +
            $"DATABASE={config.database};" +
            $"UID={config.user};" +
            $"PASSWORD={config.password}"
        );

        connection.Open();

        return connection;
    }

    public void CreateTables(MySqlConnection connection)
    {
        DatabaseTables dbTables = new DatabaseTables();

        ExecuteCreateQueries(dbTables.CreateTableCategory(), connection);
        ExecuteCreateQueries(dbTables.CreateTableRevenue(), connection);
        ExecuteCreateQueries(dbTables.CreateTableSpending(), connection);

        connection.Close();
    }

    public void ExecuteCreateQueries(string sqlQuery, MySqlConnection connection)
    {
        using MySqlCommand command = new MySqlCommand(sqlQuery, connection);

        command.ExecuteNonQuery();
    }
}

