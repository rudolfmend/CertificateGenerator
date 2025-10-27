using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.IO.Compression;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Správca databázy pre CertificateGenerator s podporou viacerých prenositeľných SQLite databáz
    /// </summary>
    public class DatabaseManager
    {
        private string _connectionString;
        private string _currentDatabasePath;
        private readonly string _databaseFolder;
        private readonly string _machineIdentifier;

        public DatabaseManager()
        {
            // Priečinok pre databázy v Dokumentoch
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appFolder = Path.Combine(documentsPath, "CertificateGenerator");
            _databaseFolder = Path.Combine(appFolder, "Databases");

            bool isFirstRun = !Directory.Exists(_databaseFolder);

            if (!Directory.Exists(_databaseFolder))
            {
                Directory.CreateDirectory(_databaseFolder);
            }

            _machineIdentifier = GetMachineIdentifier();

            // Inicializácia - načítanie alebo vytvorenie databázy
            InitializeDatabaseSystem();

            // Zobraz uvítaciu správu pri prvom spustení
            if (isFirstRun)
            {
                ShowFirstRunMessage();
            }
        }

        /// <summary>
        /// Uvítacia správa pri prvom spustení
        /// </summary>
        private void ShowFirstRunMessage()
        {
            System.Windows.MessageBox.Show(
                $"Databázové súbory budú uložené v:\n{_databaseFolder}\n\n" +
                "Tento priečinok môžete kedykoľvek otvoriť cez menu Súbor → Otvoriť priečinok s databázami",
                "Umiestnenie databáz",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        /// <summary>
        /// Získanie unikátneho identifikátora počítača
        /// </summary>
        internal string GetMachineIdentifier()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key != null)
                    {
                        var guid = key.GetValue("MachineGuid")?.ToString();
                        if (!string.IsNullOrEmpty(guid))
                        {
                            return guid.Replace("-", "").Substring(0, 16);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba pri získavaní MachineGuid: {ex.Message}");
            }

            // Fallback - použiť uložený alebo vytvoriť nový trvalý GUID
            return GetOrCreatePersistentIdentifier();
        }

        /// <summary>
        /// Získanie alebo vytvorenie trvalého identifikátora (uložený v súbore)
        /// </summary>
        private string GetOrCreatePersistentIdentifier()
        {
            string identifierFile = Path.Combine(_databaseFolder, ".machine_id");

            try
            {
                // Ak súbor existuje, načítaj identifikátor
                if (File.Exists(identifierFile))
                {
                    string existingId = File.ReadAllText(identifierFile).Trim();
                    if (!string.IsNullOrEmpty(existingId) && existingId.Length == 16)
                    {
                        Debug.WriteLine($"Načítaný trvalý identifikátor: {existingId}");
                        return existingId;
                    }
                }

                // Vytvor nový identifikátor a ulož ho
                string newId = Guid.NewGuid().ToString("N").Substring(0, 16);
                File.WriteAllText(identifierFile, newId);
                Debug.WriteLine($"Vytvorený nový trvalý identifikátor: {newId}");
                return newId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba pri práci s trvalým identifikátorom: {ex.Message}");
                // Posledná záchrana - použiť environment username + machine name
                string fallbackId = $"{Environment.UserName}{Environment.MachineName}";
                return fallbackId.GetHashCode().ToString("X").PadRight(16, '0').Substring(0, 16);
            }
        }

        /// <summary>
        /// Inicializácia databázového systému
        /// </summary>
        private void InitializeDatabaseSystem()
        {
            var existingDatabases = GetAllDatabases();

            if (existingDatabases.Count == 0)
            {
                // Žiadna databáza neexistuje - vytvor novú
                CreateNewDatabase();
            }
            else
            {
                // Najdi databázu pre aktuálny počítač
                var localDatabase = existingDatabases
                    .FirstOrDefault(db => Path.GetFileName(db).Contains(_machineIdentifier));

                if (localDatabase != null)
                {
                    // Použiť existujúcu lokálnu databázu
                    _currentDatabasePath = localDatabase;
                }
                else
                {
                    // Existujú len cudzie databázy - vytvor novú pre tento PC
                    CreateNewDatabase();
                }

                _connectionString = $"Data Source={_currentDatabasePath};Version=3;";
            }
        }

        /// <summary>
        /// Vytvorenie novej databázy pre aktuálny počítač
        /// </summary>
        private void CreateNewDatabase()
        {
            string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"CertificateGenerator_{uniqueId}_{timestamp}.db";
            _currentDatabasePath = Path.Combine(_databaseFolder, fileName);
            _connectionString = $"Data Source={_currentDatabasePath};Version=3;";

            InitializeDatabase();
            Debug.WriteLine($"Vytvorená nová databáza: {fileName}");
        }

        /// <summary>
        /// Získanie zoznamu všetkých databáz v priečinku
        /// </summary>
        public List<string> GetAllDatabases()
        {
            if (!Directory.Exists(_databaseFolder))
                return new List<string>();

            return Directory.GetFiles(_databaseFolder, "CertificateGenerator_*.db")
                           .OrderByDescending(f => File.GetLastWriteTime(f))
                           .ToList();
        }

        /// <summary>
        /// Získanie informácií o databáze
        /// </summary>
        public DatabaseInfo GetDatabaseInfo(string databasePath)
        {
            string fileName = Path.GetFileName(databasePath);
            var parts = fileName.Replace(".db", "").Split('_');

            if (parts.Length >= 3)
            {
                string machineId = parts[1];
                string timestamp = parts[2];

                return new DatabaseInfo
                {
                    FilePath = databasePath,
                    FileName = fileName,
                    MachineIdentifier = machineId,
                    Timestamp = timestamp,
                    IsLocal = machineId == _machineIdentifier,
                    LastModified = File.GetLastWriteTime(databasePath),
                    FileSize = new FileInfo(databasePath).Length
                };
            }

            return null;
        }

        /// <summary>
        /// Detekcia cudzích databáz
        /// </summary>
        public bool HasForeignDatabases()
        {
            var allDatabases = GetAllDatabases();
            return allDatabases.Any(db => !Path.GetFileName(db).Contains(_machineIdentifier));
        }

        /// <summary>
        /// Získanie zoznamu cudzích databáz
        /// </summary>
        public List<DatabaseInfo> GetForeignDatabases()
        {
            var allDatabases = GetAllDatabases();
            var foreignDatabases = allDatabases
                .Where(db => !Path.GetFileName(db).Contains(_machineIdentifier))
                .Select(db => GetDatabaseInfo(db))
                .Where(info => info != null)
                .ToList();

            return foreignDatabases;
        }

        /// <summary>
        /// Zlúčenie všetkých databáz do jednej novej
        /// </summary>
        public MergeResult MergeDatabases()
        {
            var result = new MergeResult();
            var allDatabases = GetAllDatabases();

            if (allDatabases.Count <= 1)
            {
                result.Success = false;
                result.Message = "Nie sú k dispozícii žiadne databázy na zlúčenie.";
                return result;
            }

            try
            {
                // 1. Vytvor zálohu starých databáz
                string backupPath = CreateBackupBeforeMerge(allDatabases);
                result.BackupPath = backupPath;

                // 2. Vytvor novú zlúčenú databázu
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string mergedFileName = $"CertificateGenerator_{_machineIdentifier}_{timestamp}_merged.db";
                string mergedDbPath = Path.Combine(_databaseFolder, mergedFileName);

                // 3. Vytvor štruktúru novej databázy
                CreateEmptyDatabase(mergedDbPath);

                // 4. Skopíruj dáta zo všetkých databáz
                int totalRecords = 0;
                foreach (var dbPath in allDatabases)
                {
                    totalRecords += CopyDatabaseContent(dbPath, mergedDbPath);
                }

                result.TotalRecordsMerged = totalRecords;
                result.DatabasesMerged = allDatabases.Count;

                // 5. Nastav novú databázu ako aktívnu
                _currentDatabasePath = mergedDbPath;
                _connectionString = $"Data Source={_currentDatabasePath};Version=3;";

                // 6. Vymaž staré databázy
                foreach (var dbPath in allDatabases)
                {
                    try
                    {
                        File.Delete(dbPath);
                        Debug.WriteLine($"Vymazaná stará databáza: {Path.GetFileName(dbPath)}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Chyba pri mazaní {Path.GetFileName(dbPath)}: {ex.Message}");
                    }
                }

                result.Success = true;
                result.Message = $"Úspešne zlúčených {result.DatabasesMerged} databáz ({result.TotalRecordsMerged} záznamov).";
                result.NewDatabasePath = mergedDbPath;

                Debug.WriteLine($"Zlúčenie dokončené: {mergedFileName}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Chyba pri zlúčení databáz: {ex.Message}";
                Debug.WriteLine($"Chyba pri zlúčení: {ex}");
            }

            return result;
        }

        /// <summary>
        /// Vytvorenie zálohy pred zlúčením
        /// </summary>
        private string CreateBackupBeforeMerge(List<string> databases)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string backupFileName = $"Backup_BeforeMerge_{timestamp}.zip";
            string backupPath = Path.Combine(_databaseFolder, backupFileName);

            using (var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
            {
                foreach (var dbPath in databases)
                {
                    archive.CreateEntryFromFile(dbPath, Path.GetFileName(dbPath));
                }
            }

            Debug.WriteLine($"Vytvorená záloha: {backupFileName}");
            return backupPath;
        }

        /// <summary>
        /// Vytvorenie prázdnej databázy so štruktúrou tabuliek
        /// </summary>
        private void CreateEmptyDatabase(string databasePath)
        {
            string connString = $"Data Source={databasePath};Version=3;";

            using (var connection = new SQLiteConnection(connString))
            {
                connection.Open();
                CreateTables(connection);
            }
        }

        /// <summary>
        /// Skopírovanie obsahu jednej databázy do druhej
        /// </summary>
        private int CopyDatabaseContent(string sourceDbPath, string targetDbPath)
        {
            int totalRecords = 0;
            string sourceConnString = $"Data Source={sourceDbPath};Version=3;";
            string targetConnString = $"Data Source={targetDbPath};Version=3;";

            using (var sourceConn = new SQLiteConnection(sourceConnString))
            using (var targetConn = new SQLiteConnection(targetConnString))
            {
                sourceConn.Open();
                targetConn.Open();

                // Skopíruj Participants
                totalRecords += CopyTable(sourceConn, targetConn, "Participants",
                    "Name, BirthDate, RegistrationNumber, Notes, CreatedAt, UpdatedAt, UsageCount, LastUsed",
                    "@Name, @BirthDate, @RegistrationNumber, @Notes, @CreatedAt, @UpdatedAt, @UsageCount, @LastUsed");

                // Skopíruj Organizers
                totalRecords += CopyTable(sourceConn, targetConn, "Organizers",
                    "Name, Description, CreatedAt, UpdatedAt, UsageCount, LastUsed",
                    "@Name, @Description, @CreatedAt, @UpdatedAt, @UsageCount, @LastUsed");

                // Skopíruj EventTopics
                totalRecords += CopyTable(sourceConn, targetConn, "EventTopics",
                    "Topic, Description, CreatedAt, UpdatedAt, UsageCount, LastUsed",
                    "@Topic, @Description, @CreatedAt, @UpdatedAt, @UsageCount, @LastUsed");

                // Skopíruj Certificates
                totalRecords += CopyTable(sourceConn, targetConn, "Certificates",
                    "OrganizerId, OrganizerName, ParticipantId, ParticipantName, ParticipantBirthDate, ParticipantRegistrationNumber, EventTopicId, EventTopicName, EventDate, Notes, PaperFormat, FilePath, CreatedAt",
                    "@OrganizerId, @OrganizerName, @ParticipantId, @ParticipantName, @ParticipantBirthDate, @ParticipantRegistrationNumber, @EventTopicId, @EventTopicName, @EventDate, @Notes, @PaperFormat, @FilePath, @CreatedAt");

                // Skopíruj CertificateTemplates
                totalRecords += CopyTable(sourceConn, targetConn, "CertificateTemplates",
                    "Name, IsDefault, TitleColor, TextColor, AccentColor, BackgroundColor, TitleFontFamily, TitleFontSize, HeaderFontFamily, HeaderFontSize, TextFontFamily, TextFontSize, MarginTop, MarginRight, MarginBottom, MarginLeft, TitleAlignment, ShowSeparatorLine, SeparatorStyle, LogoPath, LogoPosition, LogoWidth, LogoHeight, CertificateTitle, ShowTitle, ShowOrganizer, ShowEventTopic, ShowEventDate, ShowBirthDate, ShowRegistrationNumber, ShowNotes, CustomHeaderText, CustomFooterText, MainContentText, ShowMainContent, ShowBorder, BorderColor, BorderWidth, LabelOrganizer, LabelEventTopic, LabelParticipant, LabelEventDate, LabelBirthDate, LabelRegistrationNumber, LabelNotes, CreatedAt, UpdatedAt",
                    "@Name, @IsDefault, @TitleColor, @TextColor, @AccentColor, @BackgroundColor, @TitleFontFamily, @TitleFontSize, @HeaderFontFamily, @HeaderFontSize, @TextFontFamily, @TextFontSize, @MarginTop, @MarginRight, @MarginBottom, @MarginLeft, @TitleAlignment, @ShowSeparatorLine, @SeparatorStyle, @LogoPath, @LogoPosition, @LogoWidth, @LogoHeight, @CertificateTitle, @ShowTitle, @ShowOrganizer, @ShowEventTopic, @ShowEventDate, @ShowBirthDate, @ShowRegistrationNumber, @ShowNotes, @CustomHeaderText, @CustomFooterText, @MainContentText, @ShowMainContent, @ShowBorder, @BorderColor, @BorderWidth, @LabelOrganizer, @LabelEventTopic, @LabelParticipant, @LabelEventDate, @LabelBirthDate, @LabelRegistrationNumber, @LabelNotes, @CreatedAt, @UpdatedAt");
            }

            return totalRecords;
        }

        /// <summary>
        /// Skopírovanie jednej tabuľky medzi databázami
        /// </summary>
        private int CopyTable(SQLiteConnection sourceConn, SQLiteConnection targetConn, string tableName, string columns, string parameters)
        {
            int count = 0;

            // Kontrola existencie tabuľky v zdrojovej databáze
            string checkTableQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
            using (var checkCmd = new SQLiteCommand(checkTableQuery, sourceConn))
            {
                var result = checkCmd.ExecuteScalar();
                if (result == null)
                {
                    Debug.WriteLine($"Tabuľka {tableName} neexistuje v zdrojovej databáze - preskakujem");
                    return 0;
                }
            }

            string selectQuery = $"SELECT {columns} FROM {tableName}";
            string insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

            try
            {
                using (var selectCmd = new SQLiteCommand(selectQuery, sourceConn))
                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        using (var insertCmd = new SQLiteCommand(insertQuery, targetConn))
                        {
                            // Pridaj parametre
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string paramName = $"@{reader.GetName(i)}";
                                insertCmd.Parameters.AddWithValue(paramName, reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i));
                            }

                            insertCmd.ExecuteNonQuery();
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba pri kopírovaní tabuľky {tableName}: {ex.Message}");
                // Pokračuj aj v prípade chyby - niektoré tabuľky môžu mať inú štruktúru
            }

            Debug.WriteLine($"Skopírovaných {count} záznamov z tabuľky {tableName}");
            return count;
        }

        /// <summary>
        /// Inicializácia databázy a vytvorenie tabuliek
        /// </summary>
        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                CreateTables(connection);
            }
        }

        /// <summary>
        /// Vytvorenie tabuliek v databáze
        /// </summary>
        private void CreateTables(SQLiteConnection connection)
        {
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

            Debug.WriteLine("Tabuľky vytvorené/overené");
        }

        /// <summary>
        /// Prepnutie na inú databázu
        /// </summary>
        public bool SwitchDatabase(string databasePath)
        {
            if (!File.Exists(databasePath))
                return false;

            try
            {
                _currentDatabasePath = databasePath;
                _connectionString = $"Data Source={_currentDatabasePath};Version=3;";
                Debug.WriteLine($"Prepnuté na databázu: {Path.GetFileName(databasePath)}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba pri prepínaní databázy: {ex.Message}");
                return false;
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
        /// Získanie cesty k aktuálnej databáze
        /// </summary>
        public string GetDatabasePath()
        {
            return _currentDatabasePath;
        }

        /// <summary>
        /// Získanie priečinka pre databázy
        /// </summary>
        public string GetDatabaseFolder()
        {
            return _databaseFolder;
        }

        /// <summary>
        /// Zálohuje aktuálnu databázu
        /// </summary>
        public void BackupDatabase(string backupPath)
        {
            File.Copy(_currentDatabasePath, backupPath, true);
        }

        /// <summary>
        /// Obnoví databázu zo zálohy
        /// </summary>
        public void RestoreDatabase(string backupPath)
        {
            if (File.Exists(backupPath))
            {
                string targetPath = Path.Combine(_databaseFolder, Path.GetFileName(backupPath));
                File.Copy(backupPath, targetPath, true);
            }
        }
    }

    /// <summary>
    /// Informácie o databáze
    /// </summary>
    public class DatabaseInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string MachineIdentifier { get; set; }
        public string Timestamp { get; set; }
        public bool IsLocal { get; set; }
        public DateTime LastModified { get; set; }
        public long FileSize { get; set; }

        public string DisplayName
        {
            get
            {
                string localTag = IsLocal ? " [Tento PC]" : " [Iný PC]";
                return $"{FileName}{localTag}";
            }
        }

        public string FormattedTimestamp
        {
            get
            {
                if (Timestamp.Length == 14)
                {
                    return $"{Timestamp.Substring(0, 4)}-{Timestamp.Substring(4, 2)}-{Timestamp.Substring(6, 2)} " +
                           $"{Timestamp.Substring(8, 2)}:{Timestamp.Substring(10, 2)}:{Timestamp.Substring(12, 2)}";
                }
                return Timestamp;
            }
        }

        public string FormattedFileSize
        {
            get
            {
                if (FileSize < 1024)
                    return $"{FileSize} B";
                else if (FileSize < 1024 * 1024)
                    return $"{FileSize / 1024:F2} KB";
                else
                    return $"{FileSize / (1024 * 1024):F2} MB";
            }
        }
    }

    /// <summary>
    /// Výsledok zlúčenia databáz
    /// </summary>
    public class MergeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int DatabasesMerged { get; set; }
        public int TotalRecordsMerged { get; set; }
        public string NewDatabasePath { get; set; }
        public string BackupPath { get; set; }
    }
}
