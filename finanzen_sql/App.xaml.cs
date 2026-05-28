using finanzen_sql.utils;
using System.Windows;
using MySqlConnector;
using finanzen_sql.Windows;

namespace finanzen_sql;
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // init base logic of an WPF application
        base.OnStartup(e);

        // init datatbase and create tables if they don't exist
        DatabaseUtils dbUtils = new();

        using MySqlConnection connection = dbUtils.CreateConnection();

        dbUtils.CreateTables(connection);

        // start main window
        MainWindow window = new(dbUtils)
        {
            // fix window in the center
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        window.Show();
    }
}
