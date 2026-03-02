using Microsoft.Data.Sqlite;

namespace TimeTracker
{
    public class TimeEntryService
    {
        private readonly string dbPath;

        public TimeEntryService(string databasePath)
        {
            dbPath = databasePath;
        }

        public bool IsEntryLocked(int id)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            
            command.CommandText = "SELECT Locked FROM TimeEntries WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            var result = command.ExecuteScalar();
            
            return result != null && !Convert.IsDBNull(result) && Convert.ToInt32(result) == 1;
        }

        public bool IsEntryArchived(int id)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            
            command.CommandText = "SELECT Archived FROM TimeEntries WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            var result = command.ExecuteScalar();
            
            return result != null && !Convert.IsDBNull(result) && Convert.ToInt32(result) == 1;
        }

        public void ToggleLock(int id)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            
            // Get current lock status
            command.CommandText = "SELECT Locked FROM TimeEntries WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            var result = command.ExecuteScalar();
            int currentLock = result != null && !Convert.IsDBNull(result) ? Convert.ToInt32(result) : 0;
            
            // Toggle lock
            int newLock = currentLock == 1 ? 0 : 1;
            command.CommandText = "UPDATE TimeEntries SET Locked = $locked WHERE Id = $id";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("$locked", newLock);
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        public ArchiveResult ToggleArchive(int id)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            
            // Get current lock and archive status
            command.CommandText = "SELECT Locked, Archived FROM TimeEntries WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                int locked = !reader.IsDBNull(0) ? reader.GetInt32(0) : 0;
                int archived = !reader.IsDBNull(1) ? reader.GetInt32(1) : 0;
                reader.Close();
                
                // If trying to archive (not currently archived), check if locked
                if (archived == 0 && locked == 0)
                {
                    return new ArchiveResult 
                    { 
                        Success = false, 
                        Message = "This entry must be locked before it can be archived." 
                    };
                }
                
                // Toggle archive status
                int newArchive = archived == 1 ? 0 : 1;
                command.CommandText = "UPDATE TimeEntries SET Archived = $archived WHERE Id = $id";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("$archived", newArchive);
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
                
                return new ArchiveResult 
                { 
                    Success = true, 
                    IsArchived = newArchive == 1 
                };
            }
            
            return new ArchiveResult 
            { 
                Success = false, 
                Message = "Entry not found." 
            };
        }

        public void DeleteUnlockedEntries()
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM TimeEntries WHERE Locked = 0 OR Locked IS NULL";
            command.ExecuteNonQuery();
        }

        public int CountLockedEntries()
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM TimeEntries WHERE Locked = 1";
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int CountArchivedEntries()
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM TimeEntries WHERE Archived = 1";
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int CountUnlockedEntries()
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM TimeEntries WHERE Locked = 0 OR Locked IS NULL";
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }

    public class ArchiveResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
    }
}
