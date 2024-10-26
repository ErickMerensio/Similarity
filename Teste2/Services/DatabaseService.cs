using Microsoft.Data.Sqlite;
using System.IO;
using static OpenCvSharp.ML.DTrees;

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
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }

            connection.Open();

            // Cria a tabela Fingerprints
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Fingerprints (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ImagePath TEXT NOT NULL,
                    [Nome] TEXT,
                    [Cargo] TEXT
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
    public void AddFingerprint(string imagePath, string nome, string cargo)
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Fingerprints (ImagePath, Nome, Cargo) VALUES ($imagePath, $nome, $cargo)";
            command.Parameters.AddWithValue("$imagePath", imagePath);
            command.Parameters.AddWithValue("$nome", nome);
            command.Parameters.AddWithValue("$cargo", cargo);
            command.ExecuteNonQuery();
        }
    }

    // Método para obter todas as impressões digitais do banco de dados
    public List<(string ImagePath, string Nome, string Cargo)> GetAllFingerprints()
    {
        List<(string ImagePath, string Nome, string Cargo)> fingerprints = new List<(string, string, string)>();

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT ImagePath, Nome, Cargo FROM Fingerprints";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string imagePath = reader.GetString(0);

                    // Verificar se os valores são NULL antes de obter Nome e Cargo
                    string nome = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty;
                    string cargo = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty;

                    fingerprints.Add((imagePath, nome, cargo));
                }
            }
        }

        return fingerprints;
    }

}
