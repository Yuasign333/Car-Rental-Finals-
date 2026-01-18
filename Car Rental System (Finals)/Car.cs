using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CarRentalSystem
{
    internal class Car
    {


        // program file path (personal laptop): C:\Users\yuanm\source\repos\Car Rental System (Finals)\Car Rental System (Finals)\

        // program file path (school desktop): 


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

        public void SetMaintenance()
        {
            // This MUST match the string in the if-statement above exactly
            this.status = "Under Maintenance";
        }

        public void ClearMaintenance()
        {
            this.status = "Available";
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
            // Truncate model name if too long (max 20 chars)

            string displayModel = model;
            if (displayModel.Length > 20)
            {
                displayModel = displayModel.Substring(0, 17) + "...";
            }

            // Truncate fuel type if too long (max 17 chars)
            string displayFuel = fuelType;
            if (displayFuel.Length > 17)
            {
                displayFuel = displayFuel.Substring(0, 14) + "...";
            }

            return carID.PadRight(8) + " | " +
                   displayModel.PadRight(20) + " | " +
                   category.PadRight(10) + " | " +
                   displayFuel.PadRight(17) + " | $" +
                   hourlyRate.ToString("F2") + "/hr";
        }
    }
}
