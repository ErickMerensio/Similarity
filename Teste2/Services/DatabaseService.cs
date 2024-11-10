using Microsoft.Data.Sqlite;

public class DatabaseService
{
    private static readonly Lazy<DatabaseService> _instance = new Lazy<DatabaseService>(() => new DatabaseService());
    private readonly string dbPath;
    private readonly SqliteConnection connection;

    private readonly string[] sampleImages =
    {
        "Resources/Images/fingerprint1.jpeg",
        "Resources/Images/fingerprint2.png",
        "Resources/Images/fingerprint3.png"
    };

    private DatabaseService()
    {
        dbPath = Path.Combine(FileSystem.AppDataDirectory, "fingerprints.db");
        connection = new SqliteConnection($"Data Source={dbPath}");

        InitializeDatabase();
    }

    public static DatabaseService Instance => _instance.Value;

    private void InitializeDatabase()
    {
        try
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Fingerprints (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ImagePath TEXT NOT NULL,
                    Nome TEXT,
                    Cargo TEXT
                )";
            command.ExecuteNonQuery();

            command.CommandText = "SELECT COUNT(*) FROM Fingerprints";
            long count = (long)command.ExecuteScalar();

            if (count == 0)
            {
                AddSampleFingerprints();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao inicializar o banco de dados: {ex.Message}");
        }
        finally
        {
            connection.Close();
        }
    }

    private void AddSampleFingerprints()
    {
        try
        {
            connection.Open();

            foreach (var imagePath in sampleImages)
            {
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Fingerprints (ImagePath) VALUES ($imagePath)";
                command.Parameters.AddWithValue("$imagePath", imagePath);
                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao adicionar amostras: {ex.Message}");
        }
        finally
        {
            connection.Close();
        }
    }

    public void AddFingerprint(string imagePath, string nome, string cargo)
    {
        try
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Fingerprints (ImagePath, Nome, Cargo) VALUES ($imagePath, $nome, $cargo)";
            command.Parameters.AddWithValue("$imagePath", imagePath);
            command.Parameters.AddWithValue("$nome", nome);
            command.Parameters.AddWithValue("$cargo", cargo);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao adicionar impressão digital: {ex.Message}");
        }
        finally
        {
            connection.Close();
        }
    }

    public List<(string ImagePath, string Nome, string Cargo)> GetAllFingerprints()
    {
        List<(string ImagePath, string Nome, string Cargo)> fingerprints = new List<(string, string, string)>();

        try
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT ImagePath, Nome, Cargo FROM Fingerprints";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string imagePath = reader.GetString(0);
                    string nome = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty;
                    string cargo = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty;

                    fingerprints.Add((imagePath, nome, cargo));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao obter impressões digitais: {ex.Message}");
        }
        finally
        {
            connection.Close();
        }
        return fingerprints;
    }
}
