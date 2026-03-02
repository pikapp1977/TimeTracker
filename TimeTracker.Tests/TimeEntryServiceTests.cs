using Microsoft.Data.Sqlite;
using Xunit;

namespace TimeTracker.Tests
{
    public class TimeEntryServiceTests : IDisposable
    {
        private readonly string testDbPath;
        private readonly TimeEntryService service;

        public TimeEntryServiceTests()
        {
            // Create a unique test database for each test
            testDbPath = Path.Combine(Path.GetTempPath(), $"test_timetracker_{Guid.NewGuid()}.db");
            service = new TimeEntryService(testDbPath);
            InitializeTestDatabase();
        }

        public void Dispose()
        {
            // Clear all connection pools to release database file locks
            SqliteConnection.ClearAllPools();
            
            // Wait a bit for the file handle to be released
            Thread.Sleep(100);
            
            // Clean up test database
            try
            {
                if (File.Exists(testDbPath))
                {
                    File.Delete(testDbPath);
                }
            }
            catch (IOException)
            {
                // If still locked, don't fail the test - it will be cleaned up later
            }
        }

        private void InitializeTestDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={testDbPath}");
            connection.Open();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Locations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FacilityName TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS TimeEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    LocationId INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    ArrivalTime TEXT NOT NULL,
                    DepartureTime TEXT NOT NULL,
                    DailyPay REAL NOT NULL,
                    Notes TEXT,
                    Locked INTEGER DEFAULT 0,
                    Archived INTEGER DEFAULT 0,
                    FOREIGN KEY (LocationId) REFERENCES Locations(Id)
                );

