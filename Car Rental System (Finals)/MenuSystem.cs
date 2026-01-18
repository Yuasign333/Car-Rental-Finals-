using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;

namespace CarRentalSystem
{
    internal class MenuSystem
    {
        private FileHandler fileHandler;
        private RentalManager rentalManager;
        private MaintenanceManager maintenanceManager;
        private List<Customer> customers;
        private List<CompanyAgent> agents;
        private Customer currentCustomer;
        private CompanyAgent currentAgent;



        public MenuSystem()
        {
            Console.OutputEncoding = Encoding.UTF8;
            fileHandler = new FileHandler();
            rentalManager = new RentalManager(fileHandler);
            maintenanceManager = new MaintenanceManager(fileHandler);

            customers = fileHandler.ReadCustomers();
            agents = fileHandler.ReadAgents();
            List<Car> allCars = fileHandler.ReadCars();

            bool dataChanged = false;

            // --- CAR DATA CLEANUP ---
            List<Car> validCars = new List<Car>();
            List<string> carIds = new List<string>();

            foreach (Car car in allCars)
            {
                if (!carIds.Contains(car.GetCarID()))
                {
                    validCars.Add(car);
                    carIds.Add(car.GetCarID());
                }
                else
                {
                    // This car is a "Zombie" duplicate, we will ignore it 
                    // and the SaveCars call below will delete its file.
                    dataChanged = true;
                }
            }

            if (dataChanged)
            {
                // This is the "Magic" line: it deletes ALL car files 
                // and only writes back the unique ones.
                fileHandler.SaveCars(validCars);

                // Refresh the rentalManager with the clean list
                rentalManager.ReloadData();
            }
        }
        public void Start()
        {
            ShowWelcomeScreen();
            MainLoginMenu();
        }

        // ═══════════════════════════════════════════════════════════
        // 🎨 WELCOME & LOGIN SCREENS
        // ═══════════════════════════════════════════════════════════

