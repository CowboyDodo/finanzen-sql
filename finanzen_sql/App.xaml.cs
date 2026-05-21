using finanzen_sql.utils;
using System.Configuration;
using System.Data;
using System.Windows;
using MySqlConnector;

namespace finanzen_sql;
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // init datatbase and create tables if they don't exist
        DatabaseUtils dbUtils = new();
        MySqlConnection connection;
        try
        {
            connection = dbUtils.InitDatabase();
        }
        catch (Exception error)
        {
            MessageBox.Show(
                $"Error connecting to the database: {error.Message}",
                "Database Connection Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            Shutdown();
            return;
        }

        dbUtils.CreateTables(connection);

        // start main window
        MainWindow window = new();
        window.Show();
    }
}


