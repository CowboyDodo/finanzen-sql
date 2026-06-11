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
    private readonly DatabaseUtils _dbUtils = new();
    // userID is set in the FinanzDodoWindow when the user logs in, so we can use it to push transactions for the correct user
    public User? user { get; set; }
    public AddPanelBuild()
    {
        InitializeComponent();
    }

    private void SendNewEntry(object sender, RoutedEventArgs e)
    {
        using MySqlConnection conn = _dbUtils.CreateConnection();

        if (!decimal.TryParse(TxtAmount.Text, out decimal amount))
        {
            MessageBox.Show("Bitte gebe einen gültigen Betrag ein.");
            return;
        };

        bool isIncome = CbIsIncome.IsChecked == true;

        if (TxtDescription.Text.Length > 32)
        {
            MessageBox.Show("Die Notiz überschreitet die max Länge von 32 Zeichen");
            return;
        }

        // check if amount is more than the user's budget
        if (!isIncome)
        {
            if (!CheckMonthlyUserBudget(amount))
                return;
        }

        // user is allowed to enter negative & postive numbers
        // if number is positive and type is expense make it negative else positive
        if (!isIncome)
            amount = -Math.Abs(amount);
        else
            amount = Math.Abs(amount);

        // set default category if user didn't choose a category
        int categoryID;
        if (isIncome && CmbCategory.SelectedValue == null)
            categoryID = _dbUtils.GetDefaultTypeIDCategory(conn, "Einnahme");
        else if (!isIncome && CmbCategory.SelectedValue == null)
            categoryID = _dbUtils.GetDefaultTypeIDCategory(conn, "Ausgabe");
        else
            categoryID = (int)CmbCategory.SelectedValue;

        Transaction transaction = new()
        {
            UserID = user!.Id,
            Amount = amount,
            Description = TxtDescription.Text,
            // if the user not selects a category, we set it to 0, otherwise we set it to the selected value of the combobox
            CategoryID = categoryID,
            IsRecurring = CbIsRecurring.IsChecked == true,
            Date = DpDate.SelectedDate ?? DateTime.Now
        };

        bool isSaved = _dbUtils.PushTransaction(conn, transaction);

        if (isSaved)
        {
            ClearFields(sender, e);
        }
    }

    private void ShowCategoryNames(object sender, EventArgs e)
    {
        bool isIncome = CbIsIncome.IsChecked == true;

        using MySqlConnection conn = _dbUtils.CreateConnection();

        List<Category> allCategories = _dbUtils.GetCategories(conn);

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

    // if IsIncome Checkbox is clicked, remove chosen category
    private void ClearCategory(object sender, RoutedEventArgs e)
    {
        CmbCategory.SelectedIndex = -1;
    }

    private bool CheckMonthlyUserBudget(decimal amountToSend)
    {
        DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        DateTime monthEnd = monthStart.AddMonths(1);

        using MySqlConnection conn = _dbUtils.CreateConnection();

        decimal monthlyExpenses = _dbUtils.GetMonthlyUserExpenses(
            conn,
            user!,
            monthStart, 
            monthEnd
        );
        
        // check if user has set a budget
        if (user!.Budget == 0)
            return true;

        if ((Math.Abs(monthlyExpenses) + amountToSend) <= user!.Budget)
            return true;

        MessageBoxResult userChoice = MessageBox.Show(
            "Achtung, du überschreitest dein Budget",
            "Fortfahren?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (userChoice == MessageBoxResult.Yes)
            return true;

        return false;
    }
}
