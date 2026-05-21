namespace finanzen_sql.classes;

/// <summary>
/// SQL queries for table creations
/// </summary>
public class DatabaseTables
{
    public string CreateTableCategory()
    {
        return @"
            CREATE TABLE IF NOT EXISTS Kategorie (
                id INT NOT NULL AUTO_INCREMENT,
                name VARCHAR(64) NOT NULL,
                typ ENUM('Einname', 'Ausgabe'),
                PRIMARY KEY (id)
            );
        ";
    }

    public string CreateTableTransaction()
    {
        return @"
            CREATE TABLE IF NOT EXISTS Transaktion (
                id INT NOT NULL AUTO_INCREMENT,
                betrag DECIMAL NOT NULL,
                beschreibung VARCHAR(255),
                istWiederkehrend bool DEFAULT false,
                user_id INT NOT NULL,
                kategorie_id INT,
                datum DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (id),
                CONSTRAINT transaktion_kategorie
                    FOREIGN KEY (kategorie_id)
                    REFERENCES Kategorie(id),
                CONSTRAINT transaktion_user
                    FOREIGN KEY (user_id)
                    REFERENCES User(id)
            );
        ";
    }

    public string CreateTableUser()
    {
        return @"
            CREATE TABLE IF NOT EXISTS User (
                id INT NOT NULL AUTO_INCREMENT,
                username VARCHAR(64) NOT NULL,
                passwort VARCHAR(255) NOT NULL,
                PRIMARY KEY (id)
            );
        ";
    }
}
