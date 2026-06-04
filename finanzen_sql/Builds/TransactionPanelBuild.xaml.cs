using finanzen_sql.utils;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;
using finanzen_sql.Windows;


namespace finanzen_sql.Builds;

/// <summary>
/// Interaktionslogik für TransactionPanelBuild.xaml
/// </summary>
public partial class TransactionPanelBuild : UserControl
{
    private DatabaseUtils _dbutils = new();
    // userID is set externally in FinanzDodoWindow
    public int userID { get; set; }
    private int _currentPage = 1;
    private int _pageSize = 7;
    public TransactionPanelBuild()
    {
        InitializeComponent();
    }

    public void GetTransactions(object sender, RoutedEventArgs e)
    {
        int? categoryFilter = CmbCategoryFilter.SelectedValue == null ? null : (int)CmbCategoryFilter.SelectedValue;
        string typFilter = CmbTypeFilter.Text;

        int offset = (_currentPage - 1) * _pageSize;
        using MySqlConnection conn = _dbutils.CreateConnection();

        DataTable table = _dbutils.GetTransactions(conn, userID, _pageSize, offset, categoryFilter, typFilter);

        DataGridTransaction.ItemsSource = table.DefaultView;

        // we need to wait until the datagrid is loaded, otherwise we get an error
        // because the columns are not generated yet
        // necessary so we can hide the category id column
        DataGridTransaction.UpdateLayout();
        DataGridTransaction.Columns[1].Visibility = Visibility.Collapsed;
    }

    private int CalculateTotalPages()
    {
        using MySqlConnection conn = _dbutils.CreateConnection();

        int totalRows = _dbutils.GetTransactionCount(conn, userID);
        int totalPages = (int)Math.Ceiling((double)totalRows / _pageSize);

        return totalPages;
    }

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

    private void CategoryFilter_DropDownOpened(object sender, EventArgs e)
    {
        using MySqlConnection conn = _dbutils.CreateConnection();

        List<Category> allCategories = _dbutils.GetCategories(conn);

        CmbCategoryFilter.ItemsSource = allCategories;
        CmbCategoryFilter.DisplayMemberPath = "name";
        CmbCategoryFilter.SelectedValuePath = "id";
    }

    private void TypeFilter_DropDownOpened(object sender, EventArgs e)
    {
        List<string> types = ["Einnahme", "Ausgabe"];

        CmbTypeFilter.ItemsSource = types;
    }

    private void CmbTypeFilter_DropDownClosed(object sender, EventArgs e)
    {
        GetTransactions(sender, new RoutedEventArgs());

    }

    private void CmbCategoryFilter_DropDownClosed(object sender, EventArgs e)
    {
        GetTransactions(sender, new RoutedEventArgs());

    }

    private void ClearFiltersClick(object sender, RoutedEventArgs e)
    {
        CmbCategoryFilter.SelectedIndex = -1;
        CmbCategoryFilter.SelectedItem = null;

        CmbTypeFilter.Text = "";

        _currentPage = 1;

        GetTransactions(sender, e);
    }

    private void EditEntryClick(object sender, RoutedEventArgs e)
    {
        // check if the sender is a button and if the datacontext of the button is a datarowview, if not we return
        // if yes we save the datarowview in a variable, because we need it to get the transaction id
        if (sender is not Button btn || btn.DataContext is not DataRowView row)
            return;

        int transactionID = Convert.ToInt32(row["Id"]);
        decimal amount = Convert.ToDecimal(row["Betrag"]);
        string? description = row["Beschreibung"].ToString();
        string? category = row["Kategorie"].ToString();
        string? type = row["Typ"].ToString();
        DateTime date = Convert.ToDateTime(row["Datum"]);

        if (type == "-")
        {
            type = "";

        }
        if (category == "Keine Angabe")
        {
            category = "";
        }

        EditTransactionWindow editWindow = new(
            transactionID,
            userID,
            amount,
            description,
            category,
            type,
            date
        );

        editWindow.ShowDialog();

        GetTransactions(sender, e);
    }

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
         _dbutils.DeleteTransaction(conn, userID, transactionID);

         GetTransactions(sender, e);   
    }
}
