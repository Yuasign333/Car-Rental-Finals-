using System;
using System.Collections.Generic;
using System.IO;

namespace CarRentalSystem
{
    internal class FileHandler
    {
        // 📁 Directory and file paths
        private string baseDirectory;
        private string customerDirectory;
        private string agentDirectory;
        private string carsFilePath;
        private string customersFilePath;
        private string agentsFilePath;
        private string rentalsFilePath;
        private string maintenanceFilePath;

        // 🔧 Constructor - sets up directory structure
        public FileHandler()
        {
            // directories (folder creation)
            baseDirectory = "CarRentalData";
            customerDirectory = Path.Combine(baseDirectory, "Customers");
            agentDirectory = Path.Combine(baseDirectory, "Agents");

            // files (file creation)
            carsFilePath = Path.Combine(baseDirectory, "Cars.csv");
            customersFilePath = Path.Combine(baseDirectory, "Customers.csv");
            agentsFilePath = Path.Combine(baseDirectory, "Agents.csv");
            rentalsFilePath = Path.Combine(baseDirectory, "Rentals.csv");
            maintenanceFilePath = Path.Combine(baseDirectory, "Maintenance.csv");

            // methods to initialize directories and files
            InitializeDirectories();
            InitializeFiles();

        }

        // 📂 Create necessary directories
        private void InitializeDirectories()
        {
            try
            {
                if (!Directory.Exists(baseDirectory))
                {
                    Directory.CreateDirectory(baseDirectory);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Created directory: {baseDirectory}");
                    Console.ResetColor();
                }

                if (!Directory.Exists(customerDirectory))
                {
                    Directory.CreateDirectory(customerDirectory);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Created directory: {customerDirectory}");
                    Console.ResetColor();
                }

                if (!Directory.Exists(agentDirectory))
                {
                    Directory.CreateDirectory(agentDirectory);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Created directory: {agentDirectory}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error creating directories: {ex.Message}");
                Console.ResetColor();
            }
        }

        // 📄 Initialize files with default data if they don't exist
        private void InitializeFiles()
        {
            // Initialize Cars.csv with 10 default cars
            if (!File.Exists(carsFilePath))
            {
                List<Car> defaultCars = GetDefaultCars();
                SaveCars(defaultCars);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"✓ Initialized {carsFilePath} with 10 cars");
                Console.ResetColor();
            }

            // Initialize Customers.csv with default customer
            if (!File.Exists(customersFilePath))
            {
                List<Customer> defaultCustomers = new List<Customer>
                {
                    new Customer("C001", "John Doe", "customer123")
                };
                SaveCustomers(defaultCustomers);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"✓ Initialized {customersFilePath} with default customer");
                Console.ResetColor();
            }

            // Initialize Agents.csv with default admin
            if (!File.Exists(agentsFilePath))
            {
                List<CompanyAgent> defaultAgents = new List<CompanyAgent>
                {
                    new CompanyAgent("A001", "Admin", "admin123")
                };
                SaveAgents(defaultAgents);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"✓ Initialized {agentsFilePath} with default admin");
                Console.ResetColor();
            }

            // Create empty files if they don't exist
            if (!File.Exists(rentalsFilePath))
                File.Create(rentalsFilePath).Close();

