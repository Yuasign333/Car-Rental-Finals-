using System;
using System.Collections.Generic;
using System.IO;

namespace CarRentalSystem
{
    internal class FileHandler
    {


        // Directory paths
        private string baseDirectory;
        private string customerDirectory;
        private string adminDirectory;

        // Admin subdirectories
        private string carsAvailableDirectory;
        private string carsRentedDirectory;
        private string rentalsActiveDirectory;
        private string rentalsCompletedDirectory;
        private string maintenanceProgressDirectory;
        private string maintenanceCompletedDirectory;
        private string revenueDirectory;

        // File paths
        private string customersFilePath;
        private string agentsFilePath;

        public FileHandler()
        {
            baseDirectory = "CarRentalData";
            customerDirectory = Path.Combine(baseDirectory, "Customers");
            adminDirectory = Path.Combine(baseDirectory, "Admin");

            // Admin subdirectories
            carsAvailableDirectory = Path.Combine(adminDirectory, "Cars_Available");
            carsRentedDirectory = Path.Combine(adminDirectory, "Cars_Rented");
            rentalsActiveDirectory = Path.Combine(adminDirectory, "Rentals_Active");
            rentalsCompletedDirectory = Path.Combine(adminDirectory, "Rentals_Completed");
            maintenanceProgressDirectory = Path.Combine(adminDirectory, "Maintenance_InProgress");
            maintenanceCompletedDirectory = Path.Combine(adminDirectory, "Maintenance_Completed");
            revenueDirectory = Path.Combine(adminDirectory, "Revenue");

            // Simple file paths
            customersFilePath = Path.Combine(adminDirectory, "Customers.csv");
            agentsFilePath = Path.Combine(adminDirectory, "Agents.csv");

            InitializeDirectories();
            InitializeFiles();
        }

        private void InitializeDirectories()
        {
            try
            {
                string[] directories = {
                    baseDirectory,
                    customerDirectory,
                    adminDirectory,
                    carsAvailableDirectory,
                    carsRentedDirectory,
                    rentalsActiveDirectory,
                    rentalsCompletedDirectory,
                    maintenanceProgressDirectory,
                    maintenanceCompletedDirectory,
                    revenueDirectory
                };

                foreach (string dir in directories)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ Created directory: {dir}");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Error creating directories: {ex.Message}");
                Console.ResetColor();
            }
        }

        private void InitializeFiles()
        {
            //  Check if we have ANY cars at all (in either Available or Rented folders)
            // We call ReadCars() because it already looks in both places.
            List<Car> existingCars = ReadCars();

            if (existingCars.Count == 0)
            {
                List<Car> defaultCars = GetDefaultCars();
                foreach (Car car in defaultCars)
                {
                    string filePath = Path.Combine(carsAvailableDirectory, car.GetCarID() + ".csv");
                    // Only write if the system is truly empty
                    File.WriteAllText(filePath, car.ToCsvString());
                }
            }

            // Do the same for Customers and Agents (Check if file exists)
            if (!File.Exists(customersFilePath))
            {
                SaveCustomers(new List<Customer> { new Customer("C001", "John Doe", "customer123") });
            }

            if (!File.Exists(agentsFilePath))
            {
                SaveAgents(new List<CompanyAgent> { new CompanyAgent("A001", "Admin", "admin123") });
            }
        }

        private List<Car> GetDefaultCars()
        {
            return new List<Car>
            {
                new Car("C001", "Toyota RAV4", "SUV", "Dual Motor", 80m),
                new Car("C002", "Ford Explorer", "SUV", "Standard Engine", 75m),
                new Car("C003", "Tesla Model X", "SUV", "EV", 120m),
                new Car("C004", "Honda Civic", "Sedan", "Standard Engine", 45m),
                new Car("C005", "Tesla Model 3", "Sedan", "EV", 65m),
                new Car("C006", "Toyota Camry", "Sedan", "Dual Motor", 50m),
                new Car("C007", "Honda Odyssey", "Van", "Standard Engine", 90m),
                new Car("C008", "Chrysler Pacifica", "Van", "Dual Motor", 95m),
                new Car("C009", "Mercedes Sprinter", "Van", "Standard Engine", 110m),
                new Car("C010", "Nissan Altima", "Sedan", "Standard Engine", 48m)
            };
        }

