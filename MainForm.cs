using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ClosedXML.Excel;

namespace TimeTracker
{
    public partial class MainForm : Form
    {
        private TabControl tabControl;
        private readonly string dbPath;
        private List<Location> locations;
        private List<TimeEntry> timeEntries;
        private BusinessSettings businessSettings;

        public MainForm()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string tryDbPath = Path.Combine(appDir, "timetracker.db");
            
            try
            {
                using var testConn = new SqliteConnection($"Data Source={tryDbPath}");
                testConn.Open();
                using var cmd = testConn.CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS _write_test (id INTEGER)";
                cmd.ExecuteNonQuery();
                dbPath = tryDbPath;
            }
            catch
            {
                string localAppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TimeTracker");
                if (!Directory.Exists(localAppDir))
                    Directory.CreateDirectory(localAppDir);
                dbPath = Path.Combine(localAppDir, "timetracker.db");
            }
            
            locations = new List<Location>();
            timeEntries = new List<TimeEntry>();
            businessSettings = new BusinessSettings();
            InitializeComponent();
            InitializeDatabase();
            LoadLocations();
            LoadTimeEntries();
            LoadBusinessSettings();
            RefreshTimeEntriesList();
            UpdateTotals();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Locations (
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
                )";
            command.ExecuteNonQuery();

            command.CommandText = "ALTER TABLE Locations ADD COLUMN City TEXT";
            try { command.ExecuteNonQuery(); } catch { }

            command.CommandText = "ALTER TABLE Locations ADD COLUMN State TEXT";
            try { command.ExecuteNonQuery(); } catch { }

            command.CommandText = "ALTER TABLE Locations ADD COLUMN Zip TEXT";
            try { command.ExecuteNonQuery(); } catch { }

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS TimeEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    LocationId INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    ArrivalTime TEXT NOT NULL,
                    DepartureTime TEXT NOT NULL,
                    DailyPay REAL NOT NULL,
                    Notes TEXT,
                    FOREIGN KEY (LocationId) REFERENCES Locations(Id)
                )";
            command.ExecuteNonQuery();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS BusinessSettings (
                    Id INTEGER PRIMARY KEY CHECK (Id = 1),
                    BusinessName TEXT,
                    BusinessAddress TEXT,
                    BusinessCity TEXT,
                    BusinessState TEXT,
                    BusinessZip TEXT,
                    BusinessPhone TEXT,
                    BusinessEmail TEXT
                )";
            command.ExecuteNonQuery();

            command.CommandText = "ALTER TABLE BusinessSettings ADD COLUMN BusinessCity TEXT";
            try { command.ExecuteNonQuery(); } catch { }

            command.CommandText = "ALTER TABLE BusinessSettings ADD COLUMN BusinessState TEXT";
            try { command.ExecuteNonQuery(); } catch { }

            command.CommandText = "ALTER TABLE BusinessSettings ADD COLUMN BusinessZip TEXT";
            try { command.ExecuteNonQuery(); } catch { }

