using System;
using System.Collections.Generic;

namespace CarRentalSystem
{
    internal class RentalManager
    {
        private List<Car> cars;
        private List<Rental> rentals;
        private FileHandler fileHandler;
        private decimal depositAmount = 50m;

        public RentalManager(FileHandler fileHandler)
        {
            this.fileHandler = fileHandler;
            LoadData();
        }

        private void LoadData()
        {
            cars = fileHandler.ReadCars();
            rentals = fileHandler.ReadRentals();
        }

        // ✅ FIX: This MUST save cars to update file locations
        private void SaveData()
        {
            fileHandler.SaveCars(cars);        // ← THIS MOVES FILES BETWEEN FOLDERS
            fileHandler.SaveRentals(rentals);
        }

        // Check if rental is possible
        public string CheckRentalConflict(string carID)
        {
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
                return "Car not found";
            }

            if (car.GetStatus() == "Rented")
            {
                return "Car is already rented";
            }

            if (car.GetStatus() == "Under Maintenance")
            {
                return "Car is under maintenance";
            }

            return "OK";
        }

        // Get available cars (optionally filtered)
        public List<Car> GetAvailableCars(string categoryFilter = "ALL", string fuelFilter = "ALL")
        {
            List<Car> availableCars = new List<Car>();

            foreach (Car c in cars)
            {
                if (c.GetStatus() == "Available")
                {
                    bool categoryMatch = (categoryFilter == "ALL" || c.GetCategory() == categoryFilter);
                    bool fuelMatch = (fuelFilter == "ALL" || c.GetFuelType() == fuelFilter);

                    if (categoryMatch && fuelMatch)
                    {
                        availableCars.Add(c);
                    }
                }
            }

            return availableCars;
        }

        // Calculate rental estimate
        public decimal CalculateEstimate(string carID, int hours, out decimal basePrice, out decimal deposit)
        {
            Car foundCar = null;

            foreach (Car c in cars)
            {
                if (c.GetCarID() == carID)
                {
                    foundCar = c;
                    break;
                }
            }

            if (foundCar == null)
            {
                basePrice = 0;
                deposit = 0;
                return 0;
            }

            basePrice = foundCar.GetHourlyRate() * hours;
            deposit = depositAmount;

            return basePrice + deposit;
        }

        // Check if driver name already exists for this customer
        public bool IsDriverNameTaken(string customerID, string driverName)
        {
            foreach (Rental rental in rentals)
            {
                if (rental.GetCustomerID() == customerID && rental.GetDriverName() == driverName)
                {
                    return true;
                }
            }
            return false;
        }

        // ✅ FIX: Confirm booking - NOW SAVES TO FILES
        public bool ConfirmBooking(string customerID, string carID, string driverName, int estimatedHours, out string rentalID)
        {
            rentalID = "";

            string conflict = CheckRentalConflict(carID);
            if (conflict != "OK")
            {
                return false;
            }

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
                return false;
            }

            rentalID = "R" + (rentals.Count + 1).ToString("D4");

            // Create rental record
            Rental rental = new Rental(rentalID, customerID, carID, driverName, estimatedHours, car.GetHourlyRate());
            rentals.Add(rental);

            // ✅ UPDATE CAR STATUS IN MEMORY
            car.RentCar(customerID, estimatedHours);

            // ✅ CRITICAL FIX: SAVE TO FILES
            // This will move the car from Cars_Available to Cars_Rented folder
            SaveData();

            return true;
        }

        // ✅ FIX: Process return - NOW PROPERLY UPDATES FILES
        public bool ProcessReturn(string carID, int actualHours, out decimal finalCost, out decimal discount, out string message)
        {
            finalCost = 0;
            discount = 0;
            message = "";

            // ✅ RELOAD DATA TO GET LATEST STATE FROM FILES
            LoadData();

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

            if (car.GetStatus() != "Rented")
            {
                message = "Car is not currently rented";
                return false;
            }

            Rental rental = null;

            foreach (Rental r in rentals)
            {
                if (r.GetCarID() == carID && r.GetStatus() == "Active")
                {
                    rental = r;
                    break;
                }
            }

            if (rental == null)
            {
                message = "No active rental found for this car";
                return false;
            }

            int estimatedHours = rental.GetEstimatedHours();
            decimal hourlyRate = car.GetHourlyRate();

            // EARLY RETURN FORMULA
            if (actualHours < estimatedHours)
            {
                int usedHours = actualHours;
                int unusedHours = estimatedHours - actualHours;

                decimal usedCost = usedHours * hourlyRate;
                decimal unusedCost = unusedHours * hourlyRate;
                discount = unusedCost * 0.20m;

                finalCost = usedCost + (unusedCost - discount);
                message = "Early return! $" + discount.ToString("F2") + " discount applied";
            }
            else if (actualHours == estimatedHours)
            {
                finalCost = actualHours * hourlyRate;
                message = "On-time return";
            }
            else
            {
                finalCost = actualHours * hourlyRate;
                decimal extraCharge = (actualHours - estimatedHours) * hourlyRate;
                message = "Late return - extra charge: $" + extraCharge.ToString("F2");
            }

            //  UPDATE RENTAL RECORD
            rental.CompleteRental(actualHours, finalCost);

            //  UPDATE CAR STATUS IN MEMORY
            car.ReturnCar();

    
            // This will move the car from Cars_Rented BACK to Cars_Available folder
            SaveData();

            return true;
        }

        // Get customer's active rentals
        public List<Rental> GetCustomerActiveRentals(string customerID)
        {
            List<Rental> activeRentals = new List<Rental>();

            foreach (Rental r in rentals)
            {
                if (r.GetCustomerID() == customerID && r.GetStatus() == "Active")
                {
                    activeRentals.Add(r);
                }
            }

            return activeRentals;
        }

        // Get all rentals for a customer
        public List<Rental> GetCustomerRentals(string customerID)
        {
            List<Rental> customerRentals = new List<Rental>();

            foreach (Rental r in rentals)
            {
                if (r.GetCustomerID() == customerID)
                {
                    customerRentals.Add(r);
                }
            }

            return customerRentals;
        }

        // Get car by ID
        public Car GetCarByID(string carID)
        {
            foreach (Car c in cars)
            {
                if (c.GetCarID() == carID)
                {
                    return c;
                }
            }
            return null;
        }

        // Get all cars
        public List<Car> GetAllCars()
        {
            return cars;
        }

        //  FIX: Reload data from files
        public void ReloadData()
        {
            LoadData();
        }
    }
}
