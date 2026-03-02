# TimeTracker Unit Tests

This project contains unit tests for the TimeTracker application, focusing on the time entry locking and archiving functionality.

## Test Coverage

The `TimeEntryServiceTests` class provides comprehensive coverage of:

### Lock Functionality
- Toggling lock status from unlocked to locked
- Toggling lock status from locked to unlocked
- Multiple toggle operations
- Checking lock status

### Archive Functionality
- Attempting to archive unlocked entries (should fail)
- Archiving locked entries
- Unarchiving entries
- Checking archive status
- Archive/unarchive workflow

### Combined Operations
- Full lock-then-archive workflow
- Lock state preservation during archiving
- Delete unlocked entries while preserving locked ones

### Edge Cases
- Non-existent entries
- Empty database operations
- Count operations

## Running the Tests

### Using Visual Studio
1. Open the solution in Visual Studio
2. Open Test Explorer (Test → Test Explorer)
3. Click "Run All" to execute all tests

### Using Command Line
```bash
dotnet test
```

### Running Specific Tests
```bash
# Run all tests in a specific class
dotnet test --filter TimeEntryServiceTests

# Run a specific test
dotnet test --filter ToggleLock_UnlockedEntry_BecomesLocked
```

## Test Structure

Each test follows the Arrange-Act-Assert (AAA) pattern:
- **Arrange**: Set up test data and preconditions
- **Act**: Execute the functionality being tested
- **Assert**: Verify the expected outcome

## Test Database

Tests use isolated SQLite in-memory databases that are:
- Created fresh for each test
- Automatically cleaned up after each test
- Independent from the production database

## Key Test Scenarios

1. **Lock Toggle Test**: Ensures entries can be locked and unlocked
2. **Archive Validation**: Verifies that only locked entries can be archived
3. **Delete Protection**: Confirms locked entries are preserved when deleting unlocked entries
4. **Workflow Test**: Tests the complete lock → archive → unarchive flow

## Adding New Tests

When adding new tests:
1. Follow the existing naming convention: `MethodName_Scenario_ExpectedBehavior`
2. Use the AAA pattern
3. Test both success and failure cases
4. Include edge cases
5. Keep tests focused and independent
