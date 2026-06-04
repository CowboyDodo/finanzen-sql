using finanzen_sql.utils;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;


namespace finanzen_sql.Builds;

/// <summary>
/// Interaktionslogik für AddPanelUtils.xaml
/// </summary>
public partial class AddPanelBuild : UserControl
{
    private readonly DatabaseUtils dbUtils = new();
    // userID is set in the FinanzDodoWindow when the user logs in, so we can use it to push transactions for the correct user
    public int userID { get; set; }
    public AddPanelBuild()
    {
        InitializeComponent();
    }

    private void SendNewEntry(object sender, RoutedEventArgs e)
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

        using MySqlConnection conn = dbUtils.CreateConnection();
        bool isSaved = dbUtils.PushTransaction(conn, userID, amount, description, isRecurring, selectedCategory, date);

        if (isSaved)
        {
            ClearFields(sender, e);
        }
    }

    private void ShowCategoryNames(object sender, EventArgs e)
    {
        bool isIncome = CbIsIncome.IsChecked == true;

        using MySqlConnection conn = dbUtils.CreateConnection();

        List<Category> allCategories = dbUtils.GetCategories(conn);

        // Filter categories based on the type (income or expense) and set the ItemsSource of the ComboBox
        CmbCategory.ItemsSource = allCategories
            .Where(c => c.typ == (isIncome ? "Einnahme" : "Ausgabe"))
            .ToList();

        CmbCategory.DisplayMemberPath = "name";
        CmbCategory.SelectedValuePath = "id";
    }

    private void ClearFields(object sender, RoutedEventArgs e)
    {
        TxtAmount.Clear();
        TxtDescription.Clear();
        CbIsIncome.IsChecked = false;
        CbIsRecurring.IsChecked = false;
        CmbCategory.SelectedIndex = -1;
        DpDate.SelectedDate = null;
    }
}
