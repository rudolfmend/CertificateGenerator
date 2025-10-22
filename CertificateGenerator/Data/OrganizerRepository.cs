using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Repository pre organizátorov
    /// </summary>
    public class OrganizerRepository
    {
        private readonly string _connectionString;

        public OrganizerRepository(DatabaseManager dbManager)
        {
            _connectionString = dbManager.GetConnectionString();
        }

        public int Add(OrganizerModel organizer)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"INSERT INTO Organizers 
                    (Name, Description, CreatedAt, UsageCount) 
                    VALUES (@Name, @Description, @CreatedAt, 0);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", organizer.Name);
                    command.Parameters.AddWithValue("@Description", organizer.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void Update(OrganizerModel organizer)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"UPDATE Organizers 
                    SET Name = @Name, 
                        Description = @Description, 
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", organizer.Id);
                    command.Parameters.AddWithValue("@Name", organizer.Name);
                    command.Parameters.AddWithValue("@Description", organizer.Description ?? (object)DBNull.Value);
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
                string sql = "DELETE FROM Organizers WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public OrganizerModel GetById(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Organizers WHERE Id = @Id";

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

        public List<OrganizerModel> GetAll()
        {
            var organizers = new List<OrganizerModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Organizers ORDER BY Name";

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        organizers.Add(MapReaderToModel(reader));
                    }
                }
            }

            return organizers;
        }

        public List<OrganizerModel> Search(string searchTerm)
        {
            var organizers = new List<OrganizerModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT * FROM Organizers 
                    WHERE Name LIKE @SearchTerm 
                       OR Description LIKE @SearchTerm
                    ORDER BY Name";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            organizers.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return organizers;
        }

        public void IncrementUsage(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"UPDATE Organizers 
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

        public List<OrganizerModel> GetMostUsed(int count = 10)
        {
            var organizers = new List<OrganizerModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT * FROM Organizers 
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
                            organizers.Add(MapReaderToModel(reader));
                        }
                    }
                }
            }

            return organizers;
        }

        private OrganizerModel MapReaderToModel(SQLiteDataReader reader)
        {
            return new OrganizerModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString(),
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
