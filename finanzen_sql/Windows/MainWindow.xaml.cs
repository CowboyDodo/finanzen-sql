using finanzen_sql.utils;
using System.Windows;

namespace finanzen_sql.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DatabaseUtils _dbUtils;
        public MainWindow(DatabaseUtils databaseUtils)
        {
            InitializeComponent();
            _dbUtils = databaseUtils;
        }
    private void LoginClick(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new(_dbUtils)
            {
                // set window in the center
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            login.Show();

            this.Close();
        }

        private void RegistrationClick(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registration = new(_dbUtils)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            registration.Show();

            this.Close();
        }
    }
}