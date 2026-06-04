using finanzen_sql.utils;
using finanzen_sql.Windows;
using System.Windows;
using System.Windows.Controls;

namespace finanzen_sql.Builds;

public partial class TopBarBuild : UserControl
{
    private readonly DatabaseUtils dbUtils = new();
    public TopBarBuild()
    {
        InitializeComponent();
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }

    private void RouteToHomepageClick(object sender, RoutedEventArgs e)
    {
        MainWindow homepage = new(dbUtils)
        {
            // fix window in the center
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        homepage.Show();

        this.CloseClick(sender, e);
    }

    /// <summary>
    /// Opens the LoginWindow and closes the current window when the user is not currently in a FinanzDodoWindow.
    /// </summary>
    /// <param name="sender">The UI element that triggered the mouse click event</param>
    /// <param name="e">Contains data associated with the mouse button event</param>
    private void CheckIfFinanzDodo(object sender, RoutedEventArgs e)
    {
        Window currentWindow = Window.GetWindow(this);

        if (currentWindow is not FinanzDodoWindow)
        {
            LoginWindow loginWindow = new(dbUtils)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            loginWindow.Show();

            this.CloseClick(sender, e);
        }
    }
}