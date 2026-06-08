using finanzen_sql.Tables;
using finanzen_sql.utils;
using MySqlConnector;
using System.Windows;
using System.Diagnostics;


namespace finanzen_sql.Windows;

/// <summary>
/// Interaktionslogik für EditWindow.xaml
/// </summary>
public partial class EditTransactionWindow : Window
{
    private DatabaseUtils _dbUtils = new();
    private List<Category> _allCategories = new();
    private readonly Transaction _transaction;
    public EditTransactionWindow(Transaction transactionToEdit)
    {
        InitializeComponent();

        LoadCategories();

        _transaction = transactionToEdit;

        TxtAmount.Text = _transaction.Amount.ToString();
        TxtDescription.Text = _transaction.Description.ToString();
        DpDate.SelectedDate = _transaction.Date;

        CbIsIncome.IsChecked = _transaction.CategoryType == "Einnahme";
        CbIsRecurring.IsChecked = _transaction.IsRecurring;

        UpdateCategoryCombo(_transaction);
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(TxtAmount.Text, out decimal amount))
        {
            MessageBox.Show("Bitte gebe einen gültigen Betrag ein.");
            return;
        }
        ;

        bool isIncome = CbIsIncome.IsChecked == true;

        if (isIncome && amount < 0)
        {
            MessageBox.Show("Der Betrag muss positiv sein, wenn es sich um eine Einnahme handelt.");
            return;
        }
        if (isIncome == false && amount > 0)
        {
            MessageBox.Show("Der Betrag muss negativ sein, wenn es sich um eine Ausgabe handelt.");
            return;
        }

        Transaction editedTransaction = new()
        {
            Id = _transaction.Id,
            UserID = _transaction.UserID,
            Amount = amount,
            Description = TxtDescription.Text,
            CategoryID = CmbCategory.SelectedValue == null ? 0 : (int)CmbCategory.SelectedValue,
            IsRecurring = CbIsRecurring.IsChecked == true,
            Date = DpDate.SelectedDate ?? DateTime.Now
        };

        using MySqlConnection conn = _dbUtils.CreateConnection();
        bool isEdited = _dbUtils.EditTransaction(conn, editedTransaction);

        if (isEdited)
        {
            this.Close();
        }
    }

    private void UpdateCategoryCombo(Transaction transaction)
    {
        bool isIncome = CbIsIncome.IsChecked == true;

        // Filter categories based on the type (income or expense) and set the ItemsSource of the ComboBox
        CmbCategory.ItemsSource = _allCategories
            .Where(c => c.Type == (isIncome ? "Einnahme" : "Ausgabe"))
            .ToList();

        CmbCategory.DisplayMemberPath = "Name";
        CmbCategory.SelectedValuePath = "Id";

        CmbCategory.SelectedValue = transaction.CategoryID;
    }

    private void ShowCategoryNames(object sender, EventArgs e)
    {
        UpdateCategoryCombo(_transaction);
    }

    private void LoadCategories()
    {
        using MySqlConnection conn = _dbUtils.CreateConnection();
        _allCategories = _dbUtils.GetCategories(conn);
    }
}
