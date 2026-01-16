using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem
{
    // Customer Class
    internal class Customer
    {
        private string customerID;
        private string name;
        private string password;

        // Constructor
        public Customer(string id, string name, string password)
        {
            this.customerID = id;
            this.name = name;
            this.password = password;
        }

        // Parse from CSV
        public static bool TryParseCsv(string csvLine, out Customer customer)
        {
            customer = null;
            try
            {
                string[] parts = csvLine.Split(',');
                if (parts.Length < 3)
                    return false;

                string id = parts[0].Trim();
                string name = parts[1].Trim();
                string password = parts[2].Trim();

                customer = new Customer(id, name, password);
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
            return $"{customerID},{name},{password}";
        }

        //  Validate password
        public bool ValidatePassword(string inputPassword)
        {
            return password == inputPassword;
        }

        // Getters Methods
        public string GetCustomerID() { return customerID; }
        public string GetName() { return name; }
    }
}