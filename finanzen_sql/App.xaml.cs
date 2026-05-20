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
        DatabaseUtils dbUtils = new DatabaseUtils();
        MySqlConnection connection = dbUtils.InitDatabase();

        dbUtils.CreateTables(connection);

        MainWindow window = new MainWindow();
        window.Show();
    }
}


