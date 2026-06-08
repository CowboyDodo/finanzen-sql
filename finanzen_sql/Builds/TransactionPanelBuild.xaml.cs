using finanzen_sql.Tables;
using finanzen_sql.utils;
using finanzen_sql.Windows;
using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace finanzen_sql.Builds;

public class TransactionFilter
{
    public int? CategoryId { get; set; }
    public string TypeFilter { get; set; } = "";
    public required string IsRecurring { get; set; } = "";
}

public partial class TransactionPanelBuild : UserControl
{
    private DatabaseUtils _dbutils = new();
    // userID is set externally in FinanzDodoWindow
    public User? user { get; set; }
    private int _currentPage = 1;
    private int _pageSize = 7;
    public TransactionPanelBuild()
    {
        InitializeComponent();
    }

    // displays transaction based on the given filter if any
    public void GetTransactions(object sender, RoutedEventArgs e)
    {
        TransactionFilter filters = SetFilters();
        int offset = (_currentPage - 1) * _pageSize;
        using MySqlConnection conn = _dbutils.CreateConnection();

        List<Transaction> allTransactions = _dbutils.GetTransactions(
            conn,
            user!.Id,
            _pageSize,
            offset,
            filters.CategoryId,
            filters.TypeFilter,
            filters.IsRecurring
        );

        DataGridTransaction.ItemsSource = allTransactions;
    }

    private void EditEntryClick(object sender, RoutedEventArgs e)
    {
        // check if the sender is a button and if the datacontext of the button is a datarowview, if not we return
        // if yes we save the datarowview in a variable, because we need it to get the transaction id
        if (sender is not Button btn || btn.DataContext is not Transaction transaction)
            return;

        EditTransactionWindow editWindow = new(transaction);

        editWindow.ShowDialog();

        GetTransactions(sender, e);

    }

    // message for user to make sure the deleltion is on purpose
    private void DeleteEntryClick(object sender, RoutedEventArgs e)
    {
        MessageBoxResult userChoice = MessageBox.Show(
            "Bist du sicher, den Eintrag löschen zu wollen?",
            "Löschen bestätigen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (userChoice != MessageBoxResult.Yes)
            return;

        using MySqlConnection conn = _dbutils.CreateConnection();

        // we get the transaction id from the button tag, which is set in the xaml
        if (sender is not Button btn)
            return;

        int transactionID = Convert.ToInt32(btn.Tag);
        _dbutils.DeleteTransaction(conn, user!.Id, transactionID);

        GetTransactions(sender, e);
    }

    /// calculating pagination
    private int CalculateTotalPages()
    {
        TransactionFilter filters = SetFilters();

        using MySqlConnection conn = _dbutils.CreateConnection();

        int totalRows = _dbutils.GetTransactionCount(conn, user!.Id, filters.CategoryId, filters.TypeFilter, filters.IsRecurring);
        int totalPages = (int)Math.Ceiling((double)totalRows / _pageSize);

        return totalPages;
    }

    // button references
    private void BtnPreviousClick(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            GetTransactions(sender, e);
        }
    }
    private void BtnNextClick(object sender, RoutedEventArgs e)
    {
        if (_currentPage < CalculateTotalPages())
        {
            _currentPage++;
            GetTransactions(sender, e);
        }
    }

    // filter DropDwonOpened references
    private void CategoryFilter_DropDownOpened(object sender, EventArgs e)
    {
        using MySqlConnection conn = _dbutils.CreateConnection();

        List<Category> allCategories = _dbutils.GetCategories(conn);

        CmbCategoryFilter.ItemsSource = allCategories;
        CmbCategoryFilter.DisplayMemberPath = "Name";
        CmbCategoryFilter.SelectedValuePath = "Id";
    }

    private void TypeFilter_DropDownOpened(object sender, EventArgs e)
    {
        List<string> types = ["Einnahme", "Ausgabe"];

        CmbTypeFilter.ItemsSource = types;
    }

    private void CmbRecurringFilter_DropDownOpened(object sender, EventArgs e)
    {
        List<string> types = ["ja", "nein"];

        CmbRecurringFilter.ItemsSource = types;
    }

    // filter DropDwonClosed reference
    private void CmbFilter_DropDownClosed(object sender, EventArgs e)
    {
        GetTransactions(sender, new RoutedEventArgs());
    }

    // clears all the filter
    private void ClearFiltersClick(object sender, RoutedEventArgs e)
    {
        CmbCategoryFilter.SelectedIndex = -1;
        CmbCategoryFilter.SelectedItem = null;

        CmbTypeFilter.Text = "";
        CmbRecurringFilter.Text = "";

        _currentPage = 1;

        GetTransactions(sender, e);
    }

    private TransactionFilter SetFilters()
    {
        return new TransactionFilter
        {
            CategoryId = CmbCategoryFilter.SelectedValue == null ? null : (int)CmbCategoryFilter.SelectedValue,
            TypeFilter = CmbTypeFilter.Text,
            IsRecurring = CmbRecurringFilter.Text
        };
    }
}
