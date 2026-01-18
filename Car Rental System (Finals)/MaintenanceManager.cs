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

            Maintenance maintenance = null;
            foreach (Maintenance m in maintenanceRecords)
            {
                if (m.GetMaintenanceID() == maintenanceID && m.GetStatus() == "In Progress")
                {
                    maintenance = m;
                    break;
                }
            }

            if (maintenance == null)
            {
                message = "Maintenance record not found or already completed";
                return false;
            }

            Car car = null;
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
                message = "Car not found";
                return false;
            }

            maintenance.CompleteMaintenance();
            car.ClearMaintenance();

            SaveData();

            message = "Maintenance completed successfully";
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