            if (!File.Exists(maintenanceFilePath))
                File.Create(maintenanceFilePath).Close();
        }

        // 🚗 Get 10 default cars
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

        // ═══════════════════════════════════════════════════════════
        // 📖 READ OPERATIONS
        // ═══════════════════════════════════════════════════════════

        // 🚗 Read Cars
        public List<Car> ReadCars()
        {
            List<Car> cars = new List<Car>();

            if (!File.Exists(carsFilePath))
                return cars;

            try
            {
                using (StreamReader sr = new StreamReader(carsFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (Car.TryParseCsv(line, out Car car))
                        {
                            cars.Add(car);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error reading cars: {ex.Message}");
                Console.ResetColor();
            }

            return cars;
        }

        // 👤 Read Customers
        public List<Customer> ReadCustomers()
        {
            List<Customer> customers = new List<Customer>();

            if (!File.Exists(customersFilePath))
                return customers;

            try
            {
                using (StreamReader sr = new StreamReader(customersFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (Customer.TryParseCsv(line, out Customer customer))
                        {
                            customers.Add(customer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error reading customers: {ex.Message}");
                Console.ResetColor();
            }

            return customers;
        }

        // 🏢 Read Agents
        public List<CompanyAgent> ReadAgents()
        {
            List<CompanyAgent> agents = new List<CompanyAgent>();

            if (!File.Exists(agentsFilePath))
                return agents;

            try
            {
                using (StreamReader sr = new StreamReader(agentsFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (CompanyAgent.TryParseCsv(line, out CompanyAgent agent))
                        {
                            agents.Add(agent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error reading agents: {ex.Message}");
                Console.ResetColor();
            }

            return agents;
        }

        // 📋 Read Rentals
        public List<Rental> ReadRentals()
        {
            List<Rental> rentals = new List<Rental>();

            if (!File.Exists(rentalsFilePath))
                return rentals;

            try
            {
                using (StreamReader sr = new StreamReader(rentalsFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (Rental.TryParseCsv(line, out Rental rental))
                        {
                            rentals.Add(rental);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error reading rentals: {ex.Message}");
                Console.ResetColor();
            }

            return rentals;
        }

        // 🔧 Read Maintenance Records
        public List<Maintenance> ReadMaintenance()
        {
            List<Maintenance> maintenanceList = new List<Maintenance>();

            if (!File.Exists(maintenanceFilePath))
                return maintenanceList;

            try
            {
                using (StreamReader sr = new StreamReader(maintenanceFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (Maintenance.TryParseCsv(line, out Maintenance maintenance))
                        {
                            maintenanceList.Add(maintenance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error reading maintenance: {ex.Message}");
                Console.ResetColor();
            }

            return maintenanceList;
        }

        // ═══════════════════════════════════════════════════════════
        // 💾 WRITE OPERATIONS
        // ═══════════════════════════════════════════════════════════

        // 🚗 Save Cars
        public bool SaveCars(List<Car> cars)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(carsFilePath, false))
                {
                    foreach (Car car in cars)
                    {
                        sw.WriteLine(car.ToCsvString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error saving cars: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        // 👤 Save Customers
        public bool SaveCustomers(List<Customer> customers)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(customersFilePath, false))
                {
                    foreach (Customer customer in customers)
                    {
                        sw.WriteLine(customer.ToCsvString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error saving customers: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        // 🏢 Save Agents
        public bool SaveAgents(List<CompanyAgent> agents)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(agentsFilePath, false))
                {
                    foreach (CompanyAgent agent in agents)
                    {
                        sw.WriteLine(agent.ToCsvString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error saving agents: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        // 📋 Save Rentals
        public bool SaveRentals(List<Rental> rentals)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(rentalsFilePath, false))
                {
                    foreach (Rental rental in rentals)
                    {
                        sw.WriteLine(rental.ToCsvString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error saving rentals: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        // 🔧 Save Maintenance
        public bool SaveMaintenance(List<Maintenance> maintenanceList)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(maintenanceFilePath, false))
                {
                    foreach (Maintenance maintenance in maintenanceList)
                    {
                        sw.WriteLine(maintenance.ToCsvString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error saving maintenance: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        // 📄 Save customer receipt to their directory
        public bool SaveCustomerReceipt(string customerID, string receiptContent)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Receipt_{timestamp}.txt";
                string filePath = Path.Combine(customerDirectory, customerID, fileName);

                // Create customer subdirectory if it doesn't exist
                string customerFolder = Path.Combine(customerDirectory, customerID);
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
                Console.WriteLine($"✗ Error saving receipt: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        // 📊 Get file paths (for display purposes)
        public string GetCarsFilePath() { return Path.GetFullPath(carsFilePath); }
        public string GetCustomersFilePath() { return Path.GetFullPath(customersFilePath); }
        public string GetAgentsFilePath() { return Path.GetFullPath(agentsFilePath); }
        public string GetRentalsFilePath() { return Path.GetFullPath(rentalsFilePath); }
        public string GetMaintenanceFilePath() { return Path.GetFullPath(maintenanceFilePath); }
    }
}