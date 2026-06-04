using finanzen_sql.utils;
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
    private readonly int _userID;
    public FinanzDodoWindow(DatabaseUtils databaseUtils, int userIDentification)
    {
        InitializeComponent();
        _dbUtils = databaseUtils;
        _userID = userIDentification;

        // set userID in the AddPanelBuild and TransactionPanelBuild
        BuildAddPanel.userID = _userID;
        BuildPanelTransaction.userID = _userID;
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

        BuildPanelTransaction.GetTransactions(sender, e);
    }

    private void AddClick(object sender, RoutedEventArgs e)
    {
        AddPanel.Visibility = Visibility.Visible;
        TransactionPanel.Visibility = Visibility.Collapsed;
        OverviewPanel.Visibility = Visibility.Collapsed;
    }
}
