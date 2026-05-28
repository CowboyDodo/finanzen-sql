using finanzen_sql.utils;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;


namespace finanzen_sql.Windows;
public partial class LoginWindow : Window
{
    private readonly DatabaseUtils dbUtils;
    public LoginWindow(DatabaseUtils databaseUtils)
    {
        InitializeComponent();
        dbUtils = databaseUtils;
    }

    private void RouteToRegistrationClick(object sender, RoutedEventArgs e)
    {
        RegistrationWindow registration = new(dbUtils)
        {
            // set window in the center
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        registration.Show();

        this.Close();
    }

    private void UserLogin(object sender, RoutedEventArgs e)
    {
        string username = UsernameTBox.Text;
        string password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Gib bitte alle Daten an");
            return;
        }

        using MySqlConnection conn = dbUtils.CreateConnection();

        bool result = dbUtils.LoginUser(conn, username, password);

        if (result == false)
        {
            MessageBox.Show("Das Passwort oder der Username ist falsch");
            return;
        }

        MessageBox.Show("Du hast dich erfolgreich eingeloggt");
        return;
    }
}
