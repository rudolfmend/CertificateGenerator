using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Repository pre účastníkov
    /// </summary>
    public class ParticipantRepository
    {
        private readonly string _connectionString;

        public ParticipantRepository(DatabaseManager dbManager)
        {
            _connectionString = dbManager.GetConnectionString();
        }

        /// <summary>
        /// Pridá ho účastníka
        /// </summary>
        public int Add(ParticipantModel participant)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"INSERT INTO Participants 
                    (Name, BirthDate, RegistrationNumber, Notes, CreatedAt, UsageCount) 
                    VALUES (@Name, @BirthDate, @RegistrationNumber, @Notes, @CreatedAt, 0);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", participant.Name);
                    command.Parameters.AddWithValue("@BirthDate", participant.BirthDate?.ToString("o") ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RegistrationNumber", participant.RegistrationNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", participant.Notes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Aktualizuje účastníka
        /// </summary>
        public void Update(ParticipantModel participant)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"UPDATE Participants 
                    SET Name = @Name, 
                        BirthDate = @BirthDate, 
                        RegistrationNumber = @RegistrationNumber, 
                        Notes = @Notes, 
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", participant.Id);
                    command.Parameters.AddWithValue("@Name", participant.Name);
                    command.Parameters.AddWithValue("@BirthDate", participant.BirthDate?.ToString("o") ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RegistrationNumber", participant.RegistrationNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", participant.Notes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));

                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Odstráni účastníka
        /// </summary>
        public void Delete(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM Participants WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Získa účastníka podľa ID
        /// </summary>
        public ParticipantModel GetById(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Participants WHERE Id = @Id";

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

        /// <summary>
        /// Získa všetkých účastníkov
        /// </summary>
        public List<ParticipantModel> GetAll()
        {
            var participants = new List<ParticipantModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Participants ORDER BY Name";

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        participants.Add(MapReaderToModel(reader));
                    }
                }
            }

            return participants;
        }

        /// <summary>
        /// Vyhľadá účastníkov podľa mena
        /// </summary>
        public List<ParticipantModel> Search(string searchTerm)
        {
            var participants = new List<ParticipantModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT * FROM Participants 
                    WHERE Name LIKE @SearchTerm 
                       OR RegistrationNumber LIKE @SearchTerm
                    ORDER BY Name";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            participants.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return participants;
        }

        /// <summary>
        /// Aktualizuje počet použití a posledné použitie
        /// </summary>
        public void IncrementUsage(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"UPDATE Participants 
                    SET UsageCount = UsageCount + 1, 
                        LastUsed = @LastUsed 
                    WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@LastUsed", DateTime.Now.ToString("o"));
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Získa najpoužívanejších účastníkov
        /// </summary>
        public List<ParticipantModel> GetMostUsed(int count = 10)
        {
            var participants = new List<ParticipantModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT * FROM Participants 
                    WHERE UsageCount > 0 
                    ORDER BY UsageCount DESC, LastUsed DESC 
                    LIMIT @Count";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Count", count);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            participants.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return participants;
        }

        private ParticipantModel MapReaderToModel(SQLiteDataReader reader)
        {
            return new ParticipantModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString(),
                BirthDate = reader["BirthDate"] != DBNull.Value
                    ? DateTime.Parse(reader["BirthDate"].ToString())
                    : (DateTime?)null,
                RegistrationNumber = reader["RegistrationNumber"] != DBNull.Value
                    ? reader["RegistrationNumber"].ToString()
                    : null,
                Notes = reader["Notes"] != DBNull.Value
                    ? reader["Notes"].ToString()
                    : null,
                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                UpdatedAt = reader["UpdatedAt"] != DBNull.Value
                    ? DateTime.Parse(reader["UpdatedAt"].ToString())
                    : (DateTime?)null,
                UsageCount = Convert.ToInt32(reader["UsageCount"]),
                LastUsed = reader["LastUsed"] != DBNull.Value
                    ? DateTime.Parse(reader["LastUsed"].ToString())
                    : (DateTime?)null
            };
        }
    }
}