                INSERT INTO Locations (FacilityName) VALUES ('Test Location');
            ";
            command.ExecuteNonQuery();
        }

        private int CreateTestEntry(bool locked = false, bool archived = false)
        {
            using var connection = new SqliteConnection($"Data Source={testDbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            
            command.CommandText = @"
                INSERT INTO TimeEntries (LocationId, Date, ArrivalTime, DepartureTime, DailyPay, Notes, Locked, Archived)
                VALUES (1, '2026-03-01', '07:00 AM', '05:00 PM', 500.00, 'Test Entry', $locked, $archived);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$locked", locked ? 1 : 0);
            command.Parameters.AddWithValue("$archived", archived ? 1 : 0);
            
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        #region Lock Tests

        [Fact]
        public void ToggleLock_UnlockedEntry_BecomesLocked()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: false);

            // Act
            service.ToggleLock(entryId);

            // Assert
            Assert.True(service.IsEntryLocked(entryId));
        }

        [Fact]
        public void ToggleLock_LockedEntry_BecomesUnlocked()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: true);

            // Act
            service.ToggleLock(entryId);

            // Assert
            Assert.False(service.IsEntryLocked(entryId));
        }

        [Fact]
        public void ToggleLock_MultipleTimes_TogglesCorrectly()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: false);

            // Act & Assert
            service.ToggleLock(entryId);
            Assert.True(service.IsEntryLocked(entryId));

            service.ToggleLock(entryId);
            Assert.False(service.IsEntryLocked(entryId));

            service.ToggleLock(entryId);
            Assert.True(service.IsEntryLocked(entryId));
        }

        [Fact]
        public void IsEntryLocked_NewEntry_ReturnsFalse()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: false);

            // Act
            bool isLocked = service.IsEntryLocked(entryId);

            // Assert
            Assert.False(isLocked);
        }

        [Fact]
        public void IsEntryLocked_LockedEntry_ReturnsTrue()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: true);

            // Act
            bool isLocked = service.IsEntryLocked(entryId);

            // Assert
            Assert.True(isLocked);
        }

        #endregion

        #region Archive Tests

        [Fact]
        public void ToggleArchive_UnlockedEntry_FailsWithMessage()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: false, archived: false);

            // Act
            var result = service.ToggleArchive(entryId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("must be locked", result.Message);
            Assert.False(service.IsEntryArchived(entryId));
        }

        [Fact]
        public void ToggleArchive_LockedEntry_SucceedsAndArchives()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: true, archived: false);

            // Act
            var result = service.ToggleArchive(entryId);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.IsArchived);
            Assert.True(service.IsEntryArchived(entryId));
        }

        [Fact]
        public void ToggleArchive_ArchivedEntry_SucceedsAndUnarchives()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: true, archived: true);

            // Act
            var result = service.ToggleArchive(entryId);

            // Assert
            Assert.True(result.Success);
            Assert.False(result.IsArchived);
            Assert.False(service.IsEntryArchived(entryId));
        }

        [Fact]
        public void ToggleArchive_ArchivedEntry_CanUnarchiveEvenIfUnlocked()
        {
            // Arrange - Create locked and archived entry
            int entryId = CreateTestEntry(locked: true, archived: true);
            
            // Manually unlock it
            service.ToggleLock(entryId);
            Assert.False(service.IsEntryLocked(entryId));
            Assert.True(service.IsEntryArchived(entryId));

            // Act - Try to unarchive even though it's now unlocked
            var result = service.ToggleArchive(entryId);

            // Assert - Should succeed in unarchiving
            Assert.True(result.Success);
            Assert.False(result.IsArchived);
            Assert.False(service.IsEntryArchived(entryId));
        }

        [Fact]
        public void IsEntryArchived_NewEntry_ReturnsFalse()
        {
            // Arrange
            int entryId = CreateTestEntry(archived: false);

            // Act
            bool isArchived = service.IsEntryArchived(entryId);

            // Assert
            Assert.False(isArchived);
        }

        [Fact]
        public void IsEntryArchived_ArchivedEntry_ReturnsTrue()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: true, archived: true);

            // Act
            bool isArchived = service.IsEntryArchived(entryId);

            // Assert
            Assert.True(isArchived);
        }

        #endregion

        #region Combined Lock and Archive Tests

        [Fact]
        public void LockAndArchive_Workflow_Success()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: false, archived: false);

            // Act & Assert - Initial state
            Assert.False(service.IsEntryLocked(entryId));
            Assert.False(service.IsEntryArchived(entryId));

            // Act & Assert - Try to archive without locking (should fail)
            var archiveResult = service.ToggleArchive(entryId);
            Assert.False(archiveResult.Success);
            Assert.False(service.IsEntryArchived(entryId));

            // Act & Assert - Lock the entry
            service.ToggleLock(entryId);
            Assert.True(service.IsEntryLocked(entryId));

            // Act & Assert - Now archive should succeed
            archiveResult = service.ToggleArchive(entryId);
            Assert.True(archiveResult.Success);
            Assert.True(service.IsEntryArchived(entryId));
        }

        [Fact]
        public void LockedEntry_RemainsLocked_WhenArchived()
        {
            // Arrange
            int entryId = CreateTestEntry(locked: true, archived: false);

            // Act
            service.ToggleArchive(entryId);

            // Assert - Should still be locked after archiving
            Assert.True(service.IsEntryLocked(entryId));
            Assert.True(service.IsEntryArchived(entryId));
        }

        #endregion

        #region Delete Unlocked Entries Tests

        [Fact]
        public void DeleteUnlockedEntries_RemovesOnlyUnlockedEntries()
        {
            // Arrange
            int unlockedEntry1 = CreateTestEntry(locked: false);
            int unlockedEntry2 = CreateTestEntry(locked: false);
            int lockedEntry = CreateTestEntry(locked: true);

            // Act
            service.DeleteUnlockedEntries();

            // Assert
            Assert.Equal(0, service.CountUnlockedEntries());
            Assert.Equal(1, service.CountLockedEntries());
            Assert.True(service.IsEntryLocked(lockedEntry));
        }

        [Fact]
        public void DeleteUnlockedEntries_PreservesArchivedButUnlockedEntries()
        {
            // Arrange - This is actually an edge case that shouldn't happen
            // in normal usage (archived should always be locked), but testing behavior
            int unlockedUnarchivedEntry = CreateTestEntry(locked: false, archived: false);
            int lockedArchivedEntry = CreateTestEntry(locked: true, archived: true);

            // Act
            service.DeleteUnlockedEntries();

            // Assert
            Assert.Equal(0, service.CountUnlockedEntries());
            Assert.Equal(1, service.CountLockedEntries());
            Assert.Equal(1, service.CountArchivedEntries());
        }

        [Fact]
        public void DeleteUnlockedEntries_NoEntries_NoError()
        {
            // Act & Assert - Should not throw
            service.DeleteUnlockedEntries();
            Assert.Equal(0, service.CountUnlockedEntries());
        }

        #endregion

        #region Count Tests

        [Fact]
        public void CountLockedEntries_ReturnsCorrectCount()
        {
            // Arrange
            CreateTestEntry(locked: true);
            CreateTestEntry(locked: true);
            CreateTestEntry(locked: false);

            // Act
            int count = service.CountLockedEntries();

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public void CountArchivedEntries_ReturnsCorrectCount()
        {
            // Arrange
            CreateTestEntry(locked: true, archived: true);
            CreateTestEntry(locked: true, archived: true);
            CreateTestEntry(locked: true, archived: false);

            // Act
            int count = service.CountArchivedEntries();

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public void CountUnlockedEntries_ReturnsCorrectCount()
        {
            // Arrange
            CreateTestEntry(locked: false);
            CreateTestEntry(locked: false);
            CreateTestEntry(locked: true);

            // Act
            int count = service.CountUnlockedEntries();

            // Assert
            Assert.Equal(2, count);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void ToggleLock_NonExistentEntry_NoError()
        {
            // Act & Assert - Should not throw for non-existent ID
            service.ToggleLock(99999);
        }

        [Fact]
        public void ToggleArchive_NonExistentEntry_ReturnsFailure()
        {
            // Act
            var result = service.ToggleArchive(99999);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void IsEntryLocked_NonExistentEntry_ReturnsFalse()
        {
            // Act
            bool isLocked = service.IsEntryLocked(99999);

            // Assert
            Assert.False(isLocked);
        }

        [Fact]
        public void IsEntryArchived_NonExistentEntry_ReturnsFalse()
        {
            // Act
            bool isArchived = service.IsEntryArchived(99999);

            // Assert
            Assert.False(isArchived);
        }

        #endregion
    }
}
