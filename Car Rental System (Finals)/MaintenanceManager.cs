using System;
using System.Collections.Generic;

namespace CarRentalSystem
{
    internal class MaintenanceManager
    {
        private List<Car> cars;
        private List<Maintenance> maintenanceRecords;
        private FileHandler fileHandler;

        public MaintenanceManager(FileHandler fileHandler)
        {
            this.fileHandler = fileHandler;
            LoadData();
        }

        private void LoadData()
        {
            cars = fileHandler.ReadCars();
            maintenanceRecords = fileHandler.ReadMaintenance();
        }

        private void SaveData()
        {
            // This handles moving the Car file (C001.csv)
            fileHandler.SaveCars(cars);

            // This handles moving the Maintenance Record file (M0001.csv)
            fileHandler.SaveMaintenance(maintenanceRecords);
        }

        // Add new maintenance record
        public bool AddMaintenance(string carID, string technicianName, string description, out string message)
        {
            message = "";

            Car car = null;
            foreach (Car c in cars)
            {
                if (c.GetCarID() == carID)
                {
                    car = c;
                    break;
                }
            }

            if (car == null)
            {
                message = "Car not found";
                return false;
            }

            if (car.GetStatus() == "Rented")
            {
                message = "Cannot perform maintenance on rented car";
                return false;
            }

            if (car.GetStatus() == "Under Maintenance")
            {
                message = "Car is already under maintenance";
                return false;
            }

            string maintenanceID = "M" + (maintenanceRecords.Count + 1).ToString("D4");

            Maintenance maintenance = new Maintenance(maintenanceID, carID, technicianName, description);
            maintenanceRecords.Add(maintenance);

            car.SetMaintenance();

            SaveData();

            message = "Maintenance record added successfully";
            return true;
        }

        // Complete maintenance
        public bool CompleteMaintenance(string maintenanceID, out string message)
        {
            message = "";
            ReloadData(); // Make sure lists are fresh

            Maintenance maintenance = null;
            Car car = null;

            // 1. SIMPLE LOOP to find the Maintenance Record
            foreach (Maintenance m in maintenanceRecords)
            {
                // Use Trim to ignore any accidental spaces in the file or input
                if (m.GetMaintenanceID().Trim().Equals(maintenanceID.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    maintenance = m;
                    break;
                }
            }

            if (maintenance == null)
            {
                message = $"Error: Maintenance ID '{maintenanceID}' not found.";
                return false;
            }

            // 2. SIMPLE LOOP to find the Car linked to that record
            foreach (Car c in cars)
            {
                if (c.GetCarID() == maintenance.GetCarID())
                {
                    car = c;
                    break;
                }
            }

            if (car == null)
            {
                message = "Error: The car for this record no longer exists in the system.";
                return false;
            }

            // 3. Update the objects
            maintenance.CompleteMaintenance(); // Sets record to "Completed"
            car.ClearMaintenance();            // Sets car to "Available"

            // 4. Save changes (This triggers your folder move logic)
            SaveData();

            message = "Success! Maintenance completed and car is now Available.";
            return true;
        }

        // Get all maintenance records
        public List<Maintenance> GetAllMaintenance()
        {
            return maintenanceRecords;
        }

        // Get in-progress maintenance
        public List<Maintenance> GetInProgressMaintenance()
        {
            List<Maintenance> inProgress = new List<Maintenance>();

            foreach (Maintenance m in maintenanceRecords)
            {
                if (m.GetStatus() == "In Progress")
                {
                    inProgress.Add(m);
                }
            }

            return inProgress;
        }

        // Get maintenance history for a car
        public List<Maintenance> GetCarMaintenanceHistory(string carID)
        {
            List<Maintenance> carMaintenance = new List<Maintenance>();

            foreach (Maintenance m in maintenanceRecords)
            {
                if (m.GetCarID() == carID)
                {
                    carMaintenance.Add(m);
                }
            }

            return carMaintenance;
        }

        // Reload data
        public void ReloadData()
        {
            LoadData();
        }
    }
}
