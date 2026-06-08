namespace finanzen_sql.Tables;

public class Transaction
{
    public int Id { get; set; }
    public required decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public bool IsRecurring { get; set; }
    public required int UserID {  get; set; }
    public int CategoryID { get; set; }
    public required DateTime Date {  get; set; }

    public string CategoryName { get; set; } = "Keine Angabe";
    public string CategoryType { get; set; } = "-";
}
