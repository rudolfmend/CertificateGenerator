using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Repository pre vygenerované certifikáty (história)
    /// </summary>
    public class CertificateRepository
    {
        private readonly string _connectionString;

        public CertificateRepository(DatabaseManager dbManager)
        {
            _connectionString = dbManager.GetConnectionString();
        }

        public int Add(CertificateModel certificate)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"INSERT INTO Certificates 
                    (OrganizerId, OrganizerName, ParticipantId, ParticipantName, 
                     ParticipantBirthDate, ParticipantRegistrationNumber, 
                     EventTopicId, EventTopicName, EventDate, Notes, 
                     PaperFormat, FilePath, CreatedAt) 
                    VALUES 
                    (@OrganizerId, @OrganizerName, @ParticipantId, @ParticipantName, 
                     @ParticipantBirthDate, @ParticipantRegistrationNumber, 
                     @EventTopicId, @EventTopicName, @EventDate, @Notes, 
                     @PaperFormat, @FilePath, @CreatedAt);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OrganizerId", certificate.OrganizerId);
                    command.Parameters.AddWithValue("@OrganizerName", certificate.OrganizerName);
                    command.Parameters.AddWithValue("@ParticipantId", certificate.ParticipantId);
                    command.Parameters.AddWithValue("@ParticipantName", certificate.ParticipantName);
                    command.Parameters.AddWithValue("@ParticipantBirthDate", certificate.ParticipantBirthDate?.ToString("o") ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ParticipantRegistrationNumber", certificate.ParticipantRegistrationNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EventTopicId", certificate.EventTopicId);
                    command.Parameters.AddWithValue("@EventTopicName", certificate.EventTopicName);
                    command.Parameters.AddWithValue("@EventDate", certificate.EventDate?.ToString("o") ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", certificate.Notes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PaperFormat", certificate.PaperFormat ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FilePath", certificate.FilePath ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public CertificateModel GetById(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Certificates WHERE Id = @Id";

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

        public List<CertificateModel> GetAll()
        {
            var certificates = new List<CertificateModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Certificates ORDER BY CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        certificates.Add(MapReaderToModel(reader));
                    }
                }
            }

            return certificates;
        }

        public List<CertificateModel> GetByParticipant(int participantId)
        {
            var certificates = new List<CertificateModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Certificates WHERE ParticipantId = @ParticipantId ORDER BY CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ParticipantId", participantId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            certificates.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return certificates;
        }

        public List<CertificateModel> GetByOrganizer(int organizerId)
        {
            var certificates = new List<CertificateModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Certificates WHERE OrganizerId = @OrganizerId ORDER BY CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OrganizerId", organizerId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            certificates.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return certificates;
        }

        public List<CertificateModel> GetByEventTopic(int eventTopicId)
        {
            var certificates = new List<CertificateModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Certificates WHERE EventTopicId = @EventTopicId ORDER BY CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EventTopicId", eventTopicId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            certificates.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return certificates;
        }

        public List<CertificateModel> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            var certificates = new List<CertificateModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT * FROM Certificates 
                    WHERE CreatedAt >= @StartDate AND CreatedAt <= @EndDate 
                    ORDER BY CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.ToString("o"));
                    command.Parameters.AddWithValue("@EndDate", endDate.ToString("o"));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            certificates.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return certificates;
        }

        public void Delete(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM Certificates WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private CertificateModel MapReaderToModel(SQLiteDataReader reader)
        {
            return new CertificateModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                OrganizerId = Convert.ToInt32(reader["OrganizerId"]),
                OrganizerName = reader["OrganizerName"].ToString(),
                ParticipantId = Convert.ToInt32(reader["ParticipantId"]),
                ParticipantName = reader["ParticipantName"].ToString(),
                ParticipantBirthDate = reader["ParticipantBirthDate"] != DBNull.Value
                    ? DateTime.Parse(reader["ParticipantBirthDate"].ToString())
                    : (DateTime?)null,
                ParticipantRegistrationNumber = reader["ParticipantRegistrationNumber"] != DBNull.Value
                    ? reader["ParticipantRegistrationNumber"].ToString()
                    : null,
                EventTopicId = Convert.ToInt32(reader["EventTopicId"]),
                EventTopicName = reader["EventTopicName"].ToString(),
                EventDate = reader["EventDate"] != DBNull.Value
                    ? DateTime.Parse(reader["EventDate"].ToString())
                    : (DateTime?)null,
                Notes = reader["Notes"] != DBNull.Value
                    ? reader["Notes"].ToString()
                    : null,
                PaperFormat = reader["PaperFormat"] != DBNull.Value
                    ? reader["PaperFormat"].ToString()
                    : null,
                FilePath = reader["FilePath"] != DBNull.Value
                    ? reader["FilePath"].ToString()
                    : null,
                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString())
            };
        }
    }
}
