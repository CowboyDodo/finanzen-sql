namespace finanzen_sql.Tables;

public class TransactionCategorySum
{
    public required decimal SumAmount { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
}
