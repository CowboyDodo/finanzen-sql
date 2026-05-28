using finanzen_sql.utils;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using MySqlConnector;


namespace finanzen_sql.Windows;

public partial class RegistrationWindow : Window
{
    private readonly DatabaseUtils dbUtils;
    public RegistrationWindow(DatabaseUtils databaseUtils)
    {
        InitializeComponent();
        dbUtils = databaseUtils;
    }

    private void RouteToLoginClick(object sender, RoutedEventArgs e)
    {
        LoginWindow login = new(dbUtils)
        {
            // fix window in the center
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        login.Show();

        this.Close();

    }

    private void UserRegistration(object sender, RoutedEventArgs e)
    {
        string username = UsernameTBox.Text;
        string password = PasswordBox.Password;
        string repeatedPassword = Password2Box.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(repeatedPassword))
        {
            MessageBox.Show("Gib bitte alle Daten an");
            return;
        }

        if (password != repeatedPassword) {
            MessageBox.Show("Du hast nicht dasselbe Password eingegeben");
            return;
        }

        // using -> after execution dispose conncetion
        using MySqlConnection conn = dbUtils.CreateConnection();

        dbUtils.CreateUser(conn, username, password);
    }
}