            command.CommandText = "INSERT OR IGNORE INTO BusinessSettings (Id) VALUES (1)";
            try { command.ExecuteNonQuery(); } catch { }
        }

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.Size = new System.Drawing.Size(1100, 700);
            this.tabControl.TabIndex = 0;
            // 
            // Locations Tab
            // 
            TabPage locationsTab = new TabPage("Locations");
            locationsTab.Controls.Add(CreateLocationsPanel());
            this.tabControl.Controls.Add(locationsTab);
            // 
            // Time Entry Tab
            // 
            TabPage timeEntryTab = new TabPage("Time Entry");
            timeEntryTab.Controls.Add(CreateTimeEntryPanel());
            this.tabControl.Controls.Add(timeEntryTab);
            // 
            // Invoice Tab
            // 
            TabPage invoiceTab = new TabPage("Generate Invoice");
            invoiceTab.Controls.Add(CreateInvoicePanel());
            this.tabControl.Controls.Add(invoiceTab);
            // 
            // Settings Tab
            // 
            TabPage settingsTab = new TabPage("Settings");
            settingsTab.Controls.Add(CreateSettingsPanel());
            this.tabControl.Controls.Add(settingsTab);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1124, 724);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Time Tracker";
            this.ResumeLayout(false);
        }

        private Panel CreateLocationsPanel()
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            GroupBox addGroup = new GroupBox 
            { 
                Text = "Add New Location", 
                Location = new System.Drawing.Point(10, 10), 
                Size = new System.Drawing.Size(480, 420) 
            };

            Label lblFacility = new Label { Text = "Facility Name:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            TextBox txtFacility = new TextBox { Name = "txtFacilityName", Location = new System.Drawing.Point(20, 48), Width = 390 };

            Label lblContact = new Label { Text = "Contact Name:", Location = new System.Drawing.Point(20, 80), AutoSize = true };
            TextBox txtContact = new TextBox { Name = "txtContactName", Location = new System.Drawing.Point(20, 103), Width = 390 };

            Label lblEmail = new Label { Text = "Contact Email:", Location = new System.Drawing.Point(20, 135), AutoSize = true };
            TextBox txtEmail = new TextBox { Name = "txtContactEmail", Location = new System.Drawing.Point(20, 158), Width = 390 };

            Label lblPhone = new Label { Text = "Contact Phone:", Location = new System.Drawing.Point(20, 190), AutoSize = true };
            TextBox txtPhone = new TextBox { Name = "txtContactPhone", Location = new System.Drawing.Point(20, 213), Width = 390 };

            Label lblAddress = new Label { Text = "Street Address:", Location = new System.Drawing.Point(20, 245), AutoSize = true };
            TextBox txtAddress = new TextBox { Name = "txtAddress", Location = new System.Drawing.Point(20, 268), Width = 390 };

            Label lblCity = new Label { Text = "City:", Location = new System.Drawing.Point(20, 295), AutoSize = true };
            TextBox txtCity = new TextBox { Name = "txtCity", Location = new System.Drawing.Point(20, 315), Width = 180 };

            Label lblState = new Label { Text = "State:", Location = new System.Drawing.Point(220, 295), AutoSize = true };
            TextBox txtState = new TextBox { Name = "txtState", Location = new System.Drawing.Point(220, 315), Width = 60 };

            Label lblZip = new Label { Text = "Zip:", Location = new System.Drawing.Point(300, 295), AutoSize = true };
            TextBox txtZip = new TextBox { Name = "txtZip", Location = new System.Drawing.Point(300, 315), Width = 80 };

            Label lblPayRate = new Label { Text = "Pay Rate ($):", Location = new System.Drawing.Point(20, 345), AutoSize = true };
            TextBox txtPayRate = new TextBox { Name = "txtPayRate", Location = new System.Drawing.Point(20, 365), Width = 120 };

            Label lblPayRateType = new Label { Text = "Pay Rate Type:", Location = new System.Drawing.Point(160, 345), AutoSize = true };
            ComboBox cmbPayRateType = new ComboBox { Name = "cmbPayRateType", Location = new System.Drawing.Point(160, 365), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPayRateType.Items.AddRange(new string[] { "Per Hour", "Per Day" });
            cmbPayRateType.SelectedIndex = 0;

            Button btnAddLocation = new Button 
            { 
                Text = "Add Location", 
                Location = new System.Drawing.Point(300, 360), 
                Size = new System.Drawing.Size(120, 35) 
            };
            btnAddLocation.Click += (s, e) =>
            {
                string facilityName = txtFacility.Text.Trim();
                string contactName = txtContact.Text.Trim();
                string email = txtEmail.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string address = txtAddress.Text.Trim();
                string city = txtCity.Text.Trim();
                string state = txtState.Text.Trim();
                string zip = txtZip.Text.Trim();

                if (string.IsNullOrWhiteSpace(facilityName))
                {
                    MessageBox.Show("Please enter a facility name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPayRate.Text, out decimal payRate))
                {
                    MessageBox.Show("Please enter a valid pay rate.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string payRateType = cmbPayRateType.SelectedItem?.ToString() ?? "Per Hour";

                AddLocation(facilityName, contactName, email, phone, address, city, state, zip, payRate, payRateType);
                
                txtFacility.Clear();
                txtContact.Clear();
                txtEmail.Clear();
                txtPhone.Clear();
                txtAddress.Clear();
                txtCity.Clear();
                txtState.Clear();
                txtZip.Clear();
                txtPayRate.Clear();
                cmbPayRateType.SelectedIndex = 0;
                
                LoadLocations();
                RefreshLocationsList();
                MessageBox.Show("Location added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            addGroup.Controls.AddRange(new Control[] { lblFacility, txtFacility, lblContact, txtContact, 
                lblEmail, txtEmail, lblPhone, txtPhone, lblAddress, txtAddress, 
                lblCity, txtCity, lblState, txtState, lblZip, txtZip,
                lblPayRate, txtPayRate, lblPayRateType, cmbPayRateType, btnAddLocation });
            panel.Controls.Add(addGroup);

            GroupBox listGroup = new GroupBox 
            { 
                Text = "Locations List", 
                Location = new System.Drawing.Point(520, 10), 
                Size = new System.Drawing.Size(560, 660) 
            };

            ListView lstLocations = new ListView 
            { 
                Name = "lstLocations",
                Location = new System.Drawing.Point(10, 25), 
                Size = new System.Drawing.Size(450, 450),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new System.Drawing.Font("Segoe UI", 11)
            };
            lstLocations.Columns.Add("Facility", 180);
            lstLocations.Columns.Add("Contact", 120);
            lstLocations.Columns.Add("Phone", 120);
            lstLocations.Columns.Add("Pay Rate", 120);

            Button btnDeleteLocation = new Button 
            { 
                Text = "Delete Selected", 
                Location = new System.Drawing.Point(140, 485), 
                Size = new System.Drawing.Size(120, 30) 
            };
            btnDeleteLocation.Click += (s, e) =>
            {
                if (lstLocations.SelectedItems.Count > 0)
                {
                    var result = MessageBox.Show("Are you sure you want to delete this location? All time entries for this location will also be deleted.", 
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        var item = lstLocations.SelectedItems[0];
                        int id = (int)item.Tag;
                        DeleteLocation(id);
                        LoadLocations();
                        RefreshLocationsList();
                    }
                }
            };

            Button btnEditLocation = new Button 
            { 
                Text = "Edit Selected", 
                Location = new System.Drawing.Point(10, 485), 
                Size = new System.Drawing.Size(120, 30) 
            };
            btnEditLocation.Click += (s, e) =>
            {
                if (lstLocations.SelectedItems.Count > 0)
                {
                    var item = lstLocations.SelectedItems[0];
                    int id = (int)item.Tag;
                    var location = locations.FirstOrDefault(l => l.Id == id);
                    if (location != null)
                    {
                        ShowEditLocationDialog(location);
                    }
                }
            };

            listGroup.Controls.Add(lstLocations);
            listGroup.Controls.Add(btnEditLocation);
            listGroup.Controls.Add(btnDeleteLocation);
            panel.Controls.Add(listGroup);

            return panel;
        }

        private Panel CreateTimeEntryPanel()
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            GroupBox entryGroup = new GroupBox 
            { 
                Text = "Add Time Entry", 
                Location = new System.Drawing.Point(10, 10), 
                Size = new System.Drawing.Size(920, 180) 
            };

            Label lblLocation = new Label { Text = "Location:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            ComboBox cmbLocation = new ComboBox { Name = "cmbLocation", Location = new System.Drawing.Point(20, 48), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblDate = new Label { Text = "Date:", Location = new System.Drawing.Point(340, 25), AutoSize = true };
            DateTimePicker dtpDate = new DateTimePicker 
            { 
                Name = "dtpDate", 
                Location = new System.Drawing.Point(340, 48), 
                Width = 200,
                Format = DateTimePickerFormat.Short
            };
            dtpDate.Value = DateTime.Today;

            Label lblArrival = new Label { Text = "Arrival Time:", Location = new System.Drawing.Point(20, 78), AutoSize = true };
            ComboBox cmbArrivalHour = new ComboBox { Location = new System.Drawing.Point(20, 101), Width = 60, DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 1; i <= 12; i++) cmbArrivalHour.Items.Add(i);
            cmbArrivalHour.SelectedIndex = 7; // Default to 8 AM
            ComboBox cmbArrivalMin = new ComboBox { Location = new System.Drawing.Point(85, 101), Width = 50, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbArrivalMin.Items.AddRange(new string[] { "00", "15", "30", "45" });
            cmbArrivalMin.SelectedIndex = 0;
            ComboBox cmbArrivalAMPM = new ComboBox { Location = new System.Drawing.Point(140, 101), Width = 50, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbArrivalAMPM.Items.AddRange(new string[] { "AM", "PM" });
            cmbArrivalAMPM.SelectedIndex = 0;

            Label lblDeparture = new Label { Text = "Departure Time:", Location = new System.Drawing.Point(220, 78), AutoSize = true };
            ComboBox cmbDepartureHour = new ComboBox { Location = new System.Drawing.Point(220, 101), Width = 60, DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 1; i <= 12; i++) cmbDepartureHour.Items.Add(i);
            cmbDepartureHour.SelectedIndex = 4; // Default to 5 PM
            ComboBox cmbDepartureMin = new ComboBox { Location = new System.Drawing.Point(285, 101), Width = 50, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDepartureMin.Items.AddRange(new string[] { "00", "15", "30", "45" });
            cmbDepartureMin.SelectedIndex = 0;
            ComboBox cmbDepartureAMPM = new ComboBox { Location = new System.Drawing.Point(340, 101), Width = 50, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDepartureAMPM.Items.AddRange(new string[] { "AM", "PM" });
            cmbDepartureAMPM.SelectedIndex = 1; // Default to PM

            Label lblNotes = new Label { Text = "Notes:", Location = new System.Drawing.Point(420, 78), AutoSize = true };
            TextBox txtNotes = new TextBox { Name = "txtNotes", Location = new System.Drawing.Point(420, 101), Width = 200 };

            Button btnAddEntry = new Button 
            { 
                Text = "Add Entry", 
                Location = new System.Drawing.Point(580, 48), 
                Size = new System.Drawing.Size(120, 35) 
            };
            btnAddEntry.Click += (s, e) =>
            {
                if (cmbLocation.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a location.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Location selectedLocation = (Location)cmbLocation.SelectedItem;
                string date = dtpDate.Value.ToString("MM/dd/yyyy");
                
                int arrivalHour = int.Parse(cmbArrivalHour.SelectedItem.ToString());
                string arrivalMin = cmbArrivalMin.SelectedItem.ToString();
                string arrivalAMPM = cmbArrivalAMPM.SelectedItem.ToString();
                if (arrivalAMPM == "PM" && arrivalHour != 12) arrivalHour += 12;
                if (arrivalAMPM == "AM" && arrivalHour == 12) arrivalHour = 0;
                string arrival = $"{arrivalHour:D2}:{arrivalMin}:00";
                
                int departureHour = int.Parse(cmbDepartureHour.SelectedItem.ToString());
                string departureMin = cmbDepartureMin.SelectedItem.ToString();
                string departureAMPM = cmbDepartureAMPM.SelectedItem.ToString();
                if (departureAMPM == "PM" && departureHour != 12) departureHour += 12;
                if (departureAMPM == "AM" && departureHour == 12) departureHour = 0;
                string departure = $"{departureHour:D2}:{departureMin}:00";
                
                string notes = txtNotes.Text.Trim();

                decimal dailyPay = CalculateDailyPay(selectedLocation, arrival, departure);
                AddTimeEntry(selectedLocation.Id, date, arrival, departure, dailyPay, notes);
                
                cmbArrivalHour.SelectedIndex = 7;
                cmbArrivalMin.SelectedIndex = 0;
                cmbArrivalAMPM.SelectedIndex = 0;
                cmbDepartureHour.SelectedIndex = 4;
                cmbDepartureMin.SelectedIndex = 0;
                cmbDepartureAMPM.SelectedIndex = 1;
                txtNotes.Clear();
                
                LoadTimeEntries();
                RefreshTimeEntriesList();
                UpdateTotals();
            };

            Label lblHoursWorked = new Label { Text = "Total Hours: 0.00", Name = "lblTotalHours", Location = new System.Drawing.Point(20, 145), AutoSize = true };
            Label lblTotalPay = new Label { Text = "Total Pay: $0.00", Name = "lblTotalPay", Location = new System.Drawing.Point(200, 145), AutoSize = true };

            entryGroup.Controls.AddRange(new Control[] { 
                lblLocation, cmbLocation, lblDate, dtpDate, lblArrival, cmbArrivalHour, cmbArrivalMin, cmbArrivalAMPM,
                lblDeparture, cmbDepartureHour, cmbDepartureMin, cmbDepartureAMPM, lblNotes, txtNotes, btnAddEntry, lblHoursWorked, lblTotalPay 
            });
            panel.Controls.Add(entryGroup);

            GroupBox listGroup = new GroupBox 
            { 
                Text = "Time Entries", 
                Location = new System.Drawing.Point(10, 200), 
                Size = new System.Drawing.Size(920, 370) 
            };

            ListView lstEntries = new ListView 
            { 
                Name = "lstTimeEntries",
                Location = new System.Drawing.Point(10, 25), 
                Size = new System.Drawing.Size(900, 280),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lstEntries.Columns.Add("Date", 100);
            lstEntries.Columns.Add("Location", 180);
            lstEntries.Columns.Add("Arrival", 100);
            lstEntries.Columns.Add("Departure", 100);
            lstEntries.Columns.Add("Hours", 80);
            lstEntries.Columns.Add("Daily Pay", 100);
            lstEntries.Columns.Add("Notes", 200);

            Button btnDeleteEntry = new Button 
            { 
                Text = "Delete Selected", 
                Location = new System.Drawing.Point(10, 315), 
                Size = new System.Drawing.Size(120, 30) 
            };
            btnDeleteEntry.Click += (s, e) =>
            {
                if (lstEntries.SelectedItems.Count > 0)
                {
                    var item = lstEntries.SelectedItems[0];
                    int id = (int)item.Tag;
                    DeleteTimeEntry(id);
                    LoadTimeEntries();
                    RefreshTimeEntriesList();
                    UpdateTotals();
                }
            };

            Button btnClearAll = new Button 
            { 
                Text = "Clear All Entries", 
                Location = new System.Drawing.Point(140, 315), 
                Size = new System.Drawing.Size(130, 30) 
            };
            btnClearAll.Click += (s, e) =>
            {
                var result = MessageBox.Show("Are you sure you want to delete all time entries?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    ClearAllTimeEntries();
                    LoadTimeEntries();
                    RefreshTimeEntriesList();
                    UpdateTotals();
                }
            };

            listGroup.Controls.Add(lstEntries);
            listGroup.Controls.Add(btnDeleteEntry);
            listGroup.Controls.Add(btnClearAll);
            panel.Controls.Add(listGroup);

            return panel;
        }

        private Panel CreateInvoicePanel()
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            GroupBox filterGroup = new GroupBox 
            { 
                Text = "Invoice Filter", 
                Location = new System.Drawing.Point(10, 10), 
                Size = new System.Drawing.Size(920, 150) 
            };

            Label lblInvoiceLocation = new Label { Text = "Location:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            ComboBox cmbInvoiceLocation = new ComboBox { Name = "cmbInvoiceLocation", Location = new System.Drawing.Point(20, 50), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblStartDate = new Label { Text = "Start Date:", Location = new System.Drawing.Point(340, 25), AutoSize = true };
            DateTimePicker dtpStartDate = new DateTimePicker { Name = "dtpStartDate", Location = new System.Drawing.Point(340, 50), Width = 200, Format = DateTimePickerFormat.Short };

            Label lblEndDate = new Label { Text = "End Date:", Location = new System.Drawing.Point(560, 25), AutoSize = true };
            DateTimePicker dtpEndDate = new DateTimePicker { Name = "dtpEndDate", Location = new System.Drawing.Point(560, 50), Width = 200, Format = DateTimePickerFormat.Short };

            Button btnGenerate = new Button 
            { 
                Text = "Generate Excel Invoice", 
                Location = new System.Drawing.Point(20, 90), 
                Size = new System.Drawing.Size(180, 40) 
            };
            btnGenerate.Click += (s, e) =>
            {
                if (cmbInvoiceLocation.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a location.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Location selectedLocation = (Location)cmbInvoiceLocation.SelectedItem;
                string startDate = dtpStartDate.Value.ToString("MM/dd/yyyy");
                string endDate = dtpEndDate.Value.ToString("MM/dd/yyyy");

                GenerateInvoice(selectedLocation, startDate, endDate);
            };

            Button btnGenerateAll = new Button 
            { 
                Text = "Generate All Locations", 
                Location = new System.Drawing.Point(220, 90), 
                Size = new System.Drawing.Size(180, 40) 
            };
            btnGenerateAll.Click += (s, e) =>
            {
                string startDate = dtpStartDate.Value.ToString("MM/dd/yyyy");
                string endDate = dtpEndDate.Value.ToString("MM/dd/yyyy");

                foreach (var location in locations)
                {
                    GenerateInvoice(location, startDate, endDate);
                }
            };

            filterGroup.Controls.AddRange(new Control[] { 
                lblInvoiceLocation, cmbInvoiceLocation, lblStartDate, dtpStartDate, 
                lblEndDate, dtpEndDate, btnGenerate, btnGenerateAll 
            });
            panel.Controls.Add(filterGroup);

            GroupBox previewGroup = new GroupBox 
            { 
                Text = "Preview", 
                Location = new System.Drawing.Point(10, 170), 
                Size = new System.Drawing.Size(920, 400) 
            };

            TextBox txtPreview = new TextBox 
            { 
                Name = "txtInvoicePreview",
                Location = new System.Drawing.Point(10, 25), 
                Size = new System.Drawing.Size(900, 350),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new System.Drawing.Font("Courier New", 9)
            };

            previewGroup.Controls.Add(txtPreview);
            panel.Controls.Add(previewGroup);

            return panel;
        }

        private Panel CreateSettingsPanel()
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            GroupBox infoGroup = new GroupBox 
            { 
                Text = "Business Information (Appears on Invoice)", 
                Location = new System.Drawing.Point(10, 10), 
                Size = new System.Drawing.Size(500, 380) 
            };

            Label lblBusinessName = new Label { Text = "Business Name:", Location = new System.Drawing.Point(20, 30), AutoSize = true };
            TextBox txtBusinessName = new TextBox 
            { 
                Name = "txtBusinessName",
                Location = new System.Drawing.Point(20, 53), 
                Width = 400,
                Text = businessSettings.BusinessName
            };

            Label lblBusinessAddress = new Label { Text = "Street Address:", Location = new System.Drawing.Point(20, 85), AutoSize = true };
            TextBox txtBusinessAddress = new TextBox 
            { 
                Name = "txtBusinessAddress",
                Location = new System.Drawing.Point(20, 108), 
                Width = 400,
                Text = businessSettings.BusinessAddress
            };

            Label lblBusinessCity = new Label { Text = "City:", Location = new System.Drawing.Point(20, 138), AutoSize = true };
            TextBox txtBusinessCity = new TextBox 
            { 
                Name = "txtBusinessCity",
                Location = new System.Drawing.Point(20, 160), 
                Width = 180,
                Text = businessSettings.BusinessCity
            };

            Label lblBusinessState = new Label { Text = "State:", Location = new System.Drawing.Point(220, 138), AutoSize = true };
            TextBox txtBusinessState = new TextBox 
            { 
                Name = "txtBusinessState",
                Location = new System.Drawing.Point(220, 160), 
                Width = 60,
                Text = businessSettings.BusinessState
            };

            Label lblBusinessZip = new Label { Text = "Zip:", Location = new System.Drawing.Point(300, 138), AutoSize = true };
            TextBox txtBusinessZip = new TextBox 
            { 
                Name = "txtBusinessZip",
                Location = new System.Drawing.Point(300, 160), 
                Width = 80,
                Text = businessSettings.BusinessZip
            };

            Label lblBusinessPhone = new Label { Text = "Phone Number:", Location = new System.Drawing.Point(20, 195), AutoSize = true };
            TextBox txtBusinessPhone = new TextBox 
            { 
                Name = "txtBusinessPhone",
                Location = new System.Drawing.Point(20, 218), 
                Width = 200,
                Text = businessSettings.BusinessPhone
            };

            Label lblBusinessEmail = new Label { Text = "Email:", Location = new System.Drawing.Point(250, 195), AutoSize = true };
            TextBox txtBusinessEmail = new TextBox 
            { 
                Name = "txtBusinessEmail",
                Location = new System.Drawing.Point(250, 218), 
                Width = 200,
                Text = businessSettings.BusinessEmail
            };

            Button btnSaveSettings = new Button 
            { 
                Text = "Save Settings", 
                Location = new System.Drawing.Point(20, 260), 
                Size = new System.Drawing.Size(120, 35) 
            };
            btnSaveSettings.Click += (s, e) =>
            {
                businessSettings.BusinessName = txtBusinessName.Text.Trim();
                businessSettings.BusinessAddress = txtBusinessAddress.Text.Trim();
                businessSettings.BusinessCity = txtBusinessCity.Text.Trim();
                businessSettings.BusinessState = txtBusinessState.Text.Trim();
                businessSettings.BusinessZip = txtBusinessZip.Text.Trim();
                businessSettings.BusinessPhone = txtBusinessPhone.Text.Trim();
                businessSettings.BusinessEmail = txtBusinessEmail.Text.Trim();
                
                SaveBusinessSettings();
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            infoGroup.Controls.AddRange(new Control[] { 
                lblBusinessName, txtBusinessName, 
                lblBusinessAddress, txtBusinessAddress,
                lblBusinessCity, txtBusinessCity,
                lblBusinessState, txtBusinessState,
                lblBusinessZip, txtBusinessZip,
                lblBusinessPhone, txtBusinessPhone,
                lblBusinessEmail, txtBusinessEmail,
                btnSaveSettings 
            });
            panel.Controls.Add(infoGroup);

            return panel;
        }

        private void DateTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            var textBox = (TextBox)sender;
            
            if (char.IsDigit(e.KeyChar))
            {
                string currentText = textBox.Text.Replace("/", "");
                
                if (currentText.Length >= 8)
                {
                    e.Handled = true;
                    return;
                }

                string newText = currentText + e.KeyChar;
                
                if (newText.Length >= 2)
                    newText = newText.Insert(2, "/");
                if (newText.Length >= 5)
                    newText = newText.Insert(5, "/");
                
                textBox.Text = newText;
                textBox.SelectionStart = newText.Length;
                
                e.Handled = true;
            }
            else if (e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void AddLocation(string facilityName, string contactName, string email, string phone, 
            string address, string city, string state, string zip, decimal payRate, string payRateType)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Locations (FacilityName, ContactName, ContactEmail, ContactPhone, Address, City, State, Zip, PayRate, PayRateType)
                VALUES ($facilityName, $contactName, $contactEmail, $contactPhone, $address, $city, $state, $zip, $payRate, $payRateType)";
            command.Parameters.AddWithValue("$facilityName", facilityName);
            command.Parameters.AddWithValue("$contactName", contactName);
            command.Parameters.AddWithValue("$contactEmail", email);
            command.Parameters.AddWithValue("$contactPhone", phone);
            command.Parameters.AddWithValue("$address", address);
            command.Parameters.AddWithValue("$city", city);
            command.Parameters.AddWithValue("$state", state);
            command.Parameters.AddWithValue("$zip", zip);
            command.Parameters.AddWithValue("$payRate", (double)payRate);
            command.Parameters.AddWithValue("$payRateType", payRateType);
            command.ExecuteNonQuery();
        }

        private void LoadLocations()
        {
            locations.Clear();
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, FacilityName, ContactName, ContactEmail, ContactPhone, Address, City, State, Zip, PayRate, PayRateType FROM Locations ORDER BY FacilityName";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                locations.Add(new Location
                {
                    Id = reader.GetInt32(0),
                    FacilityName = reader.GetString(1),
                    ContactName = reader.GetString(2),
                    ContactEmail = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    ContactPhone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Address = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    City = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    State = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    Zip = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    PayRate = (decimal)reader.GetDouble(9),
                    PayRateType = reader.GetString(10)
                });
            }
        }

        private void LoadBusinessSettings()
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT BusinessName, BusinessAddress, BusinessCity, BusinessState, BusinessZip, BusinessPhone, BusinessEmail FROM BusinessSettings WHERE Id = 1";
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                businessSettings = new BusinessSettings
                {
                    BusinessName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                    BusinessAddress = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    BusinessCity = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    BusinessState = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    BusinessZip = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    BusinessPhone = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    BusinessEmail = reader.IsDBNull(6) ? "" : reader.GetString(6)
                };
            }
        }

        private void SaveBusinessSettings()
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO BusinessSettings (Id, BusinessName, BusinessAddress, BusinessCity, BusinessState, BusinessZip, BusinessPhone, BusinessEmail)
                VALUES (1, $name, $address, $city, $state, $zip, $phone, $email)";
            command.Parameters.AddWithValue("$name", businessSettings.BusinessName);
            command.Parameters.AddWithValue("$address", businessSettings.BusinessAddress);
            command.Parameters.AddWithValue("$city", businessSettings.BusinessCity);
            command.Parameters.AddWithValue("$state", businessSettings.BusinessState);
            command.Parameters.AddWithValue("$zip", businessSettings.BusinessZip);
            command.Parameters.AddWithValue("$phone", businessSettings.BusinessPhone);
            command.Parameters.AddWithValue("$email", businessSettings.BusinessEmail);
            command.ExecuteNonQuery();
        }

        private void DeleteLocation(int id)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM TimeEntries WHERE LocationId = $id";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();

            command.CommandText = "DELETE FROM Locations WHERE Id = $id";
            command.ExecuteNonQuery();
        }

        private void UpdateLocation(int id, string facilityName, string contactName, string email, string phone, 
            string address, string city, string state, string zip, decimal payRate, string payRateType)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Locations 
                SET FacilityName = $facilityName, ContactName = $contactName, ContactEmail = $contactEmail, 
                    ContactPhone = $contactPhone, Address = $address, City = $city, State = $state, Zip = $zip, PayRate = $payRate, PayRateType = $payRateType
                WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$facilityName", facilityName);
            command.Parameters.AddWithValue("$contactName", contactName);
            command.Parameters.AddWithValue("$contactEmail", email);
            command.Parameters.AddWithValue("$contactPhone", phone);
            command.Parameters.AddWithValue("$address", address);
            command.Parameters.AddWithValue("$city", city);
            command.Parameters.AddWithValue("$state", state);
            command.Parameters.AddWithValue("$zip", zip);
            command.Parameters.AddWithValue("$payRate", (double)payRate);
            command.Parameters.AddWithValue("$payRateType", payRateType);
            command.ExecuteNonQuery();
        }

        private void ShowEditLocationDialog(Location location)
        {
            using var dialog = new Form
            {
                Width = 450,
                Height = 400,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Edit Location",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label lblFacility = new Label { Left = 20, Top = 20, Width = 150, Text = "Facility Name:" };
            TextBox txtFacility = new TextBox { Left = 20, Top = 42, Width = 390, Text = location.FacilityName };

            Label lblContact = new Label { Left = 20, Top = 72, Width = 150, Text = "Contact Name:" };
            TextBox txtContact = new TextBox { Left = 20, Top = 94, Width = 390, Text = location.ContactName };

            Label lblEmail = new Label { Left = 20, Top = 124, Width = 150, Text = "Contact Email:" };
            TextBox txtEmail = new TextBox { Left = 20, Top = 146, Width = 390, Text = location.ContactEmail };

            Label lblPhone = new Label { Left = 20, Top = 176, Width = 150, Text = "Contact Phone:" };
            TextBox txtPhone = new TextBox { Left = 20, Top = 198, Width = 390, Text = location.ContactPhone };

            Label lblAddress = new Label { Left = 20, Top = 228, Width = 150, Text = "Street Address:" };
            TextBox txtAddress = new TextBox { Left = 20, Top = 250, Width = 390, Text = location.Address };

            Label lblCity = new Label { Left = 20, Top = 278, Width = 150, Text = "City:" };
            TextBox txtCity = new TextBox { Left = 20, Top = 300, Width = 180, Text = location.City };

            Label lblState = new Label { Left = 220, Top = 278, Width = 60, Text = "State:" };
            TextBox txtState = new TextBox { Left = 220, Top = 300, Width = 60, Text = location.State };

            Label lblZip = new Label { Left = 300, Top = 278, Width = 60, Text = "Zip:" };
            TextBox txtZip = new TextBox { Left = 300, Top = 300, Width = 80, Text = location.Zip };

            Label lblRate = new Label { Left = 20, Top = 330, Width = 150, Text = "Pay Rate ($):" };
            TextBox txtRate = new TextBox { Left = 20, Top = 350, Width = 100, Text = location.PayRate.ToString("F2") };

            Label lblType = new Label { Left = 130, Top = 330, Width = 100, Text = "Type:" };
            ComboBox cmbType = new ComboBox { Left = 130, Top = 350, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new string[] { "Per Hour", "Per Day" });
            cmbType.SelectedItem = location.PayRateType;

            Button btnSave = new Button { Text = "Save", Left = 250, Width = 80, Top = 345, DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = "Cancel", Left = 340, Width = 80, Top = 345, DialogResult = DialogResult.Cancel };

            dialog.Controls.AddRange(new Control[] { 
                lblFacility, txtFacility, lblContact, txtContact, lblEmail, txtEmail, 
                lblPhone, txtPhone, lblAddress, txtAddress, lblCity, txtCity, lblState, txtState, lblZip, txtZip,
                lblRate, txtRate, lblType, cmbType, 
                btnSave, btnCancel 
            });
            dialog.AcceptButton = btnSave;
            dialog.CancelButton = btnCancel;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string facilityName = txtFacility.Text.Trim();
                if (string.IsNullOrWhiteSpace(facilityName))
                {
                    MessageBox.Show("Please enter a facility name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtRate.Text, out decimal payRate))
                {
                    MessageBox.Show("Please enter a valid pay rate.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string payRateType = cmbType.SelectedItem.ToString() ?? "Per Hour";
                
                UpdateLocation(location.Id, facilityName, txtContact.Text.Trim(), txtEmail.Text.Trim(), 
                    txtPhone.Text.Trim(), txtAddress.Text.Trim(), txtCity.Text.Trim(), txtState.Text.Trim(), txtZip.Text.Trim(), payRate, payRateType);
                
                LoadLocations();
                RefreshLocationsList();
                MessageBox.Show("Location updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RefreshLocationsList()
        {
            foreach (TabPage tab in tabControl.TabPages)
            {
                foreach (Control ctrl in tab.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        foreach (Control grp in panel.Controls)
                        {
                            if (grp is GroupBox groupBox && groupBox.Text == "Locations List")
                            {
                                foreach (Control lstCtrl in groupBox.Controls)
                                {
                                    if (lstCtrl is ListView lst)
                                    {
                                        lst.Items.Clear();
                                        foreach (var loc in locations)
                                        {
                                            ListViewItem item = new ListViewItem(loc.FacilityName);
                                            item.Tag = loc.Id;
                                            item.SubItems.Add(loc.ContactName);
                                            item.SubItems.Add(loc.ContactPhone);
                                            item.SubItems.Add($"${loc.PayRate:F2} ({loc.PayRateType})");
                                            lst.Items.Add(item);
                                        }
                                    }
                                }
                            }
                            else if (grp is GroupBox grpBox2 && grpBox2.Text == "Add Time Entry")
                            {
                                foreach (Control ctrl2 in grpBox2.Controls)
                                {
                                    if (ctrl2 is ComboBox cmb && cmb.Name == "cmbLocation")
                                    {
                                        cmb.Items.Clear();
                                        cmb.Items.AddRange(locations.ToArray());
                                    }
                                }
                            }
                            else if (grp is GroupBox grpBox3 && grpBox3.Text == "Invoice Filter")
                            {
                                foreach (Control ctrl2 in grpBox3.Controls)
                                {
                                    if (ctrl2 is ComboBox cmb && cmb.Name == "cmbInvoiceLocation")
                                    {
                                        cmb.Items.Clear();
                                        cmb.Items.AddRange(locations.ToArray());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddTimeEntry(int locationId, string date, string arrivalTime, string departureTime, decimal dailyPay, string notes)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO TimeEntries (LocationId, Date, ArrivalTime, DepartureTime, DailyPay, Notes)
                VALUES ($locationId, $date, $arrivalTime, $departureTime, $dailyPay, $notes)";
            command.Parameters.AddWithValue("$locationId", locationId);
            command.Parameters.AddWithValue("$date", date);
            command.Parameters.AddWithValue("$arrivalTime", arrivalTime);
            command.Parameters.AddWithValue("$departureTime", departureTime);
            command.Parameters.AddWithValue("$dailyPay", (double)dailyPay);
            command.Parameters.AddWithValue("$notes", notes);
            command.ExecuteNonQuery();
        }

        private void LoadTimeEntries()
        {
            timeEntries.Clear();
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT t.Id, t.LocationId, l.FacilityName, t.Date, t.ArrivalTime, t.DepartureTime, t.DailyPay, t.Notes
                FROM TimeEntries t
                JOIN Locations l ON t.LocationId = l.Id
                ORDER BY t.Date DESC";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                timeEntries.Add(new TimeEntry
                {
                    Id = reader.GetInt32(0),
                    LocationId = reader.GetInt32(1),
                    LocationName = reader.GetString(2),
                    Date = reader.GetString(3),
                    ArrivalTime = reader.GetString(4),
                    DepartureTime = reader.GetString(5),
                    DailyPay = (decimal)reader.GetDouble(6),
                    Notes = reader.IsDBNull(7) ? "" : reader.GetString(7)
                });
            }
        }

        private void DeleteTimeEntry(int id)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM TimeEntries WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        private void ClearAllTimeEntries()
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM TimeEntries";
            command.ExecuteNonQuery();
        }

        private void RefreshTimeEntriesList()
        {
            foreach (TabPage tab in tabControl.TabPages)
            {
                foreach (Control ctrl in tab.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        foreach (Control grp in panel.Controls)
                        {
                            if (grp is GroupBox timeEntriesBox && timeEntriesBox.Text == "Time Entries")
                            {
                                foreach (Control lstCtrl in timeEntriesBox.Controls)
                                {
                                    if (lstCtrl is ListView lst)
                                    {
                                        lst.Items.Clear();
                                        decimal totalPay = 0;
                                        double totalHours = 0;

                                        foreach (var entry in timeEntries)
                                        {
                                            ListViewItem item = new ListViewItem(entry.Date);
                                            item.Tag = entry.Id;
                                            item.SubItems.Add(entry.LocationName);
                                            item.SubItems.Add(entry.ArrivalTime);
                                            item.SubItems.Add(entry.DepartureTime);

                                            var hours = CalculateHoursWorked(entry.ArrivalTime, entry.DepartureTime);
                                            item.SubItems.Add(hours.ToString("F2"));
                                            item.SubItems.Add($"${entry.DailyPay:F2}");
                                            item.SubItems.Add(entry.Notes);

                                            totalPay += entry.DailyPay;
                                            totalHours += hours;

                                            lst.Items.Add(item);
                                        }

                                        foreach (Control lblCtrl in timeEntriesBox.Controls)
                                        {
                                            if (lblCtrl is Label lbl && lbl.Name == "lblTotalHours")
                                                lbl.Text = $"Total Hours: {totalHours:F2}";
                                            if (lblCtrl is Label lbl2 && lbl2.Name == "lblTotalPay")
                                                lbl2.Text = $"Total Pay: ${totalPay:F2}";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateTotals()
        {
            decimal totalPay = 0;
            double totalHours = 0;

            foreach (var entry in timeEntries)
            {
                totalPay += entry.DailyPay;
                totalHours += CalculateHoursWorked(entry.ArrivalTime, entry.DepartureTime);
            }

            foreach (TabPage tab in tabControl.TabPages)
            {
                foreach (Control ctrl in tab.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        foreach (Control grp in panel.Controls)
                        {
                            if (grp is GroupBox groupBox && groupBox.Text == "Add Time Entry")
                            {
                                foreach (Control lblCtrl in groupBox.Controls)
                                {
                                    if (lblCtrl is Label lbl && lbl.Name == "lblTotalHours")
                                        lbl.Text = $"Total Hours: {totalHours:F2}";
                                    if (lblCtrl is Label lbl2 && lbl2.Name == "lblTotalPay")
                                        lbl2.Text = $"Total Pay: ${totalPay:F2}";
                                }
                            }
                        }
                    }
                }
            }
        }

        private decimal CalculateDailyPay(Location location, string arrival, string departure)
        {
            double hours = CalculateHoursWorked(arrival, departure);
            
            if (location.PayRateType == "Per Day")
            {
                return hours >= 8 ? location.PayRate : location.PayRate * (decimal)(hours / 8.0);
            }
            return location.PayRate * (decimal)hours;
        }

        private double CalculateHoursWorked(string arrival, string departure)
        {
            try
            {
                DateTime arrivalTime = DateTime.Parse(arrival);
                DateTime departureTime = DateTime.Parse(departure);
                
                if (departureTime < arrivalTime)
                {
                    departureTime = departureTime.AddDays(1);
                }
                
                TimeSpan diff = departureTime - arrivalTime;
                return diff.TotalHours;
            }
            catch
            {
                return 0;
            }
        }

        private void GenerateInvoice(Location location, string startDate, string endDate)
        {
            var entries = timeEntries.Where(e => 
                e.LocationId == location.Id &&
                string.Compare(e.Date, startDate) >= 0 &&
                string.Compare(e.Date, endDate) <= 0
            ).ToList();

            if (entries.Count == 0)
            {
                MessageBox.Show($"No time entries found for {location.FacilityName} in the selected date range.", 
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Invoice_{location.FacilityName}_{startDate.Replace("/", "-")}_to_{endDate.Replace("/", "-")}.xlsx"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Invoice");

                worksheet.Cell("A1").Value = "INVOICE";
                worksheet.Cell("A1").Style.Font.FontSize = 20;
                worksheet.Cell("A1").Style.Font.Bold = true;

                worksheet.Cell("A3").Value = "From:";
                worksheet.Cell("A3").Style.Font.Bold = true;
                worksheet.Cell("B3").Value = string.IsNullOrEmpty(businessSettings.BusinessName) ? "Your Company Name" : businessSettings.BusinessName;
                worksheet.Cell("A4").Value = "";
                worksheet.Cell("B4").Value = string.IsNullOrEmpty(businessSettings.BusinessAddress) ? "" : businessSettings.BusinessAddress;
                string cityStateZip = "";
                if (!string.IsNullOrEmpty(businessSettings.BusinessCity))
                    cityStateZip = businessSettings.BusinessCity;
                if (!string.IsNullOrEmpty(businessSettings.BusinessState))
                    cityStateZip += (cityStateZip.Length > 0 ? ", " : "") + businessSettings.BusinessState;
                if (!string.IsNullOrEmpty(businessSettings.BusinessZip))
                    cityStateZip += " " + businessSettings.BusinessZip;
                worksheet.Cell("B5").Value = cityStateZip;
                worksheet.Cell("B6").Value = string.IsNullOrEmpty(businessSettings.BusinessPhone) ? "" : businessSettings.BusinessPhone;
                worksheet.Cell("B7").Value = string.IsNullOrEmpty(businessSettings.BusinessEmail) ? "" : businessSettings.BusinessEmail;

                worksheet.Cell("D3").Value = "Bill To:";
                worksheet.Cell("D3").Style.Font.Bold = true;
                worksheet.Cell("E3").Value = location.FacilityName;
                worksheet.Cell("E4").Value = location.ContactName;
                worksheet.Cell("E5").Value = string.IsNullOrEmpty(location.Address) ? "" : location.Address;
                string locCityStateZip = "";
                if (!string.IsNullOrEmpty(location.City))
                    locCityStateZip = location.City;
                if (!string.IsNullOrEmpty(location.State))
                    locCityStateZip += (locCityStateZip.Length > 0 ? ", " : "") + location.State;
                if (!string.IsNullOrEmpty(location.Zip))
                    locCityStateZip += " " + location.Zip;
                worksheet.Cell("E6").Value = locCityStateZip;
                worksheet.Cell("E7").Value = location.ContactPhone;
                worksheet.Cell("E8").Value = location.ContactEmail;

                worksheet.Cell("A10").Value = "Invoice Date:";
                worksheet.Cell("B10").Value = DateTime.Now.ToString("MM/dd/yyyy");
                worksheet.Cell("A11").Value = "Period:";
                worksheet.Cell("B11").Value = $"{startDate} to {endDate}";

                worksheet.Cell("A14").Value = "Date";
                worksheet.Cell("B14").Value = "Arrival";
                worksheet.Cell("C14").Value = "Departure";
                worksheet.Cell("D14").Value = "Hours";
                worksheet.Cell("E14").Value = "Pay Rate";
                worksheet.Cell("F14").Value = "Amount";
                worksheet.Range("A14:F14").Style.Font.Bold = true;
                worksheet.Range("A14:F14").Style.Fill.BackgroundColor = XLColor.LightGray;

                decimal total = 0;
                int row = 15;
                foreach (var entry in entries.OrderBy(e => e.Date))
                {
                    double hours = CalculateHoursWorked(entry.ArrivalTime, entry.DepartureTime);
                    decimal rate = location.PayRateType == "Per Hour" ? location.PayRate : location.PayRate / 8;
                    
                    worksheet.Cell($"A{row}").Value = entry.Date;
                    worksheet.Cell($"B{row}").Value = entry.ArrivalTime;
                    worksheet.Cell($"C{row}").Value = entry.DepartureTime;
                    worksheet.Cell($"D{row}").Value = hours;
                    worksheet.Cell($"E{row}").Value = $"${rate:F2}";
                    worksheet.Cell($"F{row}").Value = entry.DailyPay;
                    
                    total += entry.DailyPay;
                    row++;
                }

                row++;
                worksheet.Cell($"E{row}").Value = "TOTAL:";
                worksheet.Cell($"E{row}").Style.Font.Bold = true;
                worksheet.Cell($"F{row}").Value = total;
                worksheet.Cell($"F{row}").Style.Font.Bold = true;

                row++;
                worksheet.Cell($"A{row + 2}").Value = "Notes:";
                foreach (var entry in entries.Where(e => !string.IsNullOrWhiteSpace(e.Notes)))
                {
                    row++;
                    worksheet.Cell($"A{row}").Value = $"{entry.Date}: {entry.Notes}";
                }

                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(saveDialog.FileName);
                MessageBox.Show($"Invoice saved to:\n{saveDialog.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                UpdateInvoicePreview(location, startDate, endDate, entries, total);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating invoice: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateInvoicePreview(Location location, string startDate, string endDate, List<TimeEntry> entries, decimal total)
        {
            string businessName = string.IsNullOrEmpty(businessSettings.BusinessName) ? "Your Company Name" : businessSettings.BusinessName;
            string businessAddress = string.IsNullOrEmpty(businessSettings.BusinessAddress) ? "" : businessSettings.BusinessAddress;
            string businessCityStateZip = "";
            if (!string.IsNullOrEmpty(businessSettings.BusinessCity))
                businessCityStateZip = businessSettings.BusinessCity;
            if (!string.IsNullOrEmpty(businessSettings.BusinessState))
                businessCityStateZip += (businessCityStateZip.Length > 0 ? ", " : "") + businessSettings.BusinessState;
            if (!string.IsNullOrEmpty(businessSettings.BusinessZip))
                businessCityStateZip += " " + businessSettings.BusinessZip;
            
            string preview = $"INVOICE\n";
            preview += $"========\n\n";
            preview += $"From: {businessName}\n";
            if (!string.IsNullOrEmpty(businessAddress))
                preview += $"{businessAddress}\n";
            if (!string.IsNullOrEmpty(businessCityStateZip))
                preview += $"{businessCityStateZip}\n";
            preview += $"\n";
            
            string locCityStateZip = "";
            if (!string.IsNullOrEmpty(location.City))
                locCityStateZip = location.City;
            if (!string.IsNullOrEmpty(location.State))
                locCityStateZip += (locCityStateZip.Length > 0 ? ", " : "") + location.State;
            if (!string.IsNullOrEmpty(location.Zip))
                locCityStateZip += " " + location.Zip;
            
            preview += $"Bill To:\n{location.FacilityName}\n{location.ContactName}\n";
            if (!string.IsNullOrEmpty(location.Address))
                preview += $"{location.Address}\n";
            if (!string.IsNullOrEmpty(locCityStateZip))
                preview += $"{locCityStateZip}\n";
            if (!string.IsNullOrEmpty(location.ContactPhone))
                preview += $"{location.ContactPhone}\n";
            if (!string.IsNullOrEmpty(location.ContactEmail))
                preview += $"{location.ContactEmail}\n";
            preview += $"\n";
            preview += $"Period: {startDate} to {endDate}\n\n";
            preview += $"Date       | Arrival  | Departure | Hours | Amount\n";
            preview += new string('-', 55) + "\n";
            
            foreach (var entry in entries.OrderBy(e => e.Date))
            {
                double hours = CalculateHoursWorked(entry.ArrivalTime, entry.DepartureTime);
                preview += $"{entry.Date} | {entry.ArrivalTime,-8} | {entry.DepartureTime,-9} | {hours:F2} | ${entry.DailyPay:F2}\n";
            }
            
            preview += new string('-', 55) + "\n";
            preview += $"{"",45} TOTAL: ${total:F2}\n";

            foreach (TabPage tab in tabControl.TabPages)
            {
                foreach (Control ctrl in tab.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        foreach (Control grp in panel.Controls)
                        {
                            if (grp is GroupBox groupBox && groupBox.Text == "Preview")
                            {
                                foreach (Control txtCtrl in groupBox.Controls)
                                {
                                    if (txtCtrl is TextBox txt && txt.Name == "txtInvoicePreview")
                                    {
                                        txt.Text = preview;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private class Location
        {
            public int Id { get; set; }
            public string FacilityName { get; set; }
            public string ContactName { get; set; }
            public string ContactEmail { get; set; }
            public string ContactPhone { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
            public decimal PayRate { get; set; }
            public string PayRateType { get; set; }
            
            public override string ToString() => FacilityName;
        }

        private class TimeEntry
        {
            public int Id { get; set; }
            public int LocationId { get; set; }
            public string LocationName { get; set; }
            public string Date { get; set; }
            public string ArrivalTime { get; set; }
            public string DepartureTime { get; set; }
            public decimal DailyPay { get; set; }
            public string Notes { get; set; }
        }

        private class BusinessSettings
        {
            public string BusinessName { get; set; }
            public string BusinessAddress { get; set; }
            public string BusinessCity { get; set; }
            public string BusinessState { get; set; }
            public string BusinessZip { get; set; }
            public string BusinessPhone { get; set; }
            public string BusinessEmail { get; set; }
        }
    }
}
