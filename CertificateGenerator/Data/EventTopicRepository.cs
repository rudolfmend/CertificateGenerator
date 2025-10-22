using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Repository pre témy podujatí
    /// </summary>
    public class EventTopicRepository
    {
        private readonly string _connectionString;

        public EventTopicRepository(DatabaseManager dbManager)
        {
            _connectionString = dbManager.GetConnectionString();
        }

        public int Add(EventTopicModel eventTopic)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"INSERT INTO EventTopics 
                    (Topic, Description, CreatedAt, UsageCount) 
                    VALUES (@Topic, @Description, @CreatedAt, 0);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Topic", eventTopic.Topic);
                    command.Parameters.AddWithValue("@Description", eventTopic.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void Update(EventTopicModel eventTopic)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"UPDATE EventTopics 
                    SET Topic = @Topic, 
                        Description = @Description, 
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", eventTopic.Id);
                    command.Parameters.AddWithValue("@Topic", eventTopic.Topic);
                    command.Parameters.AddWithValue("@Description", eventTopic.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM EventTopics WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public EventTopicModel GetById(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM EventTopics WHERE Id = @Id";

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

        public List<EventTopicModel> GetAll()
        {
            var topics = new List<EventTopicModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM EventTopics ORDER BY Topic";

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        topics.Add(MapReaderToModel(reader));
                    }
                }
            }

            return topics;
        }

        public List<EventTopicModel> Search(string searchTerm)
        {
            var topics = new List<EventTopicModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT * FROM EventTopics 
                    WHERE Topic LIKE @SearchTerm 
                       OR Description LIKE @SearchTerm
                    ORDER BY Topic";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topics.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return topics;
        }

        public void IncrementUsage(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"UPDATE EventTopics 
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

        public List<EventTopicModel> GetMostUsed(int count = 10)
        {
            var topics = new List<EventTopicModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT * FROM EventTopics 
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
                            topics.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return topics;
        }

        private EventTopicModel MapReaderToModel(SQLiteDataReader reader)
        {
            return new EventTopicModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Topic = reader["Topic"].ToString(),
                Description = reader["Description"] != DBNull.Value
                    ? reader["Description"].ToString()
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
