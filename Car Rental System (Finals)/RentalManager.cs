using System;
using System.Collections.Generic;
using System.Linq;

namespace CarRentalSystem
{
    internal class RentalManager // Manages rental operations
    {
        private List<Car> cars;
        private List<Rental> rentals;
        private FileHandler fileHandler;
        private decimal depositAmount = 50m;

        //  Constructor
        public RentalManager(FileHandler fileHandler)
        {
            this.fileHandler = fileHandler;
            LoadData();
        }

        //  Load data from files
        private void LoadData()
        {
            cars = fileHandler.ReadCars();
            rentals = fileHandler.ReadRentals();
        }

        //  Save data to files
        private void SaveData()
        {
            fileHandler.SaveCars(cars);
            fileHandler.SaveRentals(rentals);
        }

        // ═══════════════════════════════════════════════════════════
        //  CONFLICT ENGINE - Checks if rental is possible
        // ═══════════════════════════════════════════════════════════

        public string CheckRentalConflict(string carID)
        {
            Car car = null;

            foreach (Car c in cars)
            {
                if (car.GetCarID() == carID)
                {
                    car = c;
                    break; // Stop searching once we find it
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

            return "OK"; // No conflict
        }

        // ═══════════════════════════════════════════════════════════
        //  RENTAL OPERATIONS
        // ═══════════════════════════════════════════════════════════

        //  Get available cars (optionally filtered)
        public List<Car> GetAvailableCars(string categoryFilter = "ALL", string fuelFilter = "ALL")
        {
            List<Car> availableCars = new List<Car>();

            foreach (Car c in cars)
            {
                // 1. Check if the car is available first
                if (c.GetStatus() == "Available")
                {
                    // 2. Check Category (True if ALL or if it matches)
                    bool categoryMatch = (categoryFilter == "ALL" || c.GetCategory() == categoryFilter);

                    // 3. Check Fuel (True if ALL or if it matches)
                    bool fuelMatch = (fuelFilter == "ALL" || c.GetFuelType() == fuelFilter);

                    // If it passes all checks, add it to our list
                    if (categoryMatch && fuelMatch)
                    {
                        availableCars.Add(c);
                    }
                }
            }

            return availableCars;
        }

        //  Calculate rental estimate
        public decimal CalculateEstimate(string carID, int hours, out decimal basePrice, out decimal deposit)
        {
            Car foundCar = null;

        
            foreach (Car c in cars)
            {
                if (c.GetCarID() == carID)
                {
                    foundCar = c;
                    break; // Stop searching once we find it
                }
            }

            // Handle car not found
            if (foundCar == null)
            {
                basePrice = 0;
                deposit = 0;
                return 0;
            }

            // Logic for calculation
            basePrice = (decimal)foundCar.GetHourlyRate() * hours;
            deposit = (decimal)depositAmount;

            return basePrice + deposit;
        }


        //  Confirm booking
        public bool ConfirmBooking(string customerID, string carID, int estimatedHours, out string rentalID)
        {
            rentalID = "";

            // Check for conflicts
            string conflict = CheckRentalConflict(carID);
            if (conflict != "OK")
            {
                return false;
            }
              Car foundCar = null;

        
            foreach (Car c in cars)
            {
                if (c.GetCarID() == carID)
                {
                    foundCar = c;
                    break; // Stop searching once we find it
                }
            }

            Car car = null;

            foreach (Car c in cars)
            {
                if (car.GetCarID() == carID)
                {
                    car = c;
                    break; // Stop searching once we find it
                }
            }

           if ( car == null)
            {
                return false; // Car not found
            }
            else
            {
               // Car found
            }

            // Generate rental ID
            rentalID = "R" + (rentals.Count + 1).ToString("D4");

            // Create rental record
            Rental rental = new Rental(rentalID, customerID, carID, estimatedHours, car.GetHourlyRate());
            rentals.Add(rental);

            // Update car status
            car.RentCar(customerID, estimatedHours);

            // Save changes
            SaveData();

            return true;
        }

        // ═══════════════════════════════════════════════════════════
        //  RETURN OPERATIONS WITH EARLY RETURN FORMULA
        // ═══════════════════════════════════════════════════════════

        //  Process return with early return discount
        public bool ProcessReturn(string carID, int actualHours, out decimal finalCost, out decimal discount, out string message)
        {
            finalCost = 0;
            discount = 0;
            message = "";

            // Find the car
            Car car = cars.FirstOrDefault(c => c.GetCarID() == carID);
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

            // Find active rental
            Rental rental = rentals.FirstOrDefault(r =>
                r.GetCarID() == carID &&
                r.GetStatus() == "Active");

            if (rental == null)
            {
                message = "No active rental found for this car";
                return false;
            }

            // Get rental details
            int estimatedHours = rental.GetEstimatedHours();
            decimal hourlyRate = car.GetHourlyRate();

            // ═══════════════════════════════════════════════════════════
            // 💡 EARLY RETURN FORMULA
            // ═══════════════════════════════════════════════════════════
            // If returned early:
            // - Charge for actual hours used
            // - Apply 20% discount on unused hours
            // - Final = (actual × rate) + (unused × rate × 0.8)
            //
            // If returned late or on-time:
            // - Charge for actual hours (may exceed estimate)
            // ═══════════════════════════════════════════════════════════

            if (actualHours < estimatedHours)
            {
                // Early return - calculate discount
                int usedHours = actualHours;
                int unusedHours = estimatedHours - actualHours;

                decimal usedCost = usedHours * hourlyRate;
                decimal unusedCost = unusedHours * hourlyRate;
                discount = unusedCost * 0.20m; // 20% discount on unused time

                finalCost = usedCost + (unusedCost - discount);
                message = $"Early return! {discount:C} discount applied";
            }
            else if (actualHours == estimatedHours)
            {
                // On-time return
                finalCost = actualHours * hourlyRate;
                message = "On-time return";
            }
            else
            {
                // Late return - charge for all hours
                finalCost = actualHours * hourlyRate;
                decimal extraCharge = (actualHours - estimatedHours) * hourlyRate;
                message = $"Late return - extra charge: {extraCharge:C}";
            }

            // Update rental record
            rental.CompleteRental(actualHours, finalCost);

            // Return the car
            car.ReturnCar();

            // Save changes
            SaveData();

            return true;
        }

        // 📊 Get customer's active rentals
        public List<Rental> GetCustomerActiveRentals(string customerID)
        {
            return rentals.Where(r =>
                r.GetCustomerID() == customerID &&
                r.GetStatus() == "Active").ToList();
        }

        // 📋 Get all rentals for a customer
        public List<Rental> GetCustomerRentals(string customerID)
        {
            return rentals.Where(r => r.GetCustomerID() == customerID).ToList();
        }

        // 🚗 Get car by ID
        public Car GetCarByID(string carID)
        {
            return cars.FirstOrDefault(c => c.GetCarID() == carID);
        }

        // 📊 Get all cars
        public List<Car> GetAllCars()
        {
            return cars;
        }

        // 🔄 Reload data
        public void ReloadData()
        {
            LoadData();
        }
    }
}