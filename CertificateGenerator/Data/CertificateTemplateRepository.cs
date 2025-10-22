using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Repository pre šablóny certifikátov
    /// </summary>
    public class CertificateTemplateRepository
    {
        private readonly string _connectionString;

        public CertificateTemplateRepository(DatabaseManager dbManager)
        {
            _connectionString = dbManager.GetConnectionString();
            EnsureTableExists();
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

        public int Add(CertificateTemplateModel template)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Ak je toto default, zrušíme default pre ostatné
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
                     TitleAlignment, ShowSeparatorLine, SeparatorStyle,
                     LogoPath, LogoPosition, LogoWidth, LogoHeight,
                     CertificateTitle, ShowTitle, ShowOrganizer, ShowEventTopic, ShowEventDate,
                     ShowBirthDate, ShowRegistrationNumber, ShowNotes,
                     CustomHeaderText, CustomFooterText, ShowBorder, BorderColor, BorderWidth,
                     LabelOrganizer, LabelEventTopic, LabelParticipant, LabelEventDate,
                     LabelBirthDate, LabelRegistrationNumber, LabelNotes, CreatedAt)
                    VALUES 
                    (@Name, @IsDefault, @TitleColor, @TextColor, @AccentColor, @BackgroundColor,
                     @TitleFontFamily, @TitleFontSize, @HeaderFontFamily, @HeaderFontSize,
                     @TextFontFamily, @TextFontSize, @MarginTop, @MarginRight, @MarginBottom, @MarginLeft,
                     @TitleAlignment, @ShowSeparatorLine, @SeparatorStyle,
                     @LogoPath, @LogoPosition, @LogoWidth, @LogoHeight,
                     @CertificateTitle, @ShowTitle, @ShowOrganizer, @ShowEventTopic, @ShowEventDate,
                     @ShowBirthDate, @ShowRegistrationNumber, @ShowNotes,
                     @CustomHeaderText, @CustomFooterText, @ShowBorder, @BorderColor, @BorderWidth,
                     @LabelOrganizer, @LabelEventTopic, @LabelParticipant, @LabelEventDate,
                     @LabelBirthDate, @LabelRegistrationNumber, @LabelNotes, @CreatedAt);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    AddParameters(command, template);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void Update(CertificateTemplateModel template)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Ak je toto default, zrušíme default pre ostatné
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
                    Name = @Name, IsDefault = @IsDefault, TitleColor = @TitleColor, 
                    TextColor = @TextColor, AccentColor = @AccentColor, BackgroundColor = @BackgroundColor,
                    TitleFontFamily = @TitleFontFamily, TitleFontSize = @TitleFontSize,
                    HeaderFontFamily = @HeaderFontFamily, HeaderFontSize = @HeaderFontSize,
                    TextFontFamily = @TextFontFamily, TextFontSize = @TextFontSize,
                    MarginTop = @MarginTop, MarginRight = @MarginRight, 
                    MarginBottom = @MarginBottom, MarginLeft = @MarginLeft,
                    TitleAlignment = @TitleAlignment, ShowSeparatorLine = @ShowSeparatorLine,
                    SeparatorStyle = @SeparatorStyle, LogoPath = @LogoPath, LogoPosition = @LogoPosition,
                    LogoWidth = @LogoWidth, LogoHeight = @LogoHeight, CertificateTitle = @CertificateTitle,
                    ShowTitle = @ShowTitle, ShowOrganizer = @ShowOrganizer, ShowEventTopic = @ShowEventTopic,
                    ShowEventDate = @ShowEventDate, ShowBirthDate = @ShowBirthDate,
                    ShowRegistrationNumber = @ShowRegistrationNumber, ShowNotes = @ShowNotes,
                    CustomHeaderText = @CustomHeaderText, CustomFooterText = @CustomFooterText,
                    ShowBorder = @ShowBorder, BorderColor = @BorderColor, BorderWidth = @BorderWidth,
                    LabelOrganizer = @LabelOrganizer, LabelEventTopic = @LabelEventTopic,
                    LabelParticipant = @LabelParticipant, LabelEventDate = @LabelEventDate,
                    LabelBirthDate = @LabelBirthDate, LabelRegistrationNumber = @LabelRegistrationNumber,
                    LabelNotes = @LabelNotes, UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", template.Id);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));
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
                            return MapReaderToModel(reader);
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
                        return MapReaderToModel(reader);
                    }
                }
            }
            // Vrátime klasickú šablónu ako predvolenú
            return DefaultTemplates.Classic;
        }

        public List<CertificateTemplateModel> GetAll()
        {
            var templates = new List<CertificateTemplateModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM CertificateTemplates ORDER BY IsDefault DESC, Name";

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        templates.Add(MapReaderToModel(reader));
                    }
                }
            }

            return templates;
        }

        public void SetAsDefault(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Zrušíme default pre všetky
                string updateAllSql = "UPDATE CertificateTemplates SET IsDefault = 0";
                using (var updateCmd = new SQLiteCommand(updateAllSql, connection))
                {
                    updateCmd.ExecuteNonQuery();
                }

                // Nastavíme default pre vybranú
                string updateSql = "UPDATE CertificateTemplates SET IsDefault = 1 WHERE Id = @Id";
                using (var command = new SQLiteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddParameters(SQLiteCommand command, CertificateTemplateModel template)
        {
            command.Parameters.AddWithValue("@Name", template.Name);
            command.Parameters.AddWithValue("@IsDefault", template.IsDefault ? 1 : 0);
            command.Parameters.AddWithValue("@TitleColor", template.TitleColor ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TextColor", template.TextColor ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AccentColor", template.AccentColor ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BackgroundColor", template.BackgroundColor ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TitleFontFamily", template.TitleFontFamily ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TitleFontSize", template.TitleFontSize);
            command.Parameters.AddWithValue("@HeaderFontFamily", template.HeaderFontFamily ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HeaderFontSize", template.HeaderFontSize);
            command.Parameters.AddWithValue("@TextFontFamily", template.TextFontFamily ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TextFontSize", template.TextFontSize);
            command.Parameters.AddWithValue("@MarginTop", template.MarginTop);
            command.Parameters.AddWithValue("@MarginRight", template.MarginRight);
            command.Parameters.AddWithValue("@MarginBottom", template.MarginBottom);
            command.Parameters.AddWithValue("@MarginLeft", template.MarginLeft);
            command.Parameters.AddWithValue("@TitleAlignment", template.TitleAlignment ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShowSeparatorLine", template.ShowSeparatorLine ? 1 : 0);
            command.Parameters.AddWithValue("@SeparatorStyle", template.SeparatorStyle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LogoPath", template.LogoPath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LogoPosition", template.LogoPosition ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LogoWidth", template.LogoWidth);
            command.Parameters.AddWithValue("@LogoHeight", template.LogoHeight);
            command.Parameters.AddWithValue("@CertificateTitle", template.CertificateTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShowTitle", template.ShowTitle ? 1 : 0);
            command.Parameters.AddWithValue("@ShowOrganizer", template.ShowOrganizer ? 1 : 0);
            command.Parameters.AddWithValue("@ShowEventTopic", template.ShowEventTopic ? 1 : 0);
            command.Parameters.AddWithValue("@ShowEventDate", template.ShowEventDate ? 1 : 0);
            command.Parameters.AddWithValue("@ShowBirthDate", template.ShowBirthDate ? 1 : 0);
            command.Parameters.AddWithValue("@ShowRegistrationNumber", template.ShowRegistrationNumber ? 1 : 0);
            command.Parameters.AddWithValue("@ShowNotes", template.ShowNotes ? 1 : 0);
            command.Parameters.AddWithValue("@CustomHeaderText", template.CustomHeaderText ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CustomFooterText", template.CustomFooterText ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShowBorder", template.ShowBorder ? 1 : 0);
            command.Parameters.AddWithValue("@BorderColor", template.BorderColor ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BorderWidth", template.BorderWidth);
            command.Parameters.AddWithValue("@LabelOrganizer", template.LabelOrganizer ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LabelEventTopic", template.LabelEventTopic ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LabelParticipant", template.LabelParticipant ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LabelEventDate", template.LabelEventDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LabelBirthDate", template.LabelBirthDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LabelRegistrationNumber", template.LabelRegistrationNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LabelNotes", template.LabelNotes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", template.CreatedAt.ToString("o"));
        }

        private CertificateTemplateModel MapReaderToModel(SQLiteDataReader reader)
        {
            return new CertificateTemplateModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString(),
                IsDefault = Convert.ToInt32(reader["IsDefault"]) == 1,
                TitleColor = reader["TitleColor"]?.ToString(),
                TextColor = reader["TextColor"]?.ToString(),
                AccentColor = reader["AccentColor"]?.ToString(),
                BackgroundColor = reader["BackgroundColor"]?.ToString(),
                TitleFontFamily = reader["TitleFontFamily"]?.ToString(),
                TitleFontSize = Convert.ToInt32(reader["TitleFontSize"]),
                HeaderFontFamily = reader["HeaderFontFamily"]?.ToString(),
                HeaderFontSize = Convert.ToInt32(reader["HeaderFontSize"]),
                TextFontFamily = reader["TextFontFamily"]?.ToString(),
                TextFontSize = Convert.ToInt32(reader["TextFontSize"]),
                MarginTop = Convert.ToInt32(reader["MarginTop"]),
                MarginRight = Convert.ToInt32(reader["MarginRight"]),
                MarginBottom = Convert.ToInt32(reader["MarginBottom"]),
                MarginLeft = Convert.ToInt32(reader["MarginLeft"]),
                TitleAlignment = reader["TitleAlignment"]?.ToString(),
                ShowSeparatorLine = Convert.ToInt32(reader["ShowSeparatorLine"]) == 1,
                SeparatorStyle = reader["SeparatorStyle"]?.ToString(),
                LogoPath = reader["LogoPath"] != DBNull.Value ? reader["LogoPath"].ToString() : null,
                LogoPosition = reader["LogoPosition"]?.ToString(),
                LogoWidth = Convert.ToInt32(reader["LogoWidth"]),
                LogoHeight = Convert.ToInt32(reader["LogoHeight"]),
                CertificateTitle = reader["CertificateTitle"]?.ToString(),
                ShowTitle = Convert.ToInt32(reader["ShowTitle"]) == 1,
                ShowOrganizer = Convert.ToInt32(reader["ShowOrganizer"]) == 1,
                ShowEventTopic = Convert.ToInt32(reader["ShowEventTopic"]) == 1,
                ShowEventDate = Convert.ToInt32(reader["ShowEventDate"]) == 1,
                ShowBirthDate = Convert.ToInt32(reader["ShowBirthDate"]) == 1,
                ShowRegistrationNumber = Convert.ToInt32(reader["ShowRegistrationNumber"]) == 1,
                ShowNotes = Convert.ToInt32(reader["ShowNotes"]) == 1,
                CustomHeaderText = reader["CustomHeaderText"] != DBNull.Value ? reader["CustomHeaderText"].ToString() : null,
                CustomFooterText = reader["CustomFooterText"] != DBNull.Value ? reader["CustomFooterText"].ToString() : null,
                ShowBorder = Convert.ToInt32(reader["ShowBorder"]) == 1,
                BorderColor = reader["BorderColor"]?.ToString(),
                BorderWidth = Convert.ToInt32(reader["BorderWidth"]),
                LabelOrganizer = reader["LabelOrganizer"]?.ToString(),
                LabelEventTopic = reader["LabelEventTopic"]?.ToString(),
                LabelParticipant = reader["LabelParticipant"]?.ToString(),
                LabelEventDate = reader["LabelEventDate"]?.ToString(),
                LabelBirthDate = reader["LabelBirthDate"]?.ToString(),
                LabelRegistrationNumber = reader["LabelRegistrationNumber"]?.ToString(),
                LabelNotes = reader["LabelNotes"]?.ToString(),
                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                UpdatedAt = reader["UpdatedAt"] != DBNull.Value
                    ? DateTime.Parse(reader["UpdatedAt"].ToString())
                    : (DateTime?)null
            };
        }
    }
}

