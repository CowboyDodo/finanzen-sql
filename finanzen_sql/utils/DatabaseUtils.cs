using finanzen_sql.classes;
using MySqlConnector;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Diagnostics;

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
    private readonly string _connection;

    public DatabaseUtils()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(basePath, "Auth", "Auth.json");
        string json = File.ReadAllText(path);

        DatabaseConfig? config = JsonSerializer.Deserialize<DatabaseConfig>(json);

        if (config == null) throw new Exception("Invalid database configuration JSON.");

        _connection = (
            $"SERVER={config.server};" +
            $"DATABASE={config.database};" +
            $"UID={config.user};" +
            $"PASSWORD={config.password}"
        );
    }

    public MySqlConnection CreateConnection()
    {
        try
        {
            MySqlConnection connection = new(_connection);
            connection.Open();
            return connection;
        } catch (Exception error) 
        {
            throw new Exception("Failed to connect to the database. Please check your config", error);
        }
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
    }

    /// <summary>
    /// Executes the provided SQL CREATE statement
    /// </summary>
    /// <param name="sqlQuery">SQL statement containing the querie to execute</param>
    /// <param name="connection">Current MySqlConnection used to execute the command</param>
    public static void ExecuteCreateQueries(string sqlQuery, MySqlConnection connection)
    {
        using MySqlCommand command = new(sqlQuery, connection);

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Checks if a username is already registered 
    /// </summary>
    /// <param name="connection">Current MySqlConnection used to execute the command</param>
    /// <param name="username">username of login guest</param>
    /// <returns>True if the username already exists; otherwise false.</returns>
    private bool CheckUser(MySqlConnection connection, string username)
    {
        string query = "SELECT username FROM User WHERE username = @username";

        using MySqlCommand cmd = new(query, connection);

        cmd.Parameters.AddWithValue("@username", username);

        object? result = cmd.ExecuteScalar();

        if(result == null) return false;

        return true;
    }

    /// <summary>
    /// Creates a user if not already existing
    /// </summary>
    /// <param name="connection">Current MySqlConnection used to execute the command</param>
    /// <param name="username">username of login guest</param>
    /// <param name="password">password of login guest</param>
    /// <seealso cref="https://claudiobernasconi.ch/blog/how-to-hash-passwords-with-bcrypt-in-csharp/"/>
    public void CreateUser(MySqlConnection connection, string username, string password)
    {   
        // removing whitespaces
        username = username.Trim();

        if (CheckUser(connection, username))
        {
            MessageBox.Show("Es gibt bereits einen solchen User");
            return;
        }

        string query = "INSERT INTO User (username, passwort) VALUES (@username,@password)";
        // 13 means a workfactor of 13 -> number of iterations to calculate the hash; the higher the better but also slower
        string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);

        using MySqlCommand cmd = new(query, connection);

        // Add parameter to prevent SQL injection
        cmd.Parameters.AddWithValue("@username", username);
        cmd.Parameters.AddWithValue("@password", hashedPassword);

        try
        {
            cmd.ExecuteNonQuery();
            MessageBox.Show("Du hast dich erfolgreich registriert");
            return;
        }
        catch (Exception error)
        {
            MessageBox.Show($"Fehler: {error.Message}");
            return;
        }
    }

    /// <summary>
    /// Checks if the passwort of the given username is correct
    /// </summary>
    /// <param name="connection">Current MySqlConnection used to execute the command</param>
    /// <param name="username">username of login guest</param>
    /// <param name="password">password of login guest</param>
    /// <returns>True if the password is correct, false if not</returns>
    public bool LoginUser(MySqlConnection connection, string username, string password)
    {
        string query = "SELECT passwort FROM user WHERE username = @username";

        using MySqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@username", username.Trim());

        object? result = cmd.ExecuteScalar();

        if (result == null) return false;

        string? storedHash = result.ToString();

        return BCrypt.Net.BCrypt.EnhancedVerify(password, storedHash);  // is already true or false
    }
}

