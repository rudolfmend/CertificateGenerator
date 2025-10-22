using System;
using System.Data.SQLite;
using System.IO;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Správca databázy pre CertificateGenerator
    /// </summary>
    public class DatabaseManager
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public DatabaseManager()
        {
            // Uloženie databázy v AppData\Local\CertificateGenerator
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "CertificateGenerator");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _databasePath = Path.Combine(appFolder, "CertificateGenerator.db");
            _connectionString = $"Data Source={_databasePath};Version=3;";

            InitializeDatabase();
        }

        /// <summary>
        /// Inicializácia databázy a vytvorenie tabuliek
        /// </summary>
        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Vytvorenie tabuľky Participants
                string createParticipantsTable = @"
                    CREATE TABLE IF NOT EXISTS Participants (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        BirthDate TEXT,
                        RegistrationNumber TEXT,
                        Notes TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT,
                        UsageCount INTEGER DEFAULT 0,
                        LastUsed TEXT
                    )";

                // Vytvorenie tabuľky Organizers
                string createOrganizersTable = @"
                    CREATE TABLE IF NOT EXISTS Organizers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT,
                        UsageCount INTEGER DEFAULT 0,
                        LastUsed TEXT
                    )";

                // Vytvorenie tabuľky EventTopics
                string createEventTopicsTable = @"
                    CREATE TABLE IF NOT EXISTS EventTopics (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Topic TEXT NOT NULL,
                        Description TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT,
                        UsageCount INTEGER DEFAULT 0,
                        LastUsed TEXT
                    )";

                // Vytvorenie tabuľky Certificates (história)
                string createCertificatesTable = @"
                    CREATE TABLE IF NOT EXISTS Certificates (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrganizerId INTEGER NOT NULL,
                        OrganizerName TEXT NOT NULL,
                        ParticipantId INTEGER NOT NULL,
                        ParticipantName TEXT NOT NULL,
                        ParticipantBirthDate TEXT,
                        ParticipantRegistrationNumber TEXT,
                        EventTopicId INTEGER NOT NULL,
                        EventTopicName TEXT NOT NULL,
                        EventDate TEXT,
                        Notes TEXT,
                        PaperFormat TEXT,
                        FilePath TEXT,
                        CreatedAt TEXT NOT NULL
                    )";

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = createParticipantsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createOrganizersTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createEventTopicsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createCertificatesTable;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Získanie connection stringu
        /// </summary>
        public string GetConnectionString()
        {
            return _connectionString;
        }

        /// <summary>
        /// Získanie cesty k databáze
        /// </summary>
        public string GetDatabasePath()
        {
            return _databasePath;
        }

        /// <summary>
        /// Zálohuje databázu
        /// </summary>
        public void BackupDatabase(string backupPath)
        {
            File.Copy(_databasePath, backupPath, true);
        }

        /// <summary>
        /// Obnoví databázu zo zálohy
        /// </summary>
        public void RestoreDatabase(string backupPath)
        {
            File.Copy(backupPath, _databasePath, true);
        }
    }
}
