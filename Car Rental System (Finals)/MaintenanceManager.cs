using System;
using System.Collections.Generic;
using System.Linq;

namespace CarRentalSystem
{
    internal class MaintenanceManager // Manages maintenance operations for cars
    {
        private List<Car> cars;
        private List<Maintenance> maintenanceRecords;
        private FileHandler fileHandler;

        //  Constructor
        public MaintenanceManager(FileHandler fileHandler)
        {
            this.fileHandler = fileHandler;
            LoadData();
        }

        //  Load data from files
        private void LoadData()
        {
            cars = fileHandler.ReadCars();
            maintenanceRecords = fileHandler.ReadMaintenance();
        }

        //  Save data to files
        private void SaveData()
        {
            fileHandler.SaveCars(cars);
            fileHandler.SaveMaintenance(maintenanceRecords);
        }

        // ═══════════════════════════════════════════════════════════
        //  MAINTENANCE OPERATIONS
        // ═══════════════════════════════════════════════════════════

        //  Add new maintenance record
        public bool AddMaintenance(string carID, string technicianName, string description, out string message)
        {
            message = "";

            // Find the car
            Car car = cars.FirstOrDefault(c => c.GetCarID() == carID);
            if (car == null)
            {
                message = "Car not found";
                return false;
            }

            // Check if car is rented
            if (car.GetStatus() == "Rented")
            {
                message = "Cannot perform maintenance on rented car";
                return false;
            }

            // Check if already under maintenance
            if (car.GetStatus() == "Under Maintenance")
            {
                message = "Car is already under maintenance";
                return false;
            }

            // Generate maintenance ID
            string maintenanceID = "M" + (maintenanceRecords.Count + 1).ToString("D4");

            // Create maintenance record
            Maintenance maintenance = new Maintenance(maintenanceID, carID, technicianName, description);
            maintenanceRecords.Add(maintenance);

            // Update car status
            car.SetMaintenance();

            // Save changes
            SaveData();

            message = "Maintenance record added successfully";
            return true;
        }

        // Complete maintenance
        public bool CompleteMaintenance(string maintenanceID, out string message)
        {
            message = "";

            // Find the maintenance record
            Maintenance maintenance = maintenanceRecords.FirstOrDefault(m =>
                m.GetMaintenanceID() == maintenanceID &&
                m.GetStatus() == "In Progress");

            if (maintenance == null)
            {
                message = "Maintenance record not found or already completed";
                return false;
            }

            // Find the car
            Car car = cars.FirstOrDefault(c => c.GetCarID() == maintenance.GetCarID());
            if (car == null)
            {
                message = "Car not found";
                return false;
            }

            // Complete maintenance
            maintenance.CompleteMaintenance();

            // Update car status
            car.ClearMaintenance();

            // Save changes
            SaveData();

            message = "Maintenance completed successfully";
            return true;
        }

        //  Get all maintenance records
        public List<Maintenance> GetAllMaintenance()
        {
            return maintenanceRecords;
        }

        //  Get in-progress maintenance
        public List<Maintenance> GetInProgressMaintenance()
        {
            return maintenanceRecords.Where(m => m.GetStatus() == "In Progress").ToList();
        }

        //  Get maintenance history for a car
        public List<Maintenance> GetCarMaintenanceHistory(string carID)
        {
            return maintenanceRecords.Where(m => m.GetCarID() == carID).ToList();
        }

        //  Reload data
        public void ReloadData()
        {
            LoadData();
        }
    }
}