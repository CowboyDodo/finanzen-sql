using finanzen_sql.Tables;
using finanzen_sql.utils;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;


namespace finanzen_sql.Builds;

/// <summary>
/// Interaktionslogik für AddPanelUtils.xaml
/// </summary>
public partial class AddPanelBuild : UserControl
{
    private readonly DatabaseUtils dbUtils = new();
    // userID is set in the FinanzDodoWindow when the user logs in, so we can use it to push transactions for the correct user
    public User? user { get; set; }
    public AddPanelBuild()
    {
        InitializeComponent();
    }

    private void SendNewEntry(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(TxtAmount.Text, out decimal amount))
        {
            MessageBox.Show("Bitte gebe einen gültigen Betrag ein.");
            return;
        };

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

        Transaction transaction = new()
        {
            UserID = user!.Id,
            Amount = amount,
            Description = TxtDescription.Text,
            // if the user not selects a category, we set it to 0, otherwise we set it to the selected value of the combobox
            CategoryID = CmbCategory.SelectedValue == null ? 0 : (int)CmbCategory.SelectedValue,
            IsRecurring = CbIsRecurring.IsChecked == true,
            Date = DpDate.SelectedDate ?? DateTime.Now
        };

        using MySqlConnection conn = dbUtils.CreateConnection();
        bool isSaved = dbUtils.PushTransaction(conn, transaction);

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
            .Where(c => c.Type == (isIncome ? "Einnahme" : "Ausgabe"))
            .ToList();

        CmbCategory.DisplayMemberPath = "Name";
        CmbCategory.SelectedValuePath = "Id";
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
