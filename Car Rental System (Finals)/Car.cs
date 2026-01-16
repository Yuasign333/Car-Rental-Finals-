using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CarRentalSystem
{
    internal class Car
    {
        //  Properties for Car
        private string carID;
        private string model;
        private string category; // fixed of SUV, Sedan, Van
        private string fuelType; // Dual Motor, Standard Engine, EV
        private decimal hourlyRate;
        private string status;         // Available, Rented, Under Maintenance

        // Rental tracking
        private string currentRenterID;
        private DateTime rentalStartTime;
        private int estimatedHours;

        // Constructor for Car overall details
        public Car(string id, string model, string category, string fuelType, decimal hourlyRate, string status = "Available")
        {
            this.carID = id;
            this.model = model;
            this.category = category;
            this.fuelType = fuelType;
            this.hourlyRate = hourlyRate;
            this.status = status;
            this.currentRenterID = "";
            this.rentalStartTime = DateTime.MinValue;
            this.estimatedHours = 0;
        }


        //  Parse from CSV line
        public static bool TryParseCsv(string csvLine, out Car car)
        {
            car = null;

            try
            {
                string[] parts = csvLine.Split(','); // Split by comma

                if (parts.Length < 6) // Minimum required fields
                {
                    return false;
                }
                else
                {
                    // extract rental info if present
                    string id = parts[0].Trim();
                    string model = parts[1].Trim();
                    string category = parts[2].Trim();
                    string fuelType = parts[3].Trim();
                    decimal rate = decimal.Parse(parts[4].Trim());
                    string status = parts[5].Trim();

                    car = new Car(id, model, category, fuelType, rate, status); // Create car object
                }
         

                // If there's rental info, parse it

                if (parts.Length >= 9 && !string.IsNullOrEmpty(parts[6])) // Rental info present
                {
                    car.currentRenterID = parts[6].Trim(); // Renter ID
                    car.rentalStartTime = DateTime.Parse(parts[7].Trim()); // Rental start time
                    car.estimatedHours = int.Parse(parts[8].Trim()); // Estimated hours
                }

                return true; // Successfully parsed
            }
            catch
            {
                return false; // Parsing failed
            }
        }

        //  Convert to CSV string ( we need It for saving to file)
        public string ToCsvString()
        {
            string rentalInfo = "";

            if (!string.IsNullOrEmpty(currentRenterID))
            {
                rentalInfo = $",{currentRenterID},{rentalStartTime:yyyy-MM-dd HH:mm:ss},{estimatedHours}"; // Rental info
            }
            else
            {
                rentalInfo = ",,,"; // Empty rental info
            }

            return $"{carID},{model},{category},{fuelType},{hourlyRate},{status}{rentalInfo}"; // Full CSV line
        }

        // Rent the car
        public bool RentCar(string renterID, int hours)
        {
            if (status != "Available") // Car not available
            {
                return false; // Cannot rent
            }

            else // Rent the car
            {

                status = "Rented";
                currentRenterID = renterID;
                rentalStartTime = DateTime.Now;
                estimatedHours = hours;
                return true; // Successfully rented
            }
        }

        // Return the car
        public void ReturnCar()
        {
            status = "Available";
            currentRenterID = "";
            rentalStartTime = DateTime.MinValue;
            estimatedHours = 0;
        }

        // Set maintenance status
        public void SetMaintenance()
        {
            status = "Under Maintenance";
        }

        //  Clear maintenance status
        public void ClearMaintenance()
        {
            status = "Available";
        }

        //  Getter Methods
        public string GetCarID() { return carID; }
        public string GetModel() { return model; }
        public string GetCategory() { return category; }
        public string GetFuelType() { return fuelType; }
        public decimal GetHourlyRate() { return hourlyRate; }
        public string GetStatus() { return status; }
        public string GetCurrentRenterID() { return currentRenterID; }
        public DateTime GetRentalStartTime() { return rentalStartTime; }
        public int GetEstimatedHours() { return estimatedHours; }

        // Display car info (formatted for table) (reflected on the console)
        public string ToTableRow()
        {
            return $"{carID,-4} | {model,-15} | {category,-8} | {fuelType,-13} | ${hourlyRate,-6}/hr";
        }
    }
}