        // READ CARS
        public List<Car> ReadCars()
        {
            List<Car> cars = new List<Car>();
            string[] folders = { carsAvailableDirectory, carsRentedDirectory, maintenanceProgressDirectory };

            foreach (string path in folders)
            {
                if (Directory.Exists(path))
                {
                    foreach (string file in Directory.GetFiles(path, "*.csv"))
                    {
                        //  Check the filename. If it starts with 'M', it's NOT a car.
                        string fileName = Path.GetFileName(file);
                        if (fileName.StartsWith("M", StringComparison.OrdinalIgnoreCase))
                            continue;

                        string line = File.ReadAllText(file);
                        if (Car.TryParseCsv(line, out Car car))
                        {
                            cars.Add(car);
                        }
                    }
                }
            }
            return cars;
        }
        public List<Customer> ReadCustomers()
        {
            List<Customer> customers = new List<Customer>();
            if (!File.Exists(customersFilePath))
                return customers;

            try
            {
                string[] lines = File.ReadAllLines(customersFilePath);
                foreach (string line in lines)
                {
                    if (Customer.TryParseCsv(line, out Customer customer))
                    {
                        customers.Add(customer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Error reading customers: {ex.Message}");
                Console.ResetColor();
            }

            return customers;
        }

        public List<CompanyAgent> ReadAgents()
        {
            List<CompanyAgent> agents = new List<CompanyAgent>();
            if (!File.Exists(agentsFilePath))
                return agents;

            try
            {
                string[] lines = File.ReadAllLines(agentsFilePath);
                foreach (string line in lines)
                {
                    if (CompanyAgent.TryParseCsv(line, out CompanyAgent agent))
                    {
                        agents.Add(agent);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Error reading agents: {ex.Message}");
                Console.ResetColor();
            }

            return agents;
        }

        public List<Rental> ReadRentals()
        {
            List<Rental> rentals = new List<Rental>();

            // Read Active Rentals
            if (Directory.Exists(rentalsActiveDirectory))
            {
                string[] files = Directory.GetFiles(rentalsActiveDirectory, "*.csv");
                foreach (string file in files)
                {
                    string line = File.ReadAllText(file);
                    if (Rental.TryParseCsv(line, out Rental rental))
                    {
                        rentals.Add(rental);
                    }
                }
            }

            // Read Completed Rentals
            if (Directory.Exists(rentalsCompletedDirectory))
            {
                string[] files = Directory.GetFiles(rentalsCompletedDirectory, "*.csv");
                foreach (string file in files)
                {
                    string line = File.ReadAllText(file);
                    if (Rental.TryParseCsv(line, out Rental rental))
                    {
                        rentals.Add(rental);
                    }
                }
            }

            return rentals;
        }

        public List<Maintenance> ReadMaintenance()
        {
            List<Maintenance> maintenanceList = new List<Maintenance>();

            // Read In Progress
            if (Directory.Exists(maintenanceProgressDirectory))
            {
                string[] files = Directory.GetFiles(maintenanceProgressDirectory, "*.csv");
                foreach (string file in files)
                {
                    string line = File.ReadAllText(file);
                    if (Maintenance.TryParseCsv(line, out Maintenance maintenance))
                    {
                        maintenanceList.Add(maintenance);
                    }
                }
            }

            // Read Completed
            if (Directory.Exists(maintenanceCompletedDirectory))
            {
                string[] files = Directory.GetFiles(maintenanceCompletedDirectory, "*.csv");
                foreach (string file in files)
                {
                    string line = File.ReadAllText(file);
                    if (Maintenance.TryParseCsv(line, out Maintenance maintenance))
                    {
                        maintenanceList.Add(maintenance);
                    }
                }
            }

            return maintenanceList;
        }

        // SAVE OPERATIONS
        public bool SaveCars(List<Car> cars)
        {
            try
            {
                // 1. CLEAR: Delete all existing files in car-related folders 
                // to ensure no "ghost" copies remain in the wrong place.
                string[] folders = { carsAvailableDirectory, carsRentedDirectory, maintenanceProgressDirectory };

                foreach (string folder in folders)
                {
                    if (Directory.Exists(folder))
                    {
                        string[] files = Directory.GetFiles(folder, "*.csv");
                        foreach (string file in files)
                        {
                            File.Delete(file);
                        }
                    }
                }

                // 2. ROUTE: Save each car to the specific folder it belongs in NOW
                foreach (Car car in cars)
                {
                    string fileName = car.GetCarID() + ".csv";
                    string path;

                    // Simple if-else logic for folder selection
                    if (car.GetStatus() == "Under Maintenance")
                    {
                        path = Path.Combine(maintenanceProgressDirectory, fileName);
                    }
                    else if (car.GetStatus() == "Rented")
                    {
                        path = Path.Combine(carsRentedDirectory, fileName);
                    }
                    else
                    {
                        path = Path.Combine(carsAvailableDirectory, fileName);
                    }

                    File.WriteAllText(path, car.ToCsvString());
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SaveCustomers(List<Customer> customers)
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (Customer customer in customers)
                {
                    lines.Add(customer.ToCsvString());
                }
                File.WriteAllLines(customersFilePath, lines);
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Error saving customers: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public bool SaveAgents(List<CompanyAgent> agents)
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (CompanyAgent agent in agents)
                {
                    lines.Add(agent.ToCsvString());
                }
                File.WriteAllLines(agentsFilePath, lines);
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Error saving agents: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public bool SaveRentals(List<Rental> rentals)
        {
            try
            {
                foreach (Rental rental in rentals)
                {
                    string fileName = rental.GetRentalID() + ".csv";
                    string filePath = "";

                    if (rental.GetStatus() == "Active")
                    {
                        filePath = Path.Combine(rentalsActiveDirectory, fileName);
                        // Delete from completed if exists
                        string completedPath = Path.Combine(rentalsCompletedDirectory, fileName);
                        if (File.Exists(completedPath))
                        {
                            File.Delete(completedPath);
                        }
                    }
                    else
                    {
                        filePath = Path.Combine(rentalsCompletedDirectory, fileName);
                        // Delete from active if exists
                        string activePath = Path.Combine(rentalsActiveDirectory, fileName);
                        if (File.Exists(activePath))
                        {
                            File.Delete(activePath);
                        }
                    }

                    File.WriteAllText(filePath, rental.ToCsvString());
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Error saving rentals: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public bool SaveMaintenance(List<Maintenance> maintenanceList)
        {
            try
            {
                foreach (Maintenance m in maintenanceList)
                {
                    // Use the Maintenance ID for the filename (M0001.csv)
                    string fileName = m.GetMaintenanceID() + ".csv";

                    // Define both potential paths
                    string inProgressPath = Path.Combine(maintenanceProgressDirectory, fileName);
                    string completedPath = Path.Combine(maintenanceCompletedDirectory, fileName);

                    if (m.GetStatus() == "In Progress")
                    {
                        // Save to 'In Progress' folder
                        File.WriteAllText(inProgressPath, m.ToCsvString());

                        // Remove from 'Completed' if it accidentally exists there
                        if (File.Exists(completedPath)) File.Delete(completedPath);
                    }
                    else if (m.GetStatus() == "Completed")
                    {
                        // Save to 'Completed' folder
                        File.WriteAllText(completedPath, m.ToCsvString());

                        // REMOVE FROM 'IN PROGRESS' folder (This is the move!)
                        if (File.Exists(inProgressPath)) File.Delete(inProgressPath);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Syncing Maintenance Files: {ex.Message}");
                return false;
            }
        }

        public bool SaveCustomerReceipt(string customerID, string receiptContent)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = "Receipt_" + timestamp + ".txt";
                string customerFolder = Path.Combine(customerDirectory, customerID);
                string filePath = Path.Combine(customerFolder, fileName);

                if (!Directory.Exists(customerFolder))
                {
                    Directory.CreateDirectory(customerFolder);
                }

                File.WriteAllText(filePath, receiptContent);
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Error saving receipt: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public bool SaveRevenueReport(string reportContent)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = "Revenue_" + timestamp + ".txt";
                string filePath = Path.Combine(revenueDirectory, fileName);

                File.WriteAllText(filePath, reportContent);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  ✓ Revenue report saved!");
                Console.ResetColor();

                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Error saving revenue report: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
    }
}
