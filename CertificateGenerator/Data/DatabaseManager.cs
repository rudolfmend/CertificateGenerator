using System;
using System.Diagnostics;
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
                Debug.WriteLine("tabuľka sa vytvorila");

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

                string createCertificateTemplatesTable = @"
                    CREATE TABLE IF NOT EXISTS CertificateTemplates (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        IsDefault INTEGER DEFAULT 0,
                        TitleColor TEXT,
                        TextColor TEXT,
                        AccentColor TEXT,
                        BackgroundColor TEXT,
                        TitleFontFamily TEXT,
                        TitleFontSize INTEGER,
                        HeaderFontFamily TEXT,
                        HeaderFontSize INTEGER,
                        TextFontFamily TEXT,
                        TextFontSize INTEGER,
                        MarginTop INTEGER,
                        MarginRight INTEGER,
                        MarginBottom INTEGER,
                        MarginLeft INTEGER,
                        TitleAlignment TEXT,
                        ShowSeparatorLine INTEGER,
                        SeparatorStyle TEXT,
                        LogoPath TEXT,
                        LogoPosition TEXT,
                        LogoWidth INTEGER,
                        LogoHeight INTEGER,
                        CertificateTitle TEXT,
                        ShowTitle INTEGER,
                        ShowOrganizer INTEGER,
                        ShowEventTopic INTEGER,
                        ShowEventDate INTEGER,
                        ShowBirthDate INTEGER,
                        ShowRegistrationNumber INTEGER,
                        ShowNotes INTEGER,
                        CustomHeaderText TEXT,
                        CustomFooterText TEXT,
                        MainContentText TEXT,
                        ShowMainContent INTEGER DEFAULT 1,
                        ShowBorder INTEGER,
                        BorderColor TEXT,
                        BorderWidth INTEGER,
                        LabelOrganizer TEXT,
                        LabelEventTopic TEXT,
                        LabelParticipant TEXT,
                        LabelEventDate TEXT,
                        LabelBirthDate TEXT,
                        LabelRegistrationNumber TEXT,
                        LabelNotes TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT
                    )";

                using (var command = new SQLiteCommand(connection))
                {
                    Debug.WriteLine("Command create tables.(SQLiteCommand)");
                    command.CommandText = createParticipantsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createOrganizersTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createEventTopicsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createCertificatesTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createCertificateTemplatesTable;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Získanie connection stringu
        /// </summary>
        public string GetConnectionString()
        {
            Debug.WriteLine("connectio string");
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
