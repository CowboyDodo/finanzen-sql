namespace finanzen_sql.classes;

/// <summary>
/// SQL queries for table creations
/// </summary>
public class DatabaseTables
{
    public string CreateTableCategory()
    {
        return @"
            CREATE TABLE IF NOT EXISTS Category (
                id INT NOT NULL AUTO_INCREMENT,
                name VARCHAR(64) NOT NULL,
                PRIMARY KEY (id)
            );
        ";
    }

    public string CreateTableRevenue()
    {
        return @"
            CREATE TABLE IF NOT EXISTS Revenue (
                id INT NOT NULL AUTO_INCREMENT,
                amount DECIMAL NOT NULL,
                categoryId INT,
                timeAdded DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (id),
                CONSTRAINT revenue_category
                    FOREIGN KEY (categoryId)
                    REFERENCES Category(id)
            );
        ";
    }

    public string CreateTableSpending()
    {
        return @"
            CREATE TABLE IF NOT EXISTS Spending (
                id INT NOT NULL AUTO_INCREMENT,
                amount DECIMAL NOT NULL,
                recipient VARCHAR(64) NOT NULL,
                categoryId INT,
                timeAdded DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (id),
                CONSTRAINT spending_category
                    FOREIGN KEY (categoryId)
                    REFERENCES Category(id)
            );
        ";
    }
}