using finanzen_sql.utils;
using finanzen_sql.Tables;
using System.Data;
using System.Windows;
using MySqlConnector;

namespace finanzen_sql.Windows;

/// <summary>
/// Interaktionslogik für finanzdodo.xaml
/// </summary>
public partial class FinanzDodoWindow : Window
{
    private readonly DatabaseUtils _dbUtils;
    private readonly User _user;
    public FinanzDodoWindow(DatabaseUtils databaseUtils, User userClass)
    {
        InitializeComponent();
        _dbUtils = databaseUtils;
        _user = userClass;

        // set userID in the PanelBuilds
        BuildAddPanel.user = _user;
        BuildTransactionPanel.user = _user;
    }
    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }

    private void RouteToHomepageClick(object sender, RoutedEventArgs e)
    {
        MainWindow homepage = new(_dbUtils)
        {
            // fix window in the center
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        homepage.Show();

        this.CloseClick(sender, e);
    }

    private void OverviewClick(object sender, RoutedEventArgs e)
    {
        OverviewPanel.Visibility = Visibility.Visible;
        TransactionPanel.Visibility = Visibility.Collapsed;
        AddPanel.Visibility = Visibility.Collapsed;
    }

    private void TransactionsClick(object sender, RoutedEventArgs e)
    {
        OverviewPanel.Visibility = Visibility.Collapsed;
        TransactionPanel.Visibility = Visibility.Visible;
        AddPanel.Visibility = Visibility.Collapsed;

        BuildTransactionPanel.GetTransactions(sender, e);
    }

    private void AddClick(object sender, RoutedEventArgs e)
    {
        AddPanel.Visibility = Visibility.Visible;
        TransactionPanel.Visibility = Visibility.Collapsed;
        OverviewPanel.Visibility = Visibility.Collapsed;
    }
}
