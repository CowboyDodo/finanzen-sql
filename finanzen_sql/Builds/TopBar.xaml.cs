using finanzen_sql.utils;
using finanzen_sql.Windows;
using System.Windows;
using System.Windows.Controls;

namespace finanzen_sql.Builds;

public partial class TopBar : UserControl
{
    private readonly DatabaseUtils dbUtils = new();
    public TopBar()
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
}