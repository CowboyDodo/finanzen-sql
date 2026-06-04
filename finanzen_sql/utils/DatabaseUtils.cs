using finanzen_sql.classes;
using MySqlConnector;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

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

public class Category
{
    public int id { get; set; }
    public required string name { get; set; }
    public required string typ { get; set; }
}

/// <summary>
/// Database Logic and interface between frontend and database
/// </summary>
public class DatabaseUtils
{
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
        }
        catch (Exception error)
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

        if (result == null) return false;

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
        // Trim username to prevent login problems due to leading or trailing whitespaces
        cmd.Parameters.AddWithValue("@username", username.Trim());

        object? result = cmd.ExecuteScalar();

        if (result == null) return false;

        string? storedHash = result.ToString();

        return BCrypt.Net.BCrypt.EnhancedVerify(password, storedHash);  // is already true or false
    }

    /// <summary>
    /// Gets all transactions of a user and returns them as a DataTable to be displayed in the frontend
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="user_id"></param>
    /// <returns>Table</returns>
    public DataTable GetTransactions(
        MySqlConnection connection,
        int userId,
        int pageSize,
        int offset,
        int? categotyId = null,
        string typ = ""
    )
    {
        // LEFT JOIIN is used to also get transactions without a category, which would be lost with an INNER JOIN -> null value doesnt exists in boht tables
        // COALESCE is used to display "Keine Angabe" and "-" instead of empty values for those transactions
        StringBuilder query = new(@"
            SELECT
                t.id AS Id,
                t.betrag AS Betrag,
                t.beschreibung AS Beschreibung,
                COALESCE(k.name, 'Keine Angabe') AS Kategorie,
                COALESCE(k.typ, '-') AS Typ,
                t.datum AS Datum
            FROM transaktion t
            LEFT JOIN kategorie k
                ON t.kategorie_id = k.id
            WHERE t.user_id = @userId
        ");

        if (categotyId != null) query.Append("AND k.id = @categoryId ");
        if (typ != "") query.Append("AND k.typ = @typ");

        query.Append(@"
            ORDER BY t.id DESC
            LIMIT @pageSize OFFSET @offset
        ");

        using MySqlCommand cmd = new(query.ToString(), connection);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@pageSize", pageSize);
        cmd.Parameters.AddWithValue("@offset", offset);

        if (categotyId != null) cmd.Parameters.AddWithValue("@categoryId", categotyId);
        if (typ != "") cmd.Parameters.AddWithValue("@typ", typ);

        using MySqlDataAdapter adapter = new(cmd);

        DataTable table = new();
        adapter.Fill(table);

        return table;
    }

    /// <summary>
    /// Get the total count of transactions for a user, which is needed to calculate the number of pages for the pagination in the frontend
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public int GetTransactionCount(MySqlConnection connection, int userId)
    {
        string query = @"
            SELECT COUNT(*)
            FROM transaktion
            WHERE user_id = @userId";

        using MySqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@userId", userId);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    /// <summary>
    /// Helps to get the user id of a user based on the username,
    /// which is needed for other queries to get the transactions or push new transactions for the user
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public int GetUserID(MySqlConnection connection, string username)
    {
        string query = "SELECT id FROM user WHERE username = @username";

        using MySqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@username", username.Trim());

        object? result = cmd.ExecuteScalar();

        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Gets all categories from the database and returns them as a list of Category objects
    /// to be displayed in the frontend when creating a new transaction
    /// </summary>
    /// <param name="connection">Open MySQL database connection.</param>
    /// <returns></returns>
    public List<Category> GetCategories(MySqlConnection connection)
    {
        string query = "SELECT id, name, typ FROM kategorie";

        using MySqlCommand cmd = new(query, connection);
        using MySqlDataReader result = cmd.ExecuteReader();

        List<Category> categories = new();

        while (result.Read())
        {
            categories.Add(new Category
            {
                id = result.GetInt32("id"),
                name = result.GetString("name"),
                typ = result.GetString("typ")
            });
        }

        return categories;
    }

    /// <summary>
    /// Inserts a new transaction into the database.
    /// </summary>
    /// <param name="connection">Open MySQL database connection.</param>
    /// <param name="userID">ID of the user who owns the transaction.</param>
    /// <param name="amount">Transaction amount (positive or negative depending on type).</param>
    /// <param name="description">Optional description of the transaction.</param>
    /// <param name="isRecurring">Indicates whether the transaction is recurring.</param>
    /// <param name="categoryID">Optional category ID. Can be null if no category is selected.</param>
    /// <param name="date">Date of the transaction. If not provided by the caller, the current date should be used.</param>
    /// <returns>True if the transaction was successfully added; otherwise, false.</returns>
    public bool PushTransaction(
        MySqlConnection connection,
        int userID,
        decimal amount,
        string description,
        bool isRecurring,
        int? categoryID,
        DateTime? date
    )
    {
        string query = @"
            INSERT INTO transaktion (user_id, betrag, beschreibung, istWiederkehrend, kategorie_id, datum) 
            VALUES (@userID, @amount, @description, @isRecurring, @categoryID, @date)";

        using MySqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@userID", userID);
        cmd.Parameters.AddWithValue("@amount", amount);
        cmd.Parameters.AddWithValue("@description", description);
        cmd.Parameters.AddWithValue("@isRecurring", isRecurring);
        cmd.Parameters.AddWithValue("@categoryID", categoryID);
        cmd.Parameters.AddWithValue("@date", date);

        try
        {
            cmd.ExecuteNonQuery();
            MessageBox.Show("Transaktion erfolgreich hinzugefügt");
            return true;
        }
        catch (Exception)
        {
            MessageBox.Show("Bitte überprüfe all deine Eingaben");
            return false;
        }
    }

    /// <summary>
    /// Deletes a transaction from the database
    /// </summary>
    /// <param name="connection">Open MySQL database connection.</param>
    /// <param name="userID">ID of the user who owns the transaction.</param>
    /// <param name="transactionID">ID of the chosen transaction</param>
    public void DeleteTransaction(MySqlConnection connection, int userID, int transactionID)
    {
        string query = "DELETE FROM transaktion WHERE id = @transactionID AND user_id = @userID";

        using MySqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@transactionID", transactionID);
        cmd.Parameters.AddWithValue("@userID", userID);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Edits an existing transaction in the database
    /// </summary>
    /// <param name="connection">Open MySQL database connection.</param>
    /// <param name="userID">ID of the user who owns the transaction.</param>
    /// <param name="amount">Transaction amount (positive or negative depending on type).</param>
    /// <param name="description">Optional description of the transaction.</param>
    /// <param name="isRecurring">Indicates whether the transaction is recurring.</param>
    /// <param name="categoryID">Optional category ID. Can be null if no category is selected.</param>
    /// <param name="date">Date of the transaction. If not provided by the caller, the current date should be used.</param>
    /// <returns>True if the transaction was successfully added; otherwise, false.</returns>
    public bool EditTransaction(
        MySqlConnection connection,
        int transactionID,
        int userID,
        decimal amount,
        string description,
        bool isRecurring,
        int? categoryID,
        DateTime? date
    )
    {
        string query = @"
            UPDATE transaktion 
            SET betrag = @amount, beschreibung = @description, istWiederkehrend = @isRecurring, kategorie_id = @categoryID, datum = @date
            WHERE id = @transactionID AND user_id = @userID";
        using MySqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@transactionID", transactionID);
        cmd.Parameters.AddWithValue("@userID", userID);
        cmd.Parameters.AddWithValue("@amount", amount);
        cmd.Parameters.AddWithValue("@description", description);
        cmd.Parameters.AddWithValue("@isRecurring", isRecurring);
        cmd.Parameters.AddWithValue("@categoryID", categoryID);
        cmd.Parameters.AddWithValue("@date", date);
        try
        {
            cmd.ExecuteNonQuery();
            MessageBox.Show("Transaktion erfolgreich editiert");
            return true;
        }
        catch (Exception)
        {
            MessageBox.Show("Bitte überprüfe all deine Eingaben");
            return false;
        }
    }
}

