using finanzen_sql.utils;
using System.Windows;
using MySqlConnector;


namespace finanzen_sql.Windows;

/// <summary>
/// Interaktionslogik für EditWindow.xaml
/// </summary>
public partial class EditTransactionWindow : Window
{
    private DatabaseUtils _dbUtils = new();
    private int _transactionID;
    private int _userID;
    public EditTransactionWindow(
        int transactionIdentifier,
        int userIdentifier,
        decimal amount,
        string? description,
        string? category,
        string? type,
        DateTime date
    )
    {
        InitializeComponent();

        _transactionID = transactionIdentifier;
        _userID = userIdentifier;

        TxtAmount.Text = amount.ToString();
        TxtDescription.Text = description;
        DpDate.SelectedDate = date;
        
        if (type == "Einnahme")
        {
            CbIsIncome.IsChecked = true;
        }
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        decimal amount;

        try
        {
            amount = decimal.Parse(TxtAmount.Text);
        }
        catch
        {
            MessageBox.Show("Bitte gebe einen gültigen Betrag ein.");
            return;
        }
        // check if its recurring, if the checkbox is checked, we set it to true, otherwise false
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

        string description = TxtDescription.Text;

        // if the user not selects a category, we set it to null, otherwise we set it to the selected value of the combobox
        int? selectedCategory = CmbCategory.SelectedValue == null ? null : (int)CmbCategory.SelectedValue;

        // check if its recurring, if the checkbox is checked, we set it to true, otherwise false
        bool isRecurring = CbIsRecurring.IsChecked == true;

        // default value for date is the current date, if the user not selects a date
        DateTime date = DpDate.SelectedDate ?? DateTime.Now;

        using MySqlConnection conn = _dbUtils.CreateConnection();
        
    }
    private void ShowCategoryNames(object sender, EventArgs e)
    {
        bool isIncome = CbIsIncome.IsChecked == true;

        using MySqlConnection conn = _dbUtils.CreateConnection();

        List<Category> allCategories = _dbUtils.GetCategories(conn);

        // Filter categories based on the type (income or expense) and set the ItemsSource of the ComboBox
        CmbCategory.ItemsSource = allCategories
            .Where(c => c.typ == (isIncome ? "Einnahme" : "Ausgabe"))
            .ToList();

        CmbCategory.DisplayMemberPath = "name";
        CmbCategory.SelectedValuePath = "id";
    }
}
