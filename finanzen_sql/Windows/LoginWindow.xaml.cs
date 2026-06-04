using finanzen_sql.utils;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;


namespace finanzen_sql.Windows;
public partial class LoginWindow : Window
{
    private readonly DatabaseUtils _dbUtils;
    public LoginWindow(DatabaseUtils databaseUtils)
    {
        InitializeComponent();
        _dbUtils = databaseUtils;
    }

    private void RouteToRegistrationClick(object sender, RoutedEventArgs e)
    {
        RegistrationWindow registration = new(_dbUtils)
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

        using MySqlConnection conn = _dbUtils.CreateConnection();

        // checks if the username and password are correct
        bool isCorrect = _dbUtils.LoginUser(conn, username, password);

        if (isCorrect == false)
        {
            MessageBox.Show("Das Passwort oder der Username ist falsch");
            return;
        }

        MessageBox.Show("Du hast dich erfolgreich eingeloggt");

        int userID = _dbUtils.GetUserID(conn, username);

        RouteToFinanzDodoClick(sender, e, userID);
    }

    private void RouteToFinanzDodoClick(object sender, RoutedEventArgs e, int userID)
    {
        FinanzDodoWindow finanzDodo = new(_dbUtils, userID)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        finanzDodo.Show();

        this.Close();
    }
}
