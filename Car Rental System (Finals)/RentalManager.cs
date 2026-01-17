using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                if (c.GetCarID() == carID) // ✓ FIXED: Changed from 'car' to 'c'
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

        // ═══════════════════════════════════════════════════════════
        //  RENTAL OPERATIONS
        // ═══════════════════════════════════════════════════════════

        //  Get available cars (optionally filtered)
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

        //  Calculate rental estimate
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

        //  Confirm booking
        public bool ConfirmBooking(string customerID, string carID, int estimatedHours, out string rentalID)
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
                if (c.GetCarID() == carID) // ✓ FIXED: Changed from 'car' to 'c'
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

            Rental rental = new Rental(rentalID, customerID, carID, estimatedHours, car.GetHourlyRate());
            rentals.Add(rental);

            car.RentCar(customerID, estimatedHours);

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

            Car car = null;

            // Manual search for the car ID
            foreach (Car c in cars)
            {
                if (c.GetCarID() == carID)
                {
                    car = c;
                    break; // Stop searching once we find it
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

            Rental rental = rentals.FirstOrDefault(r =>
                r.GetCarID() == carID &&
                r.GetStatus() == "Active");

            if (rental == null)
            {
                message = "No active rental found for this car";
                return false;
            }

            int estimatedHours = rental.GetEstimatedHours();
            decimal hourlyRate = car.GetHourlyRate();

            // ═══════════════════════════════════════════════════════════
            // 💡 EARLY RETURN FORMULA
            // ═══════════════════════════════════════════════════════════

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
                message = $"Late return - extra charge: ${extraCharge.ToString("F2")}";
            }

            rental.CompleteRental(actualHours, finalCost);
            car.ReturnCar();
            SaveData();

            return true;
        }

        //  Get customer's active rentals
        public List<Rental> GetCustomerActiveRentals(string customerID)
        {
            //  Create a new empty list to hold the results
            List<Rental> activeRentals = new List<Rental>();

            //  Loop through every rental in your main list
            foreach (Rental r in rentals)
            {
                //  Check if the Customer ID matches AND the status is Active
                if (r.GetCustomerID() == customerID && r.GetStatus() == "Active")
                {
                    // 4. Add the matching rental to your new list
                    activeRentals.Add(r);
                }
            }

            // 5. Return the filtered list
            return activeRentals;
        }

        public void GenerateAdminProfitReport()
        {
            decimal totalRevenue = 0m;

            // Calculate total revenue from completed rentals
            foreach (var rental in rentals)
            {
                if (rental.GetStatus() == "Completed")
                {
                    totalRevenue += rental.GetFinalCost();
                }
            }

            string reportData = $"ADMIN REVENUE REPORT\nTotal: {totalRevenue:C}";

            // Call the file handler (it already knows to go to the Admin folder)
            fileHandler.SaveProfitReport(reportData);
        }

        // Get all rentals for a customer
        public List<Rental> GetCustomerRentals(string customerID)
        {
            return rentals.Where(r => r.GetCustomerID() == customerID).ToList();
        }

        //  Get car by ID
        public Car GetCarByID(string carID)
        {
            return cars.FirstOrDefault(c => c.GetCarID() == carID);
        }

        //  Get all cars
        public List<Car> GetAllCars()
        {
            return cars;
        }

        //  Reload data
        public void ReloadData()
        {
            LoadData();
        }
    }
}
