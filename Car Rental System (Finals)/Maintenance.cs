using System;

namespace CarRentalSystem
{
    internal class Maintenance // Represents a maintenance record for a car
    {
        //  Properties
        private string maintenanceID;
        private string carID;
        private string technicianName;
        private DateTime maintenanceDate;
        private string description;
        private string status; // "In Progress", "Completed"

        //  Constructor for new maintenance
        public Maintenance(string maintenanceID, string carID, string technicianName, string description)
        {
            this.maintenanceID = maintenanceID;
            this.carID = carID;
            this.technicianName = technicianName;
            this.maintenanceDate = DateTime.Now;
            this.description = description;
            this.status = "In Progress";
        }

        //  Constructor for existing maintenance (from CSV)
        public Maintenance(string maintenanceID, string carID, string technicianName,
                          DateTime date, string description, string status)
        {
            this.maintenanceID = maintenanceID;
            this.carID = carID;
            this.technicianName = technicianName;
            this.maintenanceDate = date;
            this.description = description;
            this.status = status;
        }

        // Parse from CSV
        public static bool TryParseCsv(string csvLine, out Maintenance maintenance)
        {
            maintenance = null;
            try
            {
                string[] parts = csvLine.Split(',');
                if (parts.Length < 6)
                    return false;

                string id = parts[0].Trim();
                string carID = parts[1].Trim();
                string techName = parts[2].Trim();
                DateTime date = DateTime.Parse(parts[3].Trim());
                string desc = parts[4].Trim();
                string status = parts[5].Trim();

                maintenance = new Maintenance(id, carID, techName, date, desc, status);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Convert to CSV
        public string ToCsvString()
        {
            return $"{maintenanceID},{carID},{technicianName},{maintenanceDate:yyyy-MM-dd HH:mm:ss},{description},{status}";
        }

        // Complete maintenance
        public void CompleteMaintenance()
        {
            status = "Completed";
        }

        // Getters Methods
        public string GetMaintenanceID() { return maintenanceID; }
        public string GetCarID() { return carID; }
        public string GetTechnicianName() { return technicianName; }
        public DateTime GetMaintenanceDate() { return maintenanceDate; }
        public string GetDescription() { return description; }
        public string GetStatus() { return status; }
    }
}