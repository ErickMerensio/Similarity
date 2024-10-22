using Microsoft.Data.Sqlite;
using System.IO;

public class DatabaseService
{
    private readonly string dbPath;
    private readonly string[] sampleImages =
    {
        "Resources/Images/fingerprint1.jpeg",
        "Resources/Images/fingerprint2.png",
        "Resources/Images/fingerprint3.png"
    };

    public DatabaseService()
    {
        dbPath = Path.Combine(FileSystem.AppDataDirectory, "fingerprints.db");
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // Cria a tabela Fingerprints
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Fingerprints (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ImagePath TEXT NOT NULL
                )";
            command.ExecuteNonQuery();

            // Verifica se já existem impressões digitais no banco
            command.CommandText = "SELECT COUNT(*) FROM Fingerprints";
            long count = (long)command.ExecuteScalar();

            if (count == 0)
            {
                // Se não houver registros, insere as imagens de exemplo
                AddSampleFingerprints();
            }
        }
    }

    private void AddSampleFingerprints()
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
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
    }

    // Método para adicionar impressões digitais no banco de dados
    public void AddFingerprint(string imagePath)
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Fingerprints (ImagePath) VALUES ($imagePath)";
            command.Parameters.AddWithValue("$imagePath", imagePath);
            command.ExecuteNonQuery();
        }
    }

    // Método para obter todas as impressões digitais do banco de dados
    public List<string> GetAllFingerprints()
    {
        List<string> fingerprints = new List<string>();

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT ImagePath FROM Fingerprints";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    fingerprints.Add(reader.GetString(0));
                }
            }
        }

        return fingerprints;
    }
}