        private void ShowWelcomeScreen()
        {
            Console.Clear();

         
            Console.ForegroundColor = ConsoleColor.White;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(100, 10);
            Console.WriteLine(@"
                                      ______              ____             __        __   
                                     / ____/___ ______   / __ \___  ____  / /_____ _/ /   
                                    / /   / __ `/ ___/  / /_/ / _ \/ __ \/ __/ __ `/ /    
                                   / /___/ /_/ / /     / _, _/  __/ / / / /_/ /_/ / /     
                                   \____/\__,_/_/     /_/ |_|\___/_/ /_/\__/\__,_/_/      
                                                       
");
           
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(70, 17);
            Console.WriteLine("Management System");
            Console.ResetColor();



            Console.SetCursorPosition(46, 27);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            Console.SetCursorPosition(50, 29);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Loading");

            for (int i = 0; i < 3; i++)
            {
                Console.Write(".");
                Thread.Sleep(300);
            }

            Console.ResetColor();
            Console.Clear();
        }

        private void MainLoginMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                       MAIN LOGIN MENU                          ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n1. Customer Login");
                Console.WriteLine("\n2. Company Agent Login");
                Console.WriteLine("\n3. Register as New Customer");
                Console.WriteLine("\n4. Exit");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\n  Choice: ");
                Console.ResetColor();

                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    if (LoginUser("Customer"))
                    {
                        CustomerDashboard();
                    }
                }
                else if (choice == "2")
                {
                    if (LoginUser("Agent"))
                    {
                        AdminDashboard();
                    }
                }
                else if (choice == "3")
                {
                    RegisterNewCustomer();
                }
                else if (choice == "4")
                {
                    ExitSystem();
                    return;
                }
                else
                {
                    ShowError("Invalid choice!");
                }
            }
        }

        private bool LoginUser(string userType)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║                    {userType.ToUpper()} LOGIN                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            Console.Write("  User ID: ");
            string userID = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userID))
            {
                ShowError("User ID cannot be empty!");
                return false;
            }

            Console.Write("  Password: ");
            string password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Password cannot be empty!");
                return false;
            }

            if (userType == "Customer")
            {
                Customer customer = null;
                foreach (Customer c in customers)
                {
                    if (c.GetCustomerID() == userID)
                    {
                        customer = c;
                        break;
                    }
                }

                if (customer != null && customer.ValidatePassword(password))
                {
                    currentCustomer = customer;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n  ✓ Welcome, {customer.GetName()}!");
                    Console.ResetColor();
                    Thread.Sleep(1500);
                    return true;
                }
            }
            else if (userType == "Agent")
            {
                CompanyAgent agent = null;
                foreach (CompanyAgent a in agents)
                {
                    if (a.GetAgentID() == userID)
                    {
                        agent = a;
                        break;
                    }
                }

                if (agent != null && agent.ValidatePassword(password))
                {
                    currentAgent = agent;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n  ✓ Welcome, {agent.GetName()}!");
                    Console.ResetColor();
                    Thread.Sleep(1500);
                    return true;
                }
            }

            ShowError("Invalid credentials!");
            return false;
        }

        private void RegisterNewCustomer()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    NEW CUSTOMER REGISTRATION                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("\nName: ");
            string name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowError("\nName cannot be empty!");
                return;
            }
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("\nPassword: ");
            string password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowError("\nPassword cannot be empty!");
                return;
            }

            // checks if no duplicate names exist
            foreach (Customer c in customers)
            {
                if (c.GetName().Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    ShowError("\nA customer with this name already exists!");
                    return;
                }
            }

            string customerID = "C" + (customers.Count + 1).ToString("D3");
            Customer newCustomer = new Customer(customerID, name, password);
            customers.Add(newCustomer);
            fileHandler.SaveCustomers(customers);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✓ Registration successful!");
            Console.WriteLine($"  Your Customer ID is: {customerID}");
            Console.ResetColor();
            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        //  CUSTOMER DASHBOARD
        // ═══════════════════════════════════════════════════════════

        private void CustomerDashboard()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║  CUSTOMER DASHBOARD - Welcome, {currentCustomer.GetName(),-28} ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n1. Browse Available Cars");
                Console.WriteLine("\n2. Rent a Car");
                Console.WriteLine("\n3. View My Active Rentals");
                Console.WriteLine("\n4. View Rental History");
                Console.WriteLine("\n5. Logout");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nChoice: ");
                Console.ResetColor();

                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    BrowseCars();
                }
                else if (choice == "2")
                {
                    RentCar();
                }
                else if (choice == "3")
                {
                    ViewActiveRentals();
                }
                else if (choice == "4")
                {
                    ViewRentalHistory();
                }
                else if (choice == "5")
                {
                    currentCustomer = null;
                    return;
                }
                else
                {
                    ShowError("Invalid choice!");
                }
            }
        }

        private void BrowseCars()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      FILTER CARS                               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

          
            string categoryFilter = "";
            while (true)
            {
                Console.Clear(); // Clear screen so it looks clean
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n  [STEP 1/2] SELECT CATEGORY");
                Console.WriteLine("  1. All");
                Console.WriteLine("  2. SUV");
                Console.WriteLine("  3. Sedan");
                Console.WriteLine("  4. Van");
                Console.Write("\n  Choice: ");

                string catChoice = Console.ReadLine();
                bool isValid = true;

                switch (catChoice)
                {
                    case "1": categoryFilter = "ALL"; break;
                    case "2": categoryFilter = "SUV"; break;
                    case "3": categoryFilter = "Sedan"; break;
                    case "4": categoryFilter = "Van"; break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n  ✗ Invalid choice! Enter 1-4.");
                        Console.ReadKey();
                        isValid = false;
                        break;
                }

                if (isValid) break; // 
            }

            string fuelFilter = "";
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nCategory Selected: {categoryFilter}"); // Show progress

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n[STEP 2/2] SELECT FUEL TYPE");
                Console.WriteLine("\n1. All");
                Console.WriteLine("\n2. Dual Motor");
                Console.WriteLine("\n3. Standard Engine");
                Console.WriteLine("\n4. EV");
                Console.Write("\nChoice: ");

                string fuelChoice = Console.ReadLine();
                bool valid = true;

                switch (fuelChoice)
                {
                    case "1": fuelFilter = "ALL"; break;
                    case "2": fuelFilter = "Dual Motor"; break;
                    case "3": fuelFilter = "Standard Engine"; break;
                    case "4": fuelFilter = "EV"; break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n  ✗ Invalid choice! Enter 1-4.");
                        Console.ReadKey();
                        valid = false;
                        break;
                }

                if (valid) break; // This CLOSES the Fuel loop
            }

        
            DisplayAvailableCars(categoryFilter, fuelFilter);
        }

        private void DisplayAvailableCars(string categoryFilter, string fuelFilter)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("════════════════════════════════════════════════════════════════");
            Console.WriteLine($"  AVAILABLE CARS (CATEGORY: {categoryFilter}, FUEL: {fuelFilter})");
            Console.WriteLine("════════════════════════════════════════════════════════════════");
            Console.ResetColor();

            List<Car> availableCars = rentalManager.GetAvailableCars(categoryFilter, fuelFilter);

            if (availableCars.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n  ✗ No cars available with selected filters");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"\n  {"ID",-6} | {"Model",-20} | {"Category",-10} | {"Fuel Type",-18} | {"Price/Hr",-10}");
                Console.WriteLine(new string('-', 75)); // Decorative separator line
                foreach (Car car in availableCars)
                {
                    // Make sure these variables match the headers exactly
                    Console.WriteLine($"  {car.GetCarID(),-6} | {car.GetModel(),-20} | {car.GetCategory(),-10} | {car.GetFuelType(),-18} | ${car.GetHourlyRate(),-9:F2}");
                }
            }

            PauseScreen();
        }

        private void RentCar()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        RENT A CAR                              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            List<Car> availableCars = rentalManager.GetAvailableCars();

            if (availableCars.Count == 0)
            {
                ShowError("No cars available at the moment");
                return;
            }

            Console.WriteLine($"\n  {"ID",-6} | {"Model",-20} | {"Category",-10} | {"Fuel Type",-18} | {"Price/Hr",-10}");
            Console.WriteLine(new string('-', 75)); // Decorative separator line
            foreach (Car car in availableCars)
            {
                // Make sure these variables match the headers exactly
                Console.WriteLine($"  {car.GetCarID(),-6} | {car.GetModel(),-20} | {car.GetCategory(),-10} | {car.GetFuelType(),-18} | ${car.GetHourlyRate(),-9:F2}");
            }

            Console.Write("\nEnter Car ID to Rent (or 0 to cancel): ");
            string carID = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(carID) || carID == "0")
            {
                return;
            }

            bool carExists = false;
            foreach (Car c in availableCars)
            {
                if (c.GetCarID() == carID)
                {
                    carExists = true;
                    break;
                }
            }

            if (!carExists)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowError("Invalid Car ID!");
                return;
            }

            string conflict = rentalManager.CheckRentalConflict(carID);
            if (conflict != "OK")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowError("Cannot rent: " + conflict);
                return;
            }

            Console.Write("  \nEnter Driver Name: ");
            string driverName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(driverName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowError("Driver name cannot be empty!");
                return;
            }

            if (rentalManager.IsDriverNameTaken(currentCustomer.GetCustomerID(), driverName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowError("\nThis driver name is already used in one of your bookings!");
                return;
            }

            Console.Write("  \nHow many hours do you plan to rent? ");
            string hoursInput = Console.ReadLine();

            if (!int.TryParse(hoursInput, out int hours) || hours <= 0)
            {
                ShowError("Invalid number of hours! Please enter a positive number.");
                return;
            }

            decimal basePrice, deposit;
            decimal totalDue = rentalManager.CalculateEstimate(carID, hours, out basePrice, out deposit);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  >>> RENTAL ESTIMATE <<<");
            Console.ResetColor();
            Console.WriteLine($"  Base Price:  ${basePrice:F2}");
            Console.WriteLine($"  Deposit:     ${deposit:F2}");
            Console.WriteLine("  ─────────────────────");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Total Due:   ${totalDue:F2}");
            Console.ResetColor();

            Console.Write("\n  Confirm Booking? (Y/N): ");
            string confirm = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(confirm) || confirm.ToUpper() != "Y")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  ⚠ Booking cancelled");
                Console.ResetColor();
                PauseScreen();
                return;
            }

            string rentalID;
            if (rentalManager.ConfirmBooking(currentCustomer.GetCustomerID(), carID, driverName, hours, out rentalID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n  ✓ Booking confirmed!");

              
                Console.Write("\n  Generating receipt");

                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(500);
                    Console.Write("."); 
                }

                Console.ResetColor();
                Thread.Sleep(500);// Give the user a moment to see the 3rd dot before clearing
                Console.Clear();
                Car car = rentalManager.GetCarByID(carID);
                string receipt = GenerateRentalReceipt(rentalID, car, driverName, hours, basePrice, deposit, totalDue);
                Console.WriteLine(receipt);

                fileHandler.SaveCustomerReceipt(currentCustomer.GetCustomerID(), receipt);

                PauseScreen();
            }
            else
            {
                ShowError("Booking failed. Please try again.");
            }
        }

        private string GenerateRentalReceipt(string rentalID, Car car, string driverName, int hours, decimal basePrice, decimal deposit, decimal total)
        {
            StringBuilder receipt = new StringBuilder();
            receipt.AppendLine("\n╔════════════════════════════════════════════════════════════════╗");
            receipt.AppendLine("║                      RENTAL RECEIPT                            ║");
            receipt.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            receipt.AppendLine($"  Rental ID:       {rentalID}");
            receipt.AppendLine($"  Customer:        {currentCustomer.GetName()}");
            receipt.AppendLine($"  Customer ID:     {currentCustomer.GetCustomerID()}");
            receipt.AppendLine($"  Driver Name:     {driverName}");
            receipt.AppendLine($"  Car:             {car.GetModel()} ({car.GetCarID()})");
            receipt.AppendLine($"  Rental Start:    {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            receipt.AppendLine($"  Estimated Hours: {hours}");
            receipt.AppendLine("  ─────────────────────────────────────────────────────────────────");
            receipt.AppendLine($"  Base Price:      ${basePrice:F2}");
            receipt.AppendLine($"  Deposit:         ${deposit:F2}");
            receipt.AppendLine("  ─────────────────────────────────────────────────────────────────");
            receipt.AppendLine($"  TOTAL DUE:       ${total:F2}");
            receipt.AppendLine("═════════════════════════════════════════════════════════════════");
            return receipt.ToString();
        }

        private void ViewActiveRentals()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    MY ACTIVE RENTALS                           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            List<Rental> activeRentals = rentalManager.GetCustomerActiveRentals(currentCustomer.GetCustomerID());

            if (activeRentals.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  You have no active rentals.");
                Console.ResetColor();
            }
            else
            {
                foreach (Rental rental in activeRentals)
                {
                    Car car = rentalManager.GetCarByID(rental.GetCarID());
                    Console.WriteLine($"  Rental ID:       {rental.GetRentalID()}");
                    Console.WriteLine($"  Driver:          {rental.GetDriverName()}");
                    Console.WriteLine($"  Car:             {car.GetModel()} ({car.GetCarID()})");
                    Console.WriteLine($"  Started:         {rental.GetRentalStartTime():yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"  Estimated Hours: {rental.GetEstimatedHours()}");
                    Console.WriteLine($"  Total Cost:      ${rental.GetTotalCost():F2}");
                    Console.WriteLine("  ─────────────────────────────────────────────────────────────────");
                }
            }

            PauseScreen();
        }

        private void ViewRentalHistory()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                     RENTAL HISTORY                             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            List<Rental> allRentals = rentalManager.GetCustomerRentals(currentCustomer.GetCustomerID());

            if (allRentals.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  No rental history found.");
                Console.ResetColor();
            }
            else
            {
                foreach (Rental rental in allRentals)
                {
                    Car car = rentalManager.GetCarByID(rental.GetCarID());
                    string endDate = "Ongoing";
                    if (rental.GetStatus() == "Completed")
                    {
                        endDate = rental.GetRentalEndTime().ToString("yyyy-MM-dd");
                    }

                    Console.WriteLine($"  [{rental.GetStatus()}] Rental ID: {rental.GetRentalID()}");
                    Console.WriteLine($"    Driver: {rental.GetDriverName()}");
                    Console.WriteLine($"    Car: {car.GetModel()}");
                    Console.WriteLine($"    Period: {rental.GetRentalStartTime():yyyy-MM-dd} to {endDate}");
                    Console.WriteLine($"    Total: ${rental.GetTotalCost():F2}");
                    Console.WriteLine("  ─────────────────────────────────────────────────────────────────");
                }
            }

            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        //  ADMIN DASHBOARD
        // ═══════════════════════════════════════════════════════════

        private void AdminDashboard()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║  ADMIN DASHBOARD - Welcome, {currentAgent.GetName(),-29}      ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\n1. View Fleet Status");
                Console.WriteLine("\n2. Process Car Return");
                Console.WriteLine("\n3. Add Maintenance Record");
                Console.WriteLine("\n4. Complete Maintenance");
                Console.WriteLine("\n5. View Maintenance Records");
                Console.WriteLine("\n6. Add/Remove Cars");
                Console.WriteLine("\n7. View Total Revenue");
                Console.WriteLine("\n8. Logout");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\n  Choice: ");
                Console.ResetColor();

                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    ViewFleetStatus();
                }
                else if (choice == "2")
                {
                    ProcessReturn();
                }
                else if (choice == "3")
                {
                    AddMaintenance();
                }
                else if (choice == "4")
                {
                    CompleteMaintenance();
                }
                else if (choice == "5")
                {
                    ViewMaintenanceRecords();
                }
                else if (choice == "6")
                {
                    AddorRemoveCarsMenu();
                }
                else if (choice == "7")
                {
                    ViewTotalRevenue();
                }
                else if (choice == "8")
                {
                    currentAgent = null;
                    return;
                }
                else
                {
                    ShowError("Invalid choice!");
                }
            }
        }

        private void ViewFleetStatus()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        FLEET STATUS                            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            rentalManager.ReloadData();
            List<Car> allCars = rentalManager.GetAllCars();

            // Remove duplicates by ID (keep only the first occurrence)
            List<Car> uniqueCars = new List<Car>();
            List<string> seenIds = new List<string>();

         

            foreach (Car car in allCars)
            {
                bool alreadySeen = false;
                foreach (string id in seenIds)
                {
                    if (id == car.GetCarID())
                    {
                        alreadySeen = true;
                        break;
                    }
                }

                if (!alreadySeen)
                {
                    uniqueCars.Add(car);
                    seenIds.Add(car.GetCarID());
                }
            }

            int available = 0, rented = 0, maintenance = 0;

            Console.WriteLine($"  {"ID",-8} | {"Model",-20} | {"Category",-10} | {"Status",-20}");
            Console.WriteLine("  ───────────────────────────────────────────────────────────────────────");

            foreach (Car car in uniqueCars)
            {
                string status = car.GetStatus();

                if (status == "Available")
                {
                    available++;
                }
                else if (status == "Rented")
                {
                    rented++;
                }
                else if (status == "Under Maintenance")
                {
                    maintenance++;
                }
                // Skip cars with IDs starting with "M"
                if (car.GetCarID().StartsWith("M", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ConsoleColor statusColor = ConsoleColor.White;
                if (status == "Available")
                {
                    statusColor = ConsoleColor.Green;
                }
                else if (status == "Rented")
                {
                    statusColor = ConsoleColor.Yellow;
                }
                else
                {
                    statusColor = ConsoleColor.Red;
                }

                // Truncate model name if too long
                string modelName = car.GetModel();
                if (modelName.Length > 20)
                {
                    modelName = modelName.Substring(0, 17) + "...";
                }

                Console.Write($"  {car.GetCarID(),-8} | {modelName,-20} | {car.GetCategory(),-10} | ");
                Console.ForegroundColor = statusColor;
                Console.WriteLine($"{status,-20}");
                Console.ResetColor();
            }

            Console.WriteLine("  ───────────────────────────────────────────────────────────────────────");
            Console.WriteLine($"  Total: {uniqueCars.Count} | Available: {available} | Rented: {rented} | Maintenance: {maintenance}");

            PauseScreen();
        }

        private void ProcessReturn()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    PROCESS CAR RETURN                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            rentalManager.ReloadData();
            List<Car> rentedCars = rentalManager.GetAllCars().FindAll(c => c.GetStatus() == "Rented");

            if (rentedCars.Count == 0)
            {
                ShowError("No cars currently rented");
                return;
            }

            Console.WriteLine("  Currently Rented Cars:");
            Console.WriteLine($"  {"ID",-6} | {"Model",-17} | {"Renter ID",-12} | {"Est. Hours",-10}");
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────");

            foreach (Car car in rentedCars)
            {
                Console.WriteLine($"  {car.GetCarID(),-6} | {car.GetModel(),-17} | {car.GetCurrentRenterID(),-12} | {car.GetEstimatedHours(),-10}");
            }
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────\n");

            Console.Write("  Enter Car ID to process return (or 0 to cancel): ");
            string carID = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(carID) || carID == "0")
            {
                return;
            }

            Console.Write("  Enter actual hours used: ");
            string hoursInput = Console.ReadLine();

            if (!int.TryParse(hoursInput, out int actualHours) || actualHours <= 0)
            {
                ShowError("Invalid number of hours! Please enter a positive number.");
                return;
            }

            decimal finalCost, discount;
            string message;

            if (rentalManager.ProcessReturn(carID, actualHours, out finalCost, out discount, out message))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n  ✓ Return processed successfully!\n");
                Console.ResetColor();

                Car car = rentalManager.GetCarByID(carID);

                Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                      RETURN SUMMARY                            ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
                Console.WriteLine($"  Car:            {car.GetModel()} ({carID})");
                Console.WriteLine($"  Actual Hours:   {actualHours}");

                if (discount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  Discount:       ${discount:F2}");
                    Console.ResetColor();
                }

                Console.WriteLine($"  Final Cost:     ${finalCost:F2}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n  Status: {message}");
                Console.ResetColor();
                Console.WriteLine("═════════════════════════════════════════════════════════════════");
            }
            else
            {
                ShowError($"Return failed: {message}");
            }

            PauseScreen();
        }

        private void AddMaintenance()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   ADD MAINTENANCE RECORD                       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            maintenanceManager.ReloadData();
            rentalManager.ReloadData();
            List<Car> availableCars = rentalManager.GetAllCars().FindAll(c =>
                c.GetStatus() == "Available" || c.GetStatus() == "Under Maintenance");

            Console.WriteLine($"  {"ID",-6} | {"Model",-17} | {"Status",-18}");
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────");

            foreach (Car car in availableCars)
            {
                Console.WriteLine($"  {car.GetCarID(),-6} | {car.GetModel(),-17} | {car.GetStatus(),-18}");
            }
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────\n");

            Console.Write("Enter Car ID for maintenance (or 0 to cancel): ");
            string carID = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(carID) || carID == "0")
            {
                return;
            }

            Console.Write("  Technician Name: ");
            string techName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(techName))
            {
                ShowError("Technician name cannot be empty!");
                return;
            }

            Console.Write("  Description: ");
            string description = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(description))
            {
                ShowError("Description cannot be empty!");
                return;
            }

            string message;
            if (maintenanceManager.AddMaintenance(carID, techName, description, out message))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  ✓ {message}");
                Console.ResetColor();
            }
            else
            {
                ShowError(message);
            }

            PauseScreen();
        }

        private void CompleteMaintenance()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   COMPLETE MAINTENANCE                         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            maintenanceManager.ReloadData();
            List<Maintenance> inProgress = maintenanceManager.GetInProgressMaintenance();

            if (inProgress.Count == 0)
            {
                ShowError("No maintenance in progress");
                return;
            }

            Console.WriteLine("  In-Progress Maintenance:");
            Console.WriteLine($"  {"Maint. ID",-10} | {"Car ID",-8} | {"Technician",-15} | {"Description",-25}");
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────");

            foreach (Maintenance m in inProgress)
            {
                string desc = m.GetDescription().Length > 25 ? m.GetDescription().Substring(0, 22) + "..." : m.GetDescription();
                Console.WriteLine($"  {m.GetMaintenanceID(),-10} | {m.GetCarID(),-8} | {m.GetTechnicianName(),-15} | {desc,-25}");
            }
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────\n");

            Console.Write("  Enter Maintenance ID to complete (or 0 to cancel): ");
            string maintID = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(maintID) || maintID == "0")
            {
                return;
            }

            string message;
            if (maintenanceManager.CompleteMaintenance(maintID, out message))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  ✓ {message}");
                Console.ResetColor();
            }
            else
            {
                ShowError(message);
            }

            PauseScreen();
        }

        private void ViewMaintenanceRecords()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   MAINTENANCE RECORDS                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            maintenanceManager.ReloadData();
            List<Maintenance> allMaintenance = maintenanceManager.GetAllMaintenance();

            if (allMaintenance.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  No maintenance records found.");
                Console.ResetColor();
            }
            else
            {
                foreach (Maintenance m in allMaintenance)
                {
                    ConsoleColor statusColor = m.GetStatus() == "Completed" ? ConsoleColor.Green : ConsoleColor.Yellow;

                    Console.ForegroundColor = statusColor;
                    Console.WriteLine($"\n[{m.GetStatus()}] {m.GetMaintenanceID()}");
                    Console.ResetColor();
                    Console.WriteLine($"\nCar: {m.GetCarID()}");
                    Console.WriteLine($"\nTechnician: {m.GetTechnicianName()}");
                    Console.WriteLine($"\nDate: {m.GetMaintenanceDate():yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"\nDescription: {m.GetDescription()}");
                    Console.WriteLine("  ─────────────────────────────────────────────────────────────────");
                }
            }

            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        //  ADD CARS MENU (SINGLE & BULK WITH VALIDATION)
        // ═══════════════════════════════════════════════════════════

        private void AddorRemoveCarsMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    MANAGE CAR INVENTORY                        ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\n1. Add Single Car (Manual)");
                Console.WriteLine("\n2. Add Bulk Cars (Paste Multiple Cars)");
                Console.WriteLine("\n3. Remove Car from Fleet");
                Console.WriteLine("\n4. Return to Main Menu");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\n  Choice: ");
                Console.ResetColor();

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": AddSingleCar(); break;
                    case "2": AddBulkCars(); break;
                    case "3": RemoveCar(); break;
                    case "4": return;
                    default: ShowError("Invalid Choice!"); break;
                }
            }
        }

        private void AddSingleCar()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        ADD SINGLE CAR                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            List<Car> cars = fileHandler.ReadCars();
            string carID = "";

            // --- ID SELECTION MENU ---
            Console.WriteLine("  ID Assignment:");
            Console.WriteLine("  1. Auto-Generate (Smart Fill)");
            Console.WriteLine("  2. Manual Entry (Override)");
            Console.Write("\n  Choice: ");
            string idChoice = Console.ReadLine();

            if (idChoice == "2")
            {
                Console.Write("  Enter Manual ID (e.g., C001): ");
                carID = Console.ReadLine()?.Trim().ToUpper();

                // Check if manual ID already exists to prevent duplicates
                bool exists = false;
                foreach (Car c in cars) { if (c.GetCarID() == carID) exists = true; }

                if (exists)
                {
                    ShowError($"ID {carID} is already taken by another car!");
                    return;
                }
            }
            else
            {
                // --- SMART FILL LOGIC (Finds gaps like C001) ---
                int idSeed = 1;
                while (true)
                {
                    string checkID = $"C{idSeed:D3}";
                    bool found = false;
                    foreach (Car c in cars)
                    {
                        if (c.GetCarID() == checkID) { found = true; break; }
                    }

                    if (!found) // We found a gap!
                    {
                        carID = checkID;
                        break;
                    }
                    idSeed++;
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  [SYSTEM] Smart-Fill ID assigned: {carID}");
                Console.ResetColor();
            }

            // --- REST OF THE INPUT LOGIC ---
            Console.Write("  Model: ");
            string model = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(model)) { ShowError("Model cannot be empty!"); return; }

            // Category Selection
            string category = GetCategoryChoice(); // Helper method recommended for clean code

            // Fuel Selection
            string fuelType = GetFuelChoice(); // Helper method recommended for clean code

            Console.Write("\n  Hourly Rate: $");
            if (!decimal.TryParse(Console.ReadLine(), out decimal hourlyRate) || hourlyRate <= 0)
            {
                ShowError("Invalid hourly rate!");
                return;
            }

            // Save
            Car newCar = new Car(carID, model, category, fuelType, hourlyRate);
            cars.Add(newCar);
            fileHandler.SaveCars(cars);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✓ Car '{model}' saved successfully as {carID}!");
            Console.ResetColor();
            PauseScreen();
        }

        private void AddBulkCars()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                           ADD BULK CARS                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            Console.WriteLine("\nInstructions:");
            Console.WriteLine("\n1.Do NOT include a Car ID (the system will generate it automatically).");
            Console.WriteLine("\n2.Format: Model, Category, FuelType, HourlyRate");
            Console.WriteLine("\nExample: Toyota Fortuner,SUV,Standard Engine,50.00");
            Console.WriteLine("\nValid Categories: SUV, Sedan, Van");
            Console.WriteLine("Valid Fuel Types: Standard Engine, Dual Motor, EV");
            Console.WriteLine("\nType 'END' on a new line when done:\n");

            List<string> csvLines = new List<string>();
            while (true)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.ToUpper() == "END") break;
                csvLines.Add(line);
            }

            if (csvLines.Count == 0) { return; }

            Console.WriteLine("\n  ═══════════════ VALIDATION RESULTS ═══════════════\n");

            List<Car> existingCars = fileHandler.ReadCars();
            int successCount = 0;
            int failCount = 0;

            // --- SMART ID LOGIC ---
            int maxFound = 0;
            foreach (Car car in existingCars)
            {
                string idPart = car.GetCarID().Replace("C", "");
                if (int.TryParse(idPart, out int currentNum))
                {
                    if (currentNum > maxFound) maxFound = currentNum;
                }
            }
            int nextIdNumber = maxFound + 1;

            foreach (string line in csvLines)
            {
                string[] parts = line.Split(',');

                // Validate Column Count (Must be exactly 4)
                if (parts.Length != 4)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAILED] Wrong format (Needs 4 parts): {line}");
                    Console.ResetColor();
                    failCount++;
                    continue;
                }

                string model = parts[0].Trim();
                string category = parts[1].Trim();
                string fuelType = parts[2].Trim();
                string rateStr = parts[3].Trim();

                //. Validate Category (Case Insensitive)
                string upperCat = category.ToUpper();
                if (upperCat != "SUV" && upperCat != "SEDAN" && upperCat != "VAN")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAILED] Invalid Category '{category}': {line}");
                    Console.ResetColor();
                    failCount++;
                    continue;
                }
                // Normalize the category name for storage (SUV, Sedan, Van)
                category = char.ToUpper(category[0]) + category.Substring(1).ToLower();
                if (upperCat == "SUV") category = "SUV";

                // 2Validate Fuel Type
                string upperFuel = fuelType.ToUpper();
                if (upperFuel != "STANDARD ENGINE" && upperFuel != "DUAL MOTOR" && upperFuel != "EV")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAILED] Invalid Fuel Type '{fuelType}': {line}");
                    Console.ResetColor();
                    failCount++;
                    continue;
                }

                //  Validate Rate
                if (!decimal.TryParse(rateStr, out decimal hourlyRate) || hourlyRate <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAILED] Invalid Rate '{rateStr}': {line}");
                    Console.ResetColor();
                    failCount++;
                    continue;
                }

                //  Success - Create ID and Add
                string carID = $"C{nextIdNumber:D3}";
                Car newCar = new Car(carID, model, category, fuelType, hourlyRate);
                existingCars.Add(newCar);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  [SUCCESS] {carID} created for {model}");
                Console.ResetColor();

                successCount++;
                nextIdNumber++;
            }

            // Save and Final Message
            if (successCount > 0)
            {
                fileHandler.SaveCars(existingCars);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n  ✓ Saved {successCount} cars to the database.");
            }

            if (failCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ {failCount} lines failed validation.");
            }

            Console.ResetColor();
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        // Add this method to your MenuSystem class

        private void RemoveCar()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         REMOVE CAR(S)                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            // Reload to get latest data
            rentalManager.ReloadData();
            List<Car> cars = fileHandler.ReadCars();

            if (cars.Count == 0)
            {
                ShowError("The fleet is currently empty.");
                return;
            }

            // Display all cars
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  CURRENT FLEET:");
            Console.ResetColor();
            Console.WriteLine($"  {"ID",-8} | {"Model",-20} | {"Category",-10} | {"Status",-20}");
            Console.WriteLine("  ───────────────────────────────────────────────────────────────────────");

            foreach (Car car in cars)
            {
                ConsoleColor statusColor = ConsoleColor.White;
                if (car.GetStatus() == "Available")
                {
                    statusColor = ConsoleColor.Green;
                }
                else if (car.GetStatus() == "Rented")
                {
                    statusColor = ConsoleColor.Yellow;
                }
                else if (car.GetStatus() == "Under Maintenance")
                {
                    statusColor = ConsoleColor.Red;
                }

                Console.Write($"  {car.GetCarID(),-8} | ");

                // Truncate model name if too long
                string modelName = car.GetModel();
                if (modelName.Length > 20)
                {
                    modelName = modelName.Substring(0, 17) + "...";
                }
                Console.Write($"{modelName,-20} | {car.GetCategory(),-10} | ");

                Console.ForegroundColor = statusColor;
                Console.WriteLine($"{car.GetStatus(),-20}");
                Console.ResetColor();
            }
            Console.WriteLine("  ───────────────────────────────────────────────────────────────────────\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  INSTRUCTIONS:");
            Console.WriteLine("  - Enter single ID: C001");
            Console.WriteLine("  - Enter multiple IDs: C001,C002,C003");
            Console.WriteLine("  - Enter 0 to cancel");
            Console.ResetColor();

            Console.Write("\n  Enter Car ID(s) to remove: ");
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) || input.Trim() == "0")
            {
                Console.WriteLine("\n  Operation cancelled.");
                PauseScreen();
                return;
            }

            // Parse input - split by comma
            string[] idsToRemove = input.Split(',');
            List<string> cleanedIds = new List<string>();

            // Clean and validate IDs
            foreach (string id in idsToRemove)
            {
                string cleanId = id.Trim().ToUpper();
                if (!string.IsNullOrWhiteSpace(cleanId))
                {
                    cleanedIds.Add(cleanId);
                }
            }

            if (cleanedIds.Count == 0)
            {
                ShowError("No valid IDs provided!");
                return;
            }

            // Find cars to delete and validate
            List<Car> carsToDelete = new List<Car>();
            List<string> notFoundIds = new List<string>();
            List<string> rentedIds = new List<string>();

            foreach (string carId in cleanedIds)
            {
                bool found = false;
                foreach (Car car in cars)
                {
                    if (car.GetCarID() == carId)
                    {
                        found = true;

                        // Check if rented
                        if (car.GetStatus() == "Rented")
                        {
                            rentedIds.Add(carId);
                        }
                        else
                        {
                            carsToDelete.Add(car);
                        }
                        break;
                    }
                }

                if (!found)
                {
                    notFoundIds.Add(carId);
                }
            }

            // Display validation results
            Console.WriteLine("\n  ═══════════════ VALIDATION RESULTS ═══════════════\n");

            // Show not found IDs
            if (notFoundIds.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [NOT FOUND] The following IDs do not exist:");
                foreach (string id in notFoundIds)
                {
                    Console.WriteLine($"    - {id}");
                }
                Console.ResetColor();
            }

            // Show rented cars that cannot be deleted
            if (rentedIds.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [CANNOT DELETE] The following cars are currently rented:");
                foreach (string id in rentedIds)
                {
                    Console.WriteLine($"    - {id}");
                }
                Console.ResetColor();
            }

            // Show cars that will be deleted
            if (carsToDelete.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [READY TO DELETE] The following cars will be removed:");
                foreach (Car car in carsToDelete)
                {
                    Console.WriteLine($"    - {car.GetCarID()} ({car.GetModel()})");
                }
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n  ✗ No cars available for deletion!");
                Console.ResetColor();
                PauseScreen();
                return;
            }

            // Final confirmation
            Console.WriteLine("\n  ═══════════════════════════════════════════════════\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"  ⚠ DELETE {carsToDelete.Count} car(s) PERMANENTLY? (Y/N): ");
            Console.ResetColor();

            string confirm = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(confirm) || confirm.ToUpper() != "Y")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  Operation cancelled. No cars were deleted.");
                Console.ResetColor();
                PauseScreen();
                return;
            }

            // Perform deletion
            int deletedCount = 0;
            foreach (Car carToDelete in carsToDelete)
            {
                // Remove from memory list
                cars.Remove(carToDelete);
                deletedCount++;
            }

            // Save updated list - this will physically delete files
            fileHandler.SaveCars(cars);

            // Success message
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✓ Successfully deleted {deletedCount} car(s) from the system!");
            Console.WriteLine("  ✓ Changes saved permanently to files.");
            Console.ResetColor();

            // Show summary if there were any issues
            if (notFoundIds.Count > 0 || rentedIds.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  SUMMARY:");
                Console.WriteLine($"    Deleted: {deletedCount}");
                Console.WriteLine($"    Not Found: {notFoundIds.Count}");
                Console.WriteLine($"    Skipped (Rented): {rentedIds.Count}");
                Console.ResetColor();
            }

            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        //  VIEW TOTAL REVENUE
        // ═══════════════════════════════════════════════════════════

        private void ViewTotalRevenue()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      TOTAL REVENUE                             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            List<Rental> allRentals = fileHandler.ReadRentals();
            decimal totalRevenue = 0m;
            int completedCount = 0;

            foreach (Rental rental in allRentals)
            {
                if (rental.GetStatus() == "Completed")
                {
                    totalRevenue += rental.GetFinalCost();
                    completedCount++;
                }
            }

            Console.WriteLine($"  Total Completed Rentals: {completedCount}");
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  TOTAL REVENUE: ${totalRevenue:F2}");
            Console.ResetColor();
            Console.WriteLine("  ═════════════════════════════════════════════════════════════════");

            // Generate report
            StringBuilder report = new StringBuilder();
            report.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            report.AppendLine("║                   REVENUE REPORT                               ║");
            report.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Generated By: {currentAgent.GetName()} ({currentAgent.GetAgentID()})");
            report.AppendLine("─────────────────────────────────────────────────────────────────");
            report.AppendLine($"Total Completed Rentals: {completedCount}");
            report.AppendLine($"TOTAL REVENUE: ${totalRevenue:F2}");
            report.AppendLine("═════════════════════════════════════════════════════════════════");

            fileHandler.SaveRevenueReport(report.ToString());

            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        // UTILITY AND HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        private void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  ✗ {message}");
            Console.ResetColor();
            PauseScreen();
        }

        private void PauseScreen()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n  Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
        }

        private string GetCategoryChoice()
        {
            while (true)
            {
                Console.WriteLine("\n  Category:");
                Console.WriteLine("  1. SUV\n  2. Sedan\n  3. Van");
                Console.Write("  Choice: ");
                string choice = Console.ReadLine();
                if (choice == "1") return "SUV";
                if (choice == "2") return "Sedan";
                if (choice == "3") return "Van";
                Console.WriteLine("  Invalid selection!");
            }
        }

        private string GetFuelChoice()
        {
            while (true)
            {
                Console.WriteLine("\n  Select Fuel Type:");
                Console.WriteLine("  1. Standard Engine");
                Console.WriteLine("  2. Dual Motor");
                Console.WriteLine("  3. EV");
                Console.Write("  Choice: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": return "Standard Engine";
                    case "2": return "Dual Motor";
                    case "3": return "EV";
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  ✗ Invalid choice! Enter 1-3.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        private void ExitSystem()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║       Thank you for using Car Rental Management System!       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();
            Thread.Sleep(1500);
        }

       
    } 
} 
