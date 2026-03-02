# TimeTracker Database Schema

The TimeTracker application uses **SQLite** as its database engine. The database file is stored in the user's Documents folder as `timetracker.db`.

## Tables

### 1. Locations

Stores information about client locations/facilities where work is performed.

```sql
CREATE TABLE Locations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FacilityName TEXT NOT NULL,
    ContactName TEXT NOT NULL,
    ContactEmail TEXT,
    ContactPhone TEXT,
    Address TEXT,
    City TEXT,
    State TEXT,
    Zip TEXT,
    PayRate REAL NOT NULL,
    PayRateType TEXT NOT NULL
);
```

**Columns:**
- `Id` - Primary key, auto-incremented
- `FacilityName` - Name of the facility/location (required)
- `ContactName` - Primary contact person's name (required)
- `ContactEmail` - Contact's email address
- `ContactPhone` - Contact's phone number
- `Address` - Street address
- `City` - City name
- `State` - State abbreviation
- `Zip` - ZIP/postal code
- `PayRate` - Hourly or daily rate (required)
- `PayRateType` - Either "Hourly" or "Daily" (required)

---

### 2. TimeEntries

Stores individual time entries for work performed at locations.

```sql
CREATE TABLE TimeEntries (
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
```

**Columns:**
- `Id` - Primary key, auto-incremented
- `LocationId` - Foreign key to Locations table (required)
- `Date` - Date of work in text format (required)
- `ArrivalTime` - Start time (e.g., "7:00 AM") (required)
- `DepartureTime` - End time (e.g., "5:00 PM") (required)
- `DailyPay` - Calculated payment for this entry (required)
- `Notes` - Optional notes about this time entry
- `Locked` - 0 = unlocked, 1 = locked (prevents editing/deletion)
- `Archived` - 0 = active, 1 = archived (hidden unless "Show Archived" is checked)

**Business Rules:**
- An entry must be locked before it can be archived
- Locked entries cannot be bulk-deleted
- Archived entries can be shown/hidden via checkbox

---

### 3. BusinessSettings

Stores business information used for invoice generation. Single-row table.

```sql
CREATE TABLE BusinessSettings (
    Id INTEGER PRIMARY KEY CHECK (Id = 1),
    BusinessName TEXT,
    BusinessAddress TEXT,
    BusinessCity TEXT,
    BusinessState TEXT,
    BusinessZip TEXT,
    BusinessPhone TEXT,
    BusinessEmail TEXT,
    ShowClearAllButton INTEGER DEFAULT 0
);
```

**Columns:**
- `Id` - Always 1 (enforced by CHECK constraint)
- `BusinessName` - Your business/company name
- `BusinessAddress` - Street address
- `BusinessCity` - City name
- `BusinessState` - State abbreviation
- `BusinessZip` - ZIP/postal code
- `BusinessPhone` - Business phone number
- `BusinessEmail` - Business email address
- `ShowClearAllButton` - 0 = hidden, 1 = show "Clear All Unlocked" button

**Note:** This is a single-row table. Only one record with Id=1 exists.

---

## Relationships

```
Locations (1) ----< (many) TimeEntries
```

- One Location can have many TimeEntries
- Each TimeEntry belongs to exactly one Location
- Foreign key constraint: `TimeEntries.LocationId → Locations.Id`

---

## Indexes

The application uses the default SQLite indexes on PRIMARY KEY columns. No additional indexes are explicitly created.

---

## Data Types

SQLite uses dynamic typing, but the schema defines these types:

- **INTEGER** - Whole numbers (including booleans: 0/1)
- **REAL** - Floating point numbers (for currency/rates)
- **TEXT** - String data

---

## Database Location

**Default Path:**
```
C:\Users\{username}\Documents\timetracker.db
```

The database is automatically created on first run if it doesn't exist.

---

## Migration Strategy

The application uses ALTER TABLE statements wrapped in try-catch blocks to add columns to existing databases. This allows the schema to evolve without breaking existing installations.

Example:
```sql
ALTER TABLE Locations ADD COLUMN City TEXT
```

If the column already exists, the error is silently caught and ignored.

---

## Sample Queries

### Get all unlocked time entries with location info
```sql
SELECT t.Id, t.LocationId, l.FacilityName, t.Date, 
       t.ArrivalTime, t.DepartureTime, t.DailyPay, 
       t.Notes, t.Locked, t.Archived
FROM TimeEntries t
INNER JOIN Locations l ON t.LocationId = l.Id
WHERE (t.Locked = 0 OR t.Locked IS NULL)
  AND (t.Archived = 0 OR t.Archived IS NULL);
```

### Get all locations ordered by name
```sql
SELECT * FROM Locations ORDER BY FacilityName;
```

### Get business settings
```sql
SELECT * FROM BusinessSettings WHERE Id = 1;
```

### Count locked entries
```sql
SELECT COUNT(*) FROM TimeEntries WHERE Locked = 1;
```

---

## Version History

- **Initial Schema** - Locations and TimeEntries tables
- **v1.1** - Added City, State, Zip to Locations
- **v1.2** - Added Locked and Archived columns to TimeEntries
- **v1.3** - Added BusinessSettings table
- **v1.4** - Added City, State, Zip to BusinessSettings
- **v1.5** - Added ShowClearAllButton to BusinessSettings
