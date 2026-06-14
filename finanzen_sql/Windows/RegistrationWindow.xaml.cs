using finanzen_sql.Tables;
using finanzen_sql.utils;
using MySqlConnector;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Input;


namespace finanzen_sql.Windows;

public partial class RegistrationWindow : Window
{
    private readonly DatabaseUtils _dbUtils;
    public RegistrationWindow(DatabaseUtils databaseUtils)
    {
        InitializeComponent();
        _dbUtils = databaseUtils;
    }

    private void RouteToLoginClick(object sender, RoutedEventArgs e)
    {
        LoginWindow login = new(_dbUtils)
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
        using MySqlConnection conn = _dbUtils.CreateConnection();

        bool isRegistered = _dbUtils.CheckUser(conn, username);
        if (isRegistered)
        {
            MessageBox.Show("Dieser Benutzername ist bereits vergeben");
            return;
        }

        bool isCreated = _dbUtils.CreateUser(conn, username, password);
        if (!isCreated) return;

        MessageBox.Show("Du hast dich erfolgreich registriert");

        User? user = _dbUtils.GetUserByName(conn, username);

        if (user == null)
        {
            MessageBox.Show("Fehler beim Abrufen der Benutzerdaten");
            return;
        }

        RouteToFinanzDodoClick(sender, e, user);
    }

    private void RouteToFinanzDodoClick(object sender, RoutedEventArgs e, User user)
    {
        FinanzDodoWindow finanzDodo = new(_dbUtils, user)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        finanzDodo.Show();

        this.Close();
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            UserRegistration(sender, e);
    }
}
