using System;
using System.Collections.Generic;
using System.IO;

namespace CarRentalSystem
{
    internal class FileHandler
    {
        //full path of the program: C:\Users\yuanm\source\repos\Car Rental System (Finals)\Car Rental System (Finals)\

        // Directory paths
        private string baseDirectory;
        private string customerDirectory;
        private string agentDirectory;
        private string adminDirectory;
        private string adminReportsDirectory;

        // File paths
        private string carsFilePath;
        private string customersFilePath;
        private string agentsFilePath;
        private string rentalsFilePath;
        private string maintenanceFilePath;

        public FileHandler()
        {
            // Initialize directories using Path.Combine
            baseDirectory = "CarRentalData";
            customerDirectory = Path.Combine(baseDirectory, "Customers");     
            adminDirectory = Path.Combine(baseDirectory, "Admin");

            // admin directory contains the following sub-directories
            adminReportsDirectory = Path.Combine(adminDirectory, "Reports"); 
            agentsFilePath = Path.Combine(adminDirectory, "Agents.csv");
            carsFilePath = Path.Combine(adminDirectory, "Cars.csv");
            customersFilePath = Path.Combine(adminDirectory, "Customers.csv");    
            rentalsFilePath = Path.Combine(adminDirectory, "Rentals.csv");
            maintenanceFilePath = Path.Combine(adminDirectory, "Maintenance.csv");

            InitializeDirectories();
            InitializeFiles();
        }

        private void InitializeDirectories()
        {
            try
            {
                string[] directories = { baseDirectory, customerDirectory, agentDirectory, adminDirectory, adminReportsDirectory };

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
            if (!File.Exists(carsFilePath))
            {
                List<Car> defaultCars = GetDefaultCars();
                SaveCars(defaultCars);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  ✓ Initialized {carsFilePath} with 10 cars");
                Console.ResetColor();
            }

            if (!File.Exists(customersFilePath))
            {
                List<Customer> defaultCustomers = new List<Customer>
                {
                    new Customer("C001", "John Doe", "customer123")
                };
                SaveCustomers(defaultCustomers);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  ✓ Initialized {customersFilePath} with default customer");
                Console.ResetColor();
            }

            if (!File.Exists(agentsFilePath))
            {
                List<CompanyAgent> defaultAgents = new List<CompanyAgent>
                {
                    new CompanyAgent("A001", "Admin", "admin123")
                };
                SaveAgents(defaultAgents);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  ✓ Initialized {agentsFilePath} with default admin");
                Console.ResetColor();
            }

            if (!File.Exists(rentalsFilePath))
                File.Create(rentalsFilePath).Close();

            if (!File.Exists(maintenanceFilePath))
                File.Create(maintenanceFilePath).Close();
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

        // ═══════════════════════════════════════════════════════════
        // READ OPERATIONS
        // ═══════════════════════════════════════════════════════════

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
                Console.WriteLine($"  ✗ Error reading cars: {ex.Message}");
                Console.ResetColor();
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
                Console.WriteLine($"  ✗ Error reading agents: {ex.Message}");
                Console.ResetColor();
            }

            return agents;
        }

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
                Console.WriteLine($"  ✗ Error reading rentals: {ex.Message}");
                Console.ResetColor();
            }

            return rentals;
        }

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
                Console.WriteLine($"  ✗ Error reading maintenance: {ex.Message}");
                Console.ResetColor();
            }

            return maintenanceList;
        }

        // ═══════════════════════════════════════════════════════════
        // WRITE OPERATIONS
        // ═══════════════════════════════════════════════════════════

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
                Console.WriteLine($"  ✗ Error saving cars: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

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
                Console.WriteLine($"  ✗ Error saving customers: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

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
                Console.WriteLine($"  ✗ Error saving agents: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

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
                Console.WriteLine($"  ✗ Error saving rentals: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

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
                Console.WriteLine($"  ✗ Error saving maintenance: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public bool SaveCustomerReceipt(string customerID, string receiptContent)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Receipt_{timestamp}.txt";
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

        public bool SaveMaintenanceReport(string agentID, string carID, string reportContent)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Maintenance_{carID}_{timestamp}.txt";
                string agentFolder = Path.Combine(agentDirectory, agentID);
                string filePath = Path.Combine(agentFolder, fileName);

                if (!Directory.Exists(agentFolder))
                {
                    Directory.CreateDirectory(agentFolder);
                }

                File.WriteAllText(filePath, reportContent);
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Error saving maintenance report: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public bool SaveRevenueReport(string reportContent)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("MMMyyyy");
                string fileName = $"Revenue_{timestamp}.txt";
                string filePath = Path.Combine(adminReportsDirectory, fileName);

                if (!Directory.Exists(adminReportsDirectory))
                {
                    Directory.CreateDirectory(adminReportsDirectory);
                }

                File.WriteAllText(filePath, reportContent);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  ✓ Revenue report saved to: {filePath}");
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

        public string GetCarsFilePath() { return Path.GetFullPath(carsFilePath); }
        public string GetCustomersFilePath() { return Path.GetFullPath(customersFilePath); }
        public string GetAgentsFilePath() { return Path.GetFullPath(agentsFilePath); }
        public string GetRentalsFilePath() { return Path.GetFullPath(rentalsFilePath); }
        public string GetMaintenanceFilePath() { return Path.GetFullPath(maintenanceFilePath); }

        internal void SaveProfitReport(string reportData)
        {
            throw new NotImplementedException();
        }
    }
}
