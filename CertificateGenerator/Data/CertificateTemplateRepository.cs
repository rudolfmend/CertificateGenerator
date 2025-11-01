using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Repository pre šablóny certifikátov - rozšírená verzia
    /// </summary>
    public class CertificateTemplateRepository
    {
        private readonly string _connectionString;

        public CertificateTemplateRepository(DatabaseManager dbManager)
        {
            _connectionString = dbManager.GetConnectionString();
            EnsureTableExists();
            MigrateToExtendedSchema();
        }

        private void EnsureTableExists()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS CertificateTemplates (
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

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void MigrateToExtendedSchema()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Skontroluj či už existujú  stĺpce
                var checkSql = "PRAGMA table_info(CertificateTemplates)";
                var columns = new HashSet<string>();

                using (var cmd = new SQLiteCommand(checkSql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(reader["name"].ToString());
                    }
                }

                // Pridaj  stĺpce ak neexistujú
                if (!columns.Contains("FieldOrder"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN FieldOrder TEXT DEFAULT 'Organizer,EventTopic,EventDate,BirthDate,RegistrationNumber,Notes'");
                }

                if (!columns.Contains("CustomHeaderAlignment"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN CustomHeaderAlignment TEXT DEFAULT 'LEFT'");
                }

                if (!columns.Contains("CustomHeaderBold"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN CustomHeaderBold INTEGER DEFAULT 0");
                }

                if (!columns.Contains("CustomHeaderItalic"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN CustomHeaderItalic INTEGER DEFAULT 0");
                }

                if (!columns.Contains("CustomFooterAlignment"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN CustomFooterAlignment TEXT DEFAULT 'LEFT'");
                }

                if (!columns.Contains("CustomFooterBold"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN CustomFooterBold INTEGER DEFAULT 0");
                }

                if (!columns.Contains("CustomFooterItalic"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN CustomFooterItalic INTEGER DEFAULT 0");
                }

                // Decorations
                if (!columns.Contains("ShowTopDecoration"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN ShowTopDecoration INTEGER DEFAULT 0");
                }

                if (!columns.Contains("TopDecorationColor"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN TopDecorationColor TEXT DEFAULT '#2563EB'");
                }

                if (!columns.Contains("TopDecorationThickness"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN TopDecorationThickness INTEGER DEFAULT 2");
                }

                if (!columns.Contains("ShowBottomDecoration"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN ShowBottomDecoration INTEGER DEFAULT 0");
                }

                if (!columns.Contains("BottomDecorationColor"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN BottomDecorationColor TEXT DEFAULT '#2563EB'");
                }

                if (!columns.Contains("BottomDecorationThickness"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN BottomDecorationThickness INTEGER DEFAULT 2");
                }

                if (!columns.Contains("ContentLayout"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN ContentLayout TEXT DEFAULT 'VERTICAL'");
                }

                if (!columns.Contains("ColumnSpacing"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN ColumnSpacing INTEGER DEFAULT 20");
                }

                if (!columns.Contains("UseCaduceusStyle"))
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE CertificateTemplates ADD COLUMN UseCaduceusStyle INTEGER DEFAULT 0");
                }

                System.Diagnostics.Debug.WriteLine("[Repository Migration] ContentLayout a ColumnSpacing stĺpce skontrolované/pridané");
            }
        }

        private void ExecuteNonQuery(SQLiteConnection connection, string sql)
        {
            try
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                // Ignore if column already exists
            }
        }

        public int Add(CertificateTemplateModel template)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                if (template.IsDefault)
                {
                    string updateSql = "UPDATE CertificateTemplates SET IsDefault = 0";
                    using (var updateCmd = new SQLiteCommand(updateSql, connection))
                    {
                        updateCmd.ExecuteNonQuery();
                    }
                }

                string sql = @"INSERT INTO CertificateTemplates 
                    (Name, IsDefault, TitleColor, TextColor, AccentColor, BackgroundColor,
                     TitleFontFamily, TitleFontSize, HeaderFontFamily, HeaderFontSize,
                     TextFontFamily, TextFontSize, MarginTop, MarginRight, MarginBottom, MarginLeft,
                     TitleAlignment, ShowSeparatorLine, SeparatorStyle, LogoPath, LogoPosition,
                     LogoWidth, LogoHeight, CertificateTitle, ShowTitle,
                     ShowOrganizer, ShowEventTopic, ShowEventDate, ShowBirthDate,
                     ShowRegistrationNumber, ShowNotes, CustomHeaderText, CustomFooterText,
                     ShowBorder, BorderColor, BorderWidth,
                     LabelOrganizer, LabelEventTopic, LabelParticipant, LabelEventDate,
                     LabelBirthDate, LabelRegistrationNumber, LabelNotes,
                     FieldOrder, CustomHeaderAlignment, CustomHeaderBold, CustomHeaderItalic,
                     CustomFooterAlignment, CustomFooterBold, CustomFooterItalic,
                     ContentLayout, ColumnSpacing, UseCaduceusStyle,
                     CreatedAt, UpdatedAt)
                    VALUES 
                    (@Name, @IsDefault, @TitleColor, @TextColor, @AccentColor, @BackgroundColor,
                     @TitleFontFamily, @TitleFontSize, @HeaderFontFamily, @HeaderFontSize,
                     @TextFontFamily, @TextFontSize, @MarginTop, @MarginRight, @MarginBottom, @MarginLeft,
                     @TitleAlignment, @ShowSeparatorLine, @SeparatorStyle, @LogoPath, @LogoPosition,
                     @LogoWidth, @LogoHeight, @CertificateTitle, @ShowTitle,
                     @ShowOrganizer, @ShowEventTopic, @ShowEventDate, @ShowBirthDate,
                     @ShowRegistrationNumber, @ShowNotes, @CustomHeaderText, @CustomFooterText,
                     @ShowBorder, @BorderColor, @BorderWidth,
                     @LabelOrganizer, @LabelEventTopic, @LabelParticipant, @LabelEventDate,
                     @LabelBirthDate, @LabelRegistrationNumber, @LabelNotes,
                     @FieldOrder, @CustomHeaderAlignment, @CustomHeaderBold, @CustomHeaderItalic,
                     @CustomFooterAlignment, @CustomFooterBold, @CustomFooterItalic,
                     @ContentLayout, @ColumnSpacing, @UseCaduceusStyle,
                     @CreatedAt, @UpdatedAt)";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    AddParameters(command, template);
                    command.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"[Repository.AddParameters] ContentLayout={template.ContentLayout}, ColumnSpacing={template.ColumnSpacing}, UseCaduceusStyle={template.UseCaduceusStyle}");

                    return (int)connection.LastInsertRowId;
                }
            }
        }

        public void Update(CertificateTemplateModel template)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                if (template.IsDefault)
                {
                    string updateSql = "UPDATE CertificateTemplates SET IsDefault = 0 WHERE Id != @Id";
                    using (var updateCmd = new SQLiteCommand(updateSql, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@Id", template.Id);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                string sql = @"UPDATE CertificateTemplates SET
                    Name = @Name, IsDefault = @IsDefault,
                    TitleColor = @TitleColor, TextColor = @TextColor,
                    AccentColor = @AccentColor, BackgroundColor = @BackgroundColor,
                    TitleFontFamily = @TitleFontFamily, TitleFontSize = @TitleFontSize,
                    HeaderFontFamily = @HeaderFontFamily, HeaderFontSize = @HeaderFontSize,
                    TextFontFamily = @TextFontFamily, TextFontSize = @TextFontSize,
                    MarginTop = @MarginTop, MarginRight = @MarginRight,
                    MarginBottom = @MarginBottom, MarginLeft = @MarginLeft,
                    TitleAlignment = @TitleAlignment, ShowSeparatorLine = @ShowSeparatorLine,
                    SeparatorStyle = @SeparatorStyle, LogoPath = @LogoPath,
                    LogoPosition = @LogoPosition, LogoWidth = @LogoWidth, LogoHeight = @LogoHeight,
                    CertificateTitle = @CertificateTitle, ShowTitle = @ShowTitle,
                    ShowOrganizer = @ShowOrganizer, ShowEventTopic = @ShowEventTopic,
                    ShowEventDate = @ShowEventDate, ShowBirthDate = @ShowBirthDate,
                    ShowRegistrationNumber = @ShowRegistrationNumber, ShowNotes = @ShowNotes,
                    CustomHeaderText = @CustomHeaderText, CustomFooterText = @CustomFooterText,
                    ShowBorder = @ShowBorder, BorderColor = @BorderColor, BorderWidth = @BorderWidth,
                    LabelOrganizer = @LabelOrganizer, LabelEventTopic = @LabelEventTopic,
                    LabelParticipant = @LabelParticipant, LabelEventDate = @LabelEventDate,
                    LabelBirthDate = @LabelBirthDate, LabelRegistrationNumber = @LabelRegistrationNumber,
                    LabelNotes = @LabelNotes,
                    FieldOrder = @FieldOrder, CustomHeaderAlignment = @CustomHeaderAlignment,
                    CustomHeaderBold = @CustomHeaderBold, CustomHeaderItalic = @CustomHeaderItalic,
                    CustomFooterAlignment = @CustomFooterAlignment, CustomFooterBold = @CustomFooterBold,
                    CustomFooterItalic = @CustomFooterItalic,
                    ShowTopDecoration = @ShowTopDecoration, TopDecorationColor = @TopDecorationColor,
                    TopDecorationThickness = @TopDecorationThickness,
                    ShowBottomDecoration = @ShowBottomDecoration, BottomDecorationColor = @BottomDecorationColor,
                    BottomDecorationThickness = @BottomDecorationThickness,
                    ContentLayout = @ContentLayout, ColumnSpacing = @ColumnSpacing, UseCaduceusStyle = @UseCaduceusStyle,
                    UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", template.Id);
                    AddParameters(command, template);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM CertificateTemplates WHERE Id = @Id";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public CertificateTemplateModel GetById(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM CertificateTemplates WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public CertificateTemplateModel GetDefault()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM CertificateTemplates WHERE IsDefault = 1 LIMIT 1";
                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapFromReader(reader);
                    }
                }
            }
            return DefaultTemplates.Classic;
        }

        public List<CertificateTemplateModel> GetAll()
        {
            var templates = new List<CertificateTemplateModel>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM CertificateTemplates ORDER BY Name";
                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        templates.Add(MapFromReader(reader));
                    }
                }
            }
            return templates;
        }

        private void AddParameters(SQLiteCommand command, CertificateTemplateModel template)
        {
            command.Parameters.AddWithValue("@Name", template.Name);
            command.Parameters.AddWithValue("@IsDefault", template.IsDefault ? 1 : 0);
            command.Parameters.AddWithValue("@TitleColor", template.TitleColor ?? "#000000");
            command.Parameters.AddWithValue("@TextColor", template.TextColor ?? "#000000");
            command.Parameters.AddWithValue("@AccentColor", template.AccentColor ?? "#2563EB");
            command.Parameters.AddWithValue("@BackgroundColor", template.BackgroundColor ?? "#FFFFFF");
            command.Parameters.AddWithValue("@TitleFontFamily", template.TitleFontFamily ?? "Helvetica-Bold");
            command.Parameters.AddWithValue("@TitleFontSize", template.TitleFontSize);
            command.Parameters.AddWithValue("@HeaderFontFamily", template.HeaderFontFamily ?? "Helvetica-Bold");
            command.Parameters.AddWithValue("@HeaderFontSize", template.HeaderFontSize);
            command.Parameters.AddWithValue("@TextFontFamily", template.TextFontFamily ?? "Helvetica");
            command.Parameters.AddWithValue("@TextFontSize", template.TextFontSize);
            command.Parameters.AddWithValue("@MarginTop", template.MarginTop);
            command.Parameters.AddWithValue("@MarginRight", template.MarginRight);
            command.Parameters.AddWithValue("@MarginBottom", template.MarginBottom);
            command.Parameters.AddWithValue("@MarginLeft", template.MarginLeft);
            command.Parameters.AddWithValue("@TitleAlignment", template.TitleAlignment ?? "CENTER");
            command.Parameters.AddWithValue("@ShowSeparatorLine", template.ShowSeparatorLine ? 1 : 0);
            command.Parameters.AddWithValue("@SeparatorStyle", template.SeparatorStyle ?? "UNDERLINE");
            command.Parameters.AddWithValue("@LogoPath", (object)template.LogoPath ?? DBNull.Value);
            command.Parameters.AddWithValue("@LogoPosition", template.LogoPosition ?? "TOP");
            command.Parameters.AddWithValue("@LogoWidth", template.LogoWidth);
            command.Parameters.AddWithValue("@LogoHeight", template.LogoHeight);
            command.Parameters.AddWithValue("@CertificateTitle", template.CertificateTitle ?? "CERTIFIKÁT");
            command.Parameters.AddWithValue("@ShowTitle", template.ShowTitle ? 1 : 0);
            command.Parameters.AddWithValue("@ShowOrganizer", template.ShowOrganizer ? 1 : 0);
            command.Parameters.AddWithValue("@ShowEventTopic", template.ShowEventTopic ? 1 : 0);
            command.Parameters.AddWithValue("@ShowEventDate", template.ShowEventDate ? 1 : 0);
            command.Parameters.AddWithValue("@ShowBirthDate", template.ShowBirthDate ? 1 : 0);
            command.Parameters.AddWithValue("@ShowRegistrationNumber", template.ShowRegistrationNumber ? 1 : 0);
            command.Parameters.AddWithValue("@ShowNotes", template.ShowNotes ? 1 : 0);
            command.Parameters.AddWithValue("@CustomHeaderText", (object)template.CustomHeaderText ?? DBNull.Value);
            command.Parameters.AddWithValue("@CustomFooterText", (object)template.CustomFooterText ?? DBNull.Value);
            command.Parameters.AddWithValue("@ShowBorder", template.ShowBorder ? 1 : 0);
            command.Parameters.AddWithValue("@BorderColor", template.BorderColor ?? "#000000");
            command.Parameters.AddWithValue("@BorderWidth", template.BorderWidth);
            command.Parameters.AddWithValue("@LabelOrganizer", template.LabelOrganizer ?? "Organizátor:");
            command.Parameters.AddWithValue("@LabelEventTopic", template.LabelEventTopic ?? "Téma podujatia:");
            command.Parameters.AddWithValue("@LabelParticipant", template.LabelParticipant ?? "Účastník:");
            command.Parameters.AddWithValue("@LabelEventDate", template.LabelEventDate ?? "Dátum podujatia:");
            command.Parameters.AddWithValue("@LabelBirthDate", template.LabelBirthDate ?? "Dátum narodenia:");
            command.Parameters.AddWithValue("@LabelRegistrationNumber", template.LabelRegistrationNumber ?? "Registračné číslo:");
            command.Parameters.AddWithValue("@LabelNotes", template.LabelNotes ?? "Poznámky:");

            //  parametre
            command.Parameters.AddWithValue("@FieldOrder", template.FieldOrder ?? "Organizer,EventTopic,EventDate,BirthDate,RegistrationNumber,Notes");
            command.Parameters.AddWithValue("@CustomHeaderAlignment", template.CustomHeaderAlignment ?? "LEFT");
            command.Parameters.AddWithValue("@CustomHeaderBold", template.CustomHeaderBold ? 1 : 0);
            command.Parameters.AddWithValue("@CustomHeaderItalic", template.CustomHeaderItalic ? 1 : 0);
            command.Parameters.AddWithValue("@CustomFooterAlignment", template.CustomFooterAlignment ?? "LEFT");
            command.Parameters.AddWithValue("@CustomFooterBold", template.CustomFooterBold ? 1 : 0);
            command.Parameters.AddWithValue("@CustomFooterItalic", template.CustomFooterItalic ? 1 : 0);

            // Dekorácie
            command.Parameters.AddWithValue("@ShowTopDecoration", template.ShowTopDecoration ? 1 : 0);
            command.Parameters.AddWithValue("@TopDecorationColor", template.TopDecorationColor ?? "#2563EB");
            command.Parameters.AddWithValue("@TopDecorationThickness", template.TopDecorationThickness);
            command.Parameters.AddWithValue("@ShowBottomDecoration", template.ShowBottomDecoration ? 1 : 0);
            command.Parameters.AddWithValue("@BottomDecorationColor", template.BottomDecorationColor ?? "#2563EB");
            command.Parameters.AddWithValue("@BottomDecorationThickness", template.BottomDecorationThickness);

            // Layout obsahu
            command.Parameters.AddWithValue("@ContentLayout", template.ContentLayout ?? "VERTICAL");
            command.Parameters.AddWithValue("@ColumnSpacing", template.ColumnSpacing);
            command.Parameters.AddWithValue("@UseCaduceusStyle", template.UseCaduceusStyle ? 1 : 0);

            command.Parameters.AddWithValue("@CreatedAt", template.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            System.Diagnostics.Debug.WriteLine($"[Repository.AddParameters] ContentLayout={template.ContentLayout}, ColumnSpacing={template.ColumnSpacing}");
        }

        private CertificateTemplateModel MapFromReader(SQLiteDataReader reader)
        {
            return new CertificateTemplateModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString(),
                IsDefault = Convert.ToBoolean(reader["IsDefault"]),
                TitleColor = reader["TitleColor"]?.ToString() ?? "#000000",
                TextColor = reader["TextColor"]?.ToString() ?? "#000000",
                AccentColor = reader["AccentColor"]?.ToString() ?? "#2563EB",
                BackgroundColor = reader["BackgroundColor"]?.ToString() ?? "#FFFFFF",
                TitleFontFamily = reader["TitleFontFamily"]?.ToString() ?? "Helvetica-Bold",
                TitleFontSize = Convert.ToInt32(reader["TitleFontSize"]),
                HeaderFontFamily = reader["HeaderFontFamily"]?.ToString() ?? "Helvetica-Bold",
                HeaderFontSize = Convert.ToInt32(reader["HeaderFontSize"]),
                TextFontFamily = reader["TextFontFamily"]?.ToString() ?? "Helvetica",
                TextFontSize = Convert.ToInt32(reader["TextFontSize"]),
                MarginTop = Convert.ToInt32(reader["MarginTop"]),
                MarginRight = Convert.ToInt32(reader["MarginRight"]),
                MarginBottom = Convert.ToInt32(reader["MarginBottom"]),
                MarginLeft = Convert.ToInt32(reader["MarginLeft"]),
                TitleAlignment = reader["TitleAlignment"]?.ToString() ?? "CENTER",
                ShowSeparatorLine = Convert.ToBoolean(reader["ShowSeparatorLine"]),
                SeparatorStyle = reader["SeparatorStyle"]?.ToString() ?? "UNDERLINE",
                LogoPath = reader["LogoPath"]?.ToString(),
                LogoPosition = reader["LogoPosition"]?.ToString() ?? "TOP",
                LogoWidth = Convert.ToInt32(reader["LogoWidth"]),
                LogoHeight = Convert.ToInt32(reader["LogoHeight"]),
                CertificateTitle = reader["CertificateTitle"]?.ToString() ?? "CERTIFIKÁT",
                ShowTitle = Convert.ToBoolean(reader["ShowTitle"]),
                ShowOrganizer = Convert.ToBoolean(reader["ShowOrganizer"]),
                ShowEventTopic = Convert.ToBoolean(reader["ShowEventTopic"]),
                ShowEventDate = Convert.ToBoolean(reader["ShowEventDate"]),
                ShowBirthDate = Convert.ToBoolean(reader["ShowBirthDate"]),
                ShowRegistrationNumber = Convert.ToBoolean(reader["ShowRegistrationNumber"]),
                ShowNotes = Convert.ToBoolean(reader["ShowNotes"]),
                CustomHeaderText = reader["CustomHeaderText"]?.ToString(),
                CustomFooterText = reader["CustomFooterText"]?.ToString(),
                ShowBorder = Convert.ToBoolean(reader["ShowBorder"]),
                BorderColor = reader["BorderColor"]?.ToString() ?? "#000000",
                BorderWidth = Convert.ToInt32(reader["BorderWidth"]),
                LabelOrganizer = reader["LabelOrganizer"]?.ToString() ?? "Organizátor",
                LabelEventTopic = reader["LabelEventTopic"]?.ToString() ?? "Téma podujatia",
                LabelParticipant = reader["LabelParticipant"]?.ToString() ?? "Účastník",
                LabelEventDate = reader["LabelEventDate"]?.ToString() ?? "Dátum podujatia",
                LabelBirthDate = reader["LabelBirthDate"]?.ToString() ?? "Dátum narodenia",
                LabelRegistrationNumber = reader["LabelRegistrationNumber"]?.ToString() ?? "Registračné číslo",
                LabelNotes = reader["LabelNotes"]?.ToString() ?? "Poznámky",

                //  properties
                FieldOrder = GetStringOrDefault(reader, "FieldOrder", "Organizer,EventTopic,EventDate,BirthDate,RegistrationNumber,Notes"),
                CustomHeaderAlignment = GetStringOrDefault(reader, "CustomHeaderAlignment", "LEFT"),
                CustomHeaderBold = GetBoolOrDefault(reader, "CustomHeaderBold", false),
                CustomHeaderItalic = GetBoolOrDefault(reader, "CustomHeaderItalic", false),
                CustomFooterAlignment = GetStringOrDefault(reader, "CustomFooterAlignment", "LEFT"),
                CustomFooterBold = GetBoolOrDefault(reader, "CustomFooterBold", false),
                CustomFooterItalic = GetBoolOrDefault(reader, "CustomFooterItalic", false),

                // Dekorácie
                ShowTopDecoration = GetBoolOrDefault(reader, "ShowTopDecoration", false),
                TopDecorationColor = GetStringOrDefault(reader, "TopDecorationColor", "#2563EB"),
                TopDecorationThickness = GetIntOrDefault(reader, "TopDecorationThickness", 2),

                ShowBottomDecoration = GetBoolOrDefault(reader, "ShowBottomDecoration", false),
                BottomDecorationColor = GetStringOrDefault(reader, "BottomDecorationColor", "#2563EB"),
                BottomDecorationThickness = GetIntOrDefault(reader, "BottomDecorationThickness", 2),

                ContentLayout = GetStringOrDefault(reader, "ContentLayout", "VERTICAL"),
                ColumnSpacing = GetIntOrDefault(reader, "ColumnSpacing", 20),
                UseCaduceusStyle = GetBoolOrDefault(reader, "UseCaduceusStyle", false),

                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? DateTime.Parse(reader["UpdatedAt"].ToString()) : (DateTime?)null
            };
        }

        private string GetStringOrDefault(SQLiteDataReader reader, string columnName, string defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
            }
            catch
            {
                return defaultValue;
            }
        }

        private bool GetBoolOrDefault(SQLiteDataReader reader, string columnName, bool defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : Convert.ToBoolean(reader.GetInt32(ordinal));
            }
            catch
            {
                return defaultValue;
            }
        }

        private int GetIntOrDefault(SQLiteDataReader reader, string columnName, int defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Aktualizuje existujúce preset šablóny v DB podľa ModernTemplatePresets
        /// Volajte túto metódu po zmene preset šablón v kóde
        /// </summary>
        public void ReloadPresetsToDatabase()
        {
            var presets = Helpers.ModernTemplatePresets.GetAllPresets();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                foreach (var preset in presets)
                {
                    // Skontroluj či šablóna s týmto názvom existuje
                    string checkSql = "SELECT Id FROM CertificateTemplates WHERE Name = @Name";
                    int? existingId = null;

                    using (var checkCmd = new SQLiteCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Name", preset.Template.Name);
                        var result = checkCmd.ExecuteScalar();
                        if (result != null)
                        {
                            existingId = Convert.ToInt32(result);
                        }
                    }

                    if (existingId.HasValue)
                    {
                        // Aktualizuj existujúcu šablónu
                        preset.Template.Id = existingId.Value;
                        Update(preset.Template);
                        System.Diagnostics.Debug.WriteLine($"[ReloadPresets] Aktualizovaná: {preset.Name}");
                    }
                    else
                    {
                        // Pridaj novú šablónu
                        Add(preset.Template);
                        System.Diagnostics.Debug.WriteLine($"[ReloadPresets] Pridaná nová: {preset.Name}");
                    }
                }
            }
        }
    }
}
