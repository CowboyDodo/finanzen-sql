using finanzen_sql.Tables;
using finanzen_sql.utils;
using MySqlConnector;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace finanzen_sql.Builds;

/// <summary>
/// Interaction logic for OverviewPanelBuild.xaml
/// </summary>
public partial class OverviewPanelBuild : UserControl
{
    private DatabaseUtils _dbUtils = new();
    public User? user { get; set; }
    public OverviewPanelBuild()
    {
        InitializeComponent();
        IsVisibleChanged += ShowDatatWhenVisibleChanged;

        DpExpense.SelectedDate = DateTime.Now;
        DpIncome.SelectedDate = DateTime.Now;
        CbExpenseInterval.SelectedIndex = 4;
        CbIncomeInterval.SelectedIndex = 4;
    }

    public void LoadData()
    {
        ShowCurrentBudget();
        ShowCurrentAccountBalance();
        ShowAvailableBudget();
        ShowExpenses();
        ShowIncomes();
    }

    private void ShowCurrentBudget()
    {
        TxtCurrentBudget.Text = user!.Budget == 0
            ? "unbegrenzt"
            : user.Budget.ToString();
    }

    private void ShowAvailableBudget()
    {
        decimal availableBudget = CalculateAvailableBudget(user!);
        TxtAvailableBudget.Text = user!.Budget == 0
            ? "unbegrenzt"
            : availableBudget.ToString();

        TxtAvailableBudget.Foreground = user!.Budget == 0 || availableBudget >= 0
            ? Brushes.ForestGreen
            : Brushes.Red;
    }

    public void ShowExpenses(object? sender = null, EventArgs? e = null)
    {
        List<TransactionCategorySum> transactionByCat = CalculateTransactions("Ausgabe");

        Dictionary<string, decimal> expenseDict = transactionByCat
            .Where(c => c.Type == "Ausgabe")
            .ToDictionary(c => c.Name, c => c.SumAmount);

        TxtExpenseTransport.Text = expenseDict.GetValueOrDefault("Transport").ToString("N2");
        TxtExpenseFreetime.Text = expenseDict.GetValueOrDefault("Freizeit").ToString("N2");
        TxtExpenseRent.Text = expenseDict.GetValueOrDefault("Miete").ToString("N2");
        TxtExpenseInsurance.Text = expenseDict.GetValueOrDefault("Versicherungen").ToString("N2");
        TxtExpenseGroceries.Text = expenseDict.GetValueOrDefault("Lebensmittel").ToString("N2");
        TxtExpenseOthers.Text = expenseDict.GetValueOrDefault("Sonstiges (Ausgabe)").ToString("N2");
    }

    public void ShowIncomes(object? sender = null, EventArgs? e = null)
    {
        List<TransactionCategorySum> transactionByCat = CalculateTransactions("Einnahme");

        Dictionary<string, decimal> expenseDict = transactionByCat
            .Where(c => c.Type == "Einnahme")
            .ToDictionary(c => c.Name, c => c.SumAmount);

        TxtIncomeSalary.Text = expenseDict.GetValueOrDefault("Gehalt").ToString("N2");
        TxtIncomeFreelance.Text = expenseDict.GetValueOrDefault("Freelancing").ToString("N2");
        TxtIncomeInvestion.Text = expenseDict.GetValueOrDefault("Investitionen").ToString("N2");
        TxtIncomeInterest.Text = expenseDict.GetValueOrDefault("Zinsen").ToString("N2");
        TxtIncomeSell.Text = expenseDict.GetValueOrDefault("Verkauf").ToString("N2");
        TxtIncomeOthers.Text = expenseDict.GetValueOrDefault("Sonstiges (Einnahme)").ToString("N2");
    }

    public void ShowCurrentAccountBalance()
    {
        using MySqlConnection conn = _dbUtils.CreateConnection();

        TxtCurrentBalance.Text = _dbUtils.GetSumAmountTransactions(conn, user!.Id).ToString() + "€";
    }

    private void ShowDatatWhenVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            ShowCurrentAccountBalance();
            ShowAvailableBudget();
            ShowExpenses();
            ShowIncomes();
        }
    }

    private decimal CalculateAvailableBudget(User user)
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

        return Convert.ToDecimal(user!.Budget) - Math.Abs(monthlyExpenses);
    }

    private void TbBudget_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        if (!decimal.TryParse(TbBudget.Text, out decimal budget))
        {
            MessageBox.Show("Bitte einen gültigen Betrag eingeben.");
            return;
        }

        if (budget < 0)
            budget = Math.Abs(budget);

        user!.Budget = budget;

        using MySqlConnection conn = _dbUtils.CreateConnection();
        _dbUtils.PushUser(conn, user);

        ShowCurrentBudget();
        ShowAvailableBudget();

        TbBudget.Text = "";
    }

    private List<TransactionCategorySum> CalculateTransactions(string type)
    {
        using MySqlConnection conn = _dbUtils.CreateConnection();
        DateTime date;
        string selectedInterval;

        if (type == "Ausgabe")
        {
            date = DpExpense.SelectedDate!.Value;
            selectedInterval = (CbExpenseInterval.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        }
        else
        {
            date = DpIncome.SelectedDate!.Value;
            selectedInterval = (CbIncomeInterval.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        }

        if (selectedInterval == "Insgesamt")
            return _dbUtils.GetTransactionSumByCategory(conn, user!.Id);

        (DateTime start, DateTime end) = CalculateTimeInterval(date, selectedInterval);

        return _dbUtils.GetTransactionSumByCategory(conn, user!.Id, start, end);
    }

    private (DateTime start, DateTime end) CalculateTimeInterval(DateTime chosenDate, string selectedInterval)
    {
        DateTime start;
        DateTime end;
        int day = chosenDate.Day;
        int month = chosenDate.Month;
        int year = chosenDate.Year;

        if (selectedInterval == "Tag")
        {
            start = new DateTime(year, month, day);
            end = start.AddDays(1);
        }
        else if (selectedInterval == "Woche")
        {
            int weekDaydiff = (7 + (chosenDate.DayOfWeek - DayOfWeek.Monday)) % 7;

            DateTime weekStart = chosenDate.AddDays(-weekDaydiff);
            DateTime weekEnd = weekStart.AddDays(7);

            start = new DateTime(year, month, weekStart.Day);
            end = start.AddDays(weekEnd.Day);

        }
        else if (selectedInterval == "Monat")
        {
            start = new DateTime(year, month, 1);
            end = start.AddMonths(1);

        }
        else
        {
            start = new DateTime(year, 1, 1);
            end = start.AddYears(1);

        }

        return (start, end);
    }
}