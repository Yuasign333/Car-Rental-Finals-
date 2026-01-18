using System;

namespace CarRentalSystem
{
    internal class Rental
    {
        private string rentalID;
        private string customerID;
        private string carID;
        private string driverName;
        private DateTime rentalStartTime;
        private DateTime rentalEndTime;
        private int estimatedHours;
        private int actualHours;
        private decimal totalCost;
        private string status;

        // Constructor for new rental
        public Rental(string rentalID, string customerID, string carID, string driverName, int estimatedHours, decimal hourlyRate)
        {
            this.rentalID = rentalID;
            this.customerID = customerID;
            this.carID = carID;
            this.driverName = driverName;
            this.rentalStartTime = DateTime.Now;
            this.rentalEndTime = DateTime.MinValue;
            this.estimatedHours = estimatedHours;
            this.actualHours = 0;
            this.totalCost = estimatedHours * hourlyRate;
            this.status = "Active";
        }

        // Constructor for existing rental (from CSV)
        public Rental(string rentalID, string customerID, string carID, string driverName, DateTime startTime,
                     DateTime endTime, int estimatedHours, int actualHours, decimal totalCost, string status)
        {
            this.rentalID = rentalID;
            this.customerID = customerID;
            this.carID = carID;
            this.driverName = driverName;
            this.rentalStartTime = startTime;
            this.rentalEndTime = endTime;
            this.estimatedHours = estimatedHours;
            this.actualHours = actualHours;
            this.totalCost = totalCost;
            this.status = status;
        }

        // Parse from CSV
        public static bool TryParseCsv(string csvLine, out Rental rental)
        {
            rental = null;
            try
            {
                string[] parts = csvLine.Split(',');
                if (parts.Length < 10)
                    return false;

                string id = parts[0].Trim();
                string custID = parts[1].Trim();
                string carID = parts[2].Trim();
                string driverName = parts[3].Trim();
                DateTime startTime = DateTime.Parse(parts[4].Trim());
                DateTime endTime = parts[5].Trim() != "" ? DateTime.Parse(parts[5].Trim()) : DateTime.MinValue;
                int estHours = int.Parse(parts[6].Trim());
                int actHours = int.Parse(parts[7].Trim());
                decimal cost = decimal.Parse(parts[8].Trim());
                string status = parts[9].Trim();

                rental = new Rental(id, custID, carID, driverName, startTime, endTime, estHours, actHours, cost, status);
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
            string endTimeStr = rentalEndTime == DateTime.MinValue ? "" : rentalEndTime.ToString("yyyy-MM-dd HH:mm:ss");
            return rentalID + "," + customerID + "," + carID + "," + driverName + "," +
                   rentalStartTime.ToString("yyyy-MM-dd HH:mm:ss") + "," + endTimeStr + "," +
                   estimatedHours + "," + actualHours + "," + totalCost + "," + status;
        }

        // Complete the rental
        public void CompleteRental(int actualHours, decimal finalCost)
        {
            this.rentalEndTime = DateTime.Now;
            this.actualHours = actualHours;
            this.totalCost = finalCost;
            this.status = "Completed";
        }

        // Getters
        public string GetRentalID() { return rentalID; }
        public string GetCustomerID() { return customerID; }
        public string GetCarID() { return carID; }
        public string GetDriverName() { return driverName; }
        public DateTime GetRentalStartTime() { return rentalStartTime; }
        public DateTime GetRentalEndTime() { return rentalEndTime; }
        public int GetEstimatedHours() { return estimatedHours; }
        public int GetActualHours() { return actualHours; }
        public decimal GetTotalCost() { return totalCost; }
        public string GetStatus() { return status; }
        public decimal GetFinalCost() { return totalCost; }
    }
}
