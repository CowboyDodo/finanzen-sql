using finanzen_sql.utils;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using MySqlConnector;


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

        _dbUtils.CreateUser(conn, username, password);

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
