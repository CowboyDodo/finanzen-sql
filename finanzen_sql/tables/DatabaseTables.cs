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
                name VARCHAR(64) NOT NULL UNIQUE,
                typ ENUM('Einnahme', 'Ausgabe'),
                PRIMARY KEY (id)
            );
        ";
    }

    public string CreateTableTransaction()
    {
        return @"
            CREATE TABLE IF NOT EXISTS Transaktion (
                id INT NOT NULL AUTO_INCREMENT,
                betrag DECIMAL(10,2) NOT NULL,
                beschreibung VARCHAR(32),
                istWiederkehrend BOOLEAN DEFAULT false,
                user_id INT NOT NULL,
                kategorie_id INT,
                datum DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
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
                budget DECIMAL DEFAULT 0,
                PRIMARY KEY (id)
            );
        ";
    }

    public string CreateCategories()
    {
        return @"
            INSERT IGNORE INTO kategorie (name, typ)
            VALUES
                ('Transport', 'Ausgabe'),
                ('Freizeit', 'Ausgabe'),
                ('Miete', 'Ausgabe'),
                ('Versicherungen', 'Ausgabe'),
                ('Lebensmittel', 'Ausgabe'),
                ('Sonstiges (Ausgabe)', 'Ausgabe'),
                ('Gehalt', 'Einnahme'),
                ('Freelancing', 'Einnahme'),
                ('Investitionen', 'Einnahme'),
                ('Zinsen', 'Einnahme'),
                ('Verkauf', 'Einnahme'),
                ('Sonstiges (Einnahme)', 'Einnahme');
        ";
    }
}
