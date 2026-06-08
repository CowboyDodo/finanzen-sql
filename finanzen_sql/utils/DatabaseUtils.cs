using finanzen_sql.classes;
using finanzen_sql.Tables;
using Google.Protobuf;
using MySqlConnector;
using System.Data;
using System.Diagnostics;
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
    public bool CheckUser(MySqlConnection connection, string username)
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
    public bool CreateUser(MySqlConnection connection, string username, string password)
    {
        // removing whitespaces
        username = username.Trim();

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
            return true;
        }
        catch (Exception error)
        {
            MessageBox.Show($"Fehler: {error.Message}");
            return false;
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
    public List<Transaction> GetTransactions(
        MySqlConnection connection,
        int userId,
        int pageSize,
        int offset,
        int? categoryId = null,
        string type = "",
        string isRecurring = ""
    )
    {
        // LEFT JOIIN is used to also get transactions without a category, which would be lost with an INNER JOIN -> null value doesnt exists in boht tables
        // COALESCE is used to display "Keine Angabe" and "-" instead of empty values for those transactions
        StringBuilder query = new(@"
            SELECT
                t.id AS TransactionID,
                t.betrag AS Amount,
                t.beschreibung AS Description,
                t.datum AS Date,
                t.istWiederkehrend AS Recurring,
                t.user_id AS UserID,
                t.kategorie_id AS CategoryID,
                COALESCE(k.name, 'Keine Angabe') AS CategoryName,
                COALESCE(k.typ, '-') AS CategoryType
            FROM transaktion t
            LEFT JOIN kategorie k
                ON t.kategorie_id = k.id
            WHERE t.user_id = @userId
        ");
        
        // add filter to string if variables not null
        if (categoryId != null) query.Append("AND k.id = @categoryId ");
        if (type != "") query.Append("AND k.typ = @type ");
        if (isRecurring != "") query.Append("AND t.istWiederkehrend = @isRecurring");

        query.Append(@"
            ORDER BY t.id DESC
            LIMIT @pageSize OFFSET @offset
        ");

        using MySqlCommand cmd = new(query.ToString(), connection);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@pageSize", pageSize);
        cmd.Parameters.AddWithValue("@offset", offset);

        if (categoryId != null) cmd.Parameters.AddWithValue("@categoryId", categoryId);
        if (type != "") cmd.Parameters.AddWithValue("@type", type);
        if (isRecurring == "ja") cmd.Parameters.AddWithValue("@isRecurring", true);
        if (isRecurring == "nein") cmd.Parameters.AddWithValue("@isRecurring", false);

        using MySqlDataReader reader = cmd.ExecuteReader();

        List<Transaction> allTransactions = new();

        while (reader.Read())
        {
            allTransactions.Add(new Transaction
            {
                Id = reader.GetInt32("TransactionID"),
                Amount = reader.GetDecimal("Amount"),
                Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                IsRecurring = reader.GetBoolean("Recurring"),
                UserID = reader.GetInt32("UserID"),
                CategoryID = reader.IsDBNull("CategoryID") ? 0 : reader.GetInt32("CategoryID"),
                Date = reader.GetDateTime("Date"),
                CategoryName = reader.GetString("CategoryName"),
                CategoryType = reader.GetString("CategoryType")
            });
        }

        return allTransactions;
    }

    /// <summary>
    /// Get the total count of transactions for a user, which is needed to calculate the number of pages for the pagination in the frontend
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="userId">Id of the current user</param>
    /// <param name="categoryId">Id of the category</param>
    /// <param name="type">"Einahme" or "Ausgabe"</param>
    /// <param name="isRecurring">"ja" oder "nein"</param>
    /// <returns></returns>
    public int GetTransactionCount(
        MySqlConnection connection,
        int userId,
        int? categoryId = null,
        string type = "",
        string isRecurring = ""
    )
    {
        StringBuilder query = new(@"
            SELECT COUNT(*)
            FROM transaktion t
            LEFT JOIN kategorie k ON t.kategorie_id = k.id
            WHERE t.user_id = @userId
        ");

        // add filter to string if variables not null
        if (categoryId != null) query.Append("AND t.kategorie_id = @categoryId ");
        if (type != "") query.Append("AND k.typ = @type ");
        if (isRecurring != "") query.Append("AND t.istWiederkehrend = @isRecurring");

        using MySqlCommand cmd = new(query.ToString(), connection);
        cmd.Parameters.AddWithValue("@userId", userId);

        if (categoryId != null) cmd.Parameters.AddWithValue("@categoryId", categoryId);
        if (type != "") cmd.Parameters.AddWithValue("@type", type);
        if (isRecurring == "ja") cmd.Parameters.AddWithValue("@isRecurring", true);
        if (isRecurring == "nein") cmd.Parameters.AddWithValue("@isRecurring", false);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    /// <summary>
    /// Helps to get the user id of a user based on the username,
    /// which is needed for the login/registration process
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public User? GetUserByName(MySqlConnection connection, string username)
    {
        string query = "SELECT * FROM user WHERE username = @username";

        using MySqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@username", username.Trim());

        using MySqlDataReader reader = cmd.ExecuteReader();

        if (!reader.Read()) return null;

        return new User { 
            Id = reader.GetInt32("id"),
            Username = reader.GetString("username"),
            Password = reader.GetString("passwort"),
            Budget = reader.GetDecimal("budget")
        };
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
                Id = result.GetInt32("id"),
                Name = result.GetString("name"),
                Type = result.GetString("typ")
            });
        }

        return categories;
    }

    /// <summary>
    /// Inserts a new transaction into the database.
    /// </summary>
    /// <param name="connection">Open MySQL database connection.</param>
    /// <param name="transaction">Transaction class filled with parameters.</param>
    /// <returns>True if the transaction was successfully added; otherwise, false.</returns>
    public bool PushTransaction(
        MySqlConnection connection,
        Transaction transaction
    )
    {
        string query = @"
            INSERT INTO transaktion (user_id, betrag, beschreibung, istWiederkehrend, kategorie_id, datum) 
            VALUES (@userID, @amount, @description, @isRecurring, @categoryID, @date)";

        using MySqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@userID", transaction.UserID);
        cmd.Parameters.AddWithValue("@amount", transaction.Amount);
        cmd.Parameters.AddWithValue("@description", transaction.Description);
        cmd.Parameters.AddWithValue("@isRecurring", transaction.IsRecurring);
        cmd.Parameters.AddWithValue("@categoryID", transaction.CategoryID);
        cmd.Parameters.AddWithValue("@date", transaction.Date);

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
    /// <param name="transaction">Transaction class filled with parameters</param>
    /// <returns>True if the transaction was successfully added; otherwise, false.</returns>
    public bool EditTransaction(
        MySqlConnection connection,
        Transaction transaction
    )
    {
        string query = @"
            UPDATE transaktion 
            SET betrag = @amount, beschreibung = @description, istWiederkehrend = @isRecurring, kategorie_id = @categoryID, datum = @date
            WHERE id = @transactionID AND user_id = @userID";

        using MySqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@transactionID", transaction.Id);
        cmd.Parameters.AddWithValue("@userID", transaction.UserID);
        cmd.Parameters.AddWithValue("@amount", transaction.Amount);
        cmd.Parameters.AddWithValue("@description", transaction.Description);
        cmd.Parameters.AddWithValue("@isRecurring", transaction.IsRecurring);
        cmd.Parameters.AddWithValue("@categoryID", transaction.CategoryID);
        cmd.Parameters.AddWithValue("@date", transaction.Date);

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

