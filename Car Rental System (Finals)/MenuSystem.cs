using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private bool isCustomerLoggedIn;

        public MenuSystem()
        {
            Console.OutputEncoding = Encoding.UTF8;

            fileHandler = new FileHandler();
            rentalManager = new RentalManager(fileHandler);
            maintenanceManager = new MaintenanceManager(fileHandler);
            customers = fileHandler.ReadCustomers();
            agents = fileHandler.ReadAgents();
            currentCustomer = null;
            currentAgent = null;
            isCustomerLoggedIn = false;
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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
   ______              ____             __        __   
  / ____/___ ______   / __ \___  ____  / /_____ _/ /   
 / /   / __ `/ ___/  / /_/ / _ \/ __ \/ __/ __ `/ /    
/ /___/ /_/ / /     / _, _/  __/ / / / /_/ /_/ / /     
\____/\__,_/_/     /_/ |_|\___/_/ /_/\__/\__,_/_/      
                                                       
");
            Console.ResetColor();
            Thread.Sleep(1000);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n                   Press any key to continue...");
            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n                        Loading");

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
                    isCustomerLoggedIn = true;
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
                    isCustomerLoggedIn = false;
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
                Console.WriteLine($"\n  {"ID",-6} | {"Model",-17} | {"Category",-8} | {"Fuel Type",-15} | {"Price/Hour",-10}");
                Console.WriteLine("  ─────────────────────────────────────────────────────────────────");

                foreach (Car car in availableCars)
                {
                    Console.WriteLine("  " + car.ToTableRow());
                }
                Console.WriteLine("  ─────────────────────────────────────────────────────────────────");
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

            Console.WriteLine($"  {"ID",-6} | {"Model",-17} | {"Category",-8} | {"Fuel Type",-15} | {"Price/Hour",-10}");
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────");
            foreach (Car car in availableCars)
            {
                Console.WriteLine("  " + car.ToTableRow());
            }
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────\n");

            Console.Write("  Enter Car ID to Rent (or 0 to cancel): ");
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
                ShowError("Invalid Car ID!");
                return;
            }

            string conflict = rentalManager.CheckRentalConflict(carID);
            if (conflict != "OK")
            {
                ShowError($"Cannot rent: {conflict}");
                return;
            }

            Console.Write("  How many hours do you plan to rent? ");
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
            if (rentalManager.ConfirmBooking(currentCustomer.GetCustomerID(), carID, hours, out rentalID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n  ✓ Booking confirmed!");
                Thread.Sleep(2000);
                Console.WriteLine($"\n Generating reciept");

                for ( int i = 0; i < 3; i++)
                {        
                    Thread.Sleep(1000);
                    Console.WriteLine(".");
                }
                Console.ResetColor();
                Console.Clear();

                Car car = rentalManager.GetCarByID(carID);
                string receipt = GenerateRentalReceipt(rentalID, car, hours, basePrice, deposit, totalDue);
                Console.WriteLine(receipt);

                fileHandler.SaveCustomerReceipt(currentCustomer.GetCustomerID(), receipt);

                PauseScreen();
            }
            else
            {
                ShowError("Booking failed. Please try again.");
            }
        }

        private string GenerateRentalReceipt(string rentalID, Car car, int hours, decimal basePrice, decimal deposit, decimal total)
        {
            StringBuilder receipt = new StringBuilder();
            receipt.AppendLine("\n╔════════════════════════════════════════════════════════════════╗");
            receipt.AppendLine("║                      RENTAL RECEIPT                            ║");
            receipt.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            receipt.AppendLine($"  Rental ID:      {rentalID}");
            receipt.AppendLine($"  Customer:       {currentCustomer.GetName()}");
            receipt.AppendLine($"  Customer ID:    {currentCustomer.GetCustomerID()}");
            receipt.AppendLine($"  Car:            {car.GetModel()} ({car.GetCarID()})");
            receipt.AppendLine($"  Rental Start:   {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            receipt.AppendLine($"  Estimated Hours: {hours}");
            receipt.AppendLine("  ─────────────────────────────────────────────────────────────────");
            receipt.AppendLine($"  Base Price:     ${basePrice:F2}");
            receipt.AppendLine($"  Deposit:        ${deposit:F2}");
            receipt.AppendLine("  ─────────────────────────────────────────────────────────────────");
            receipt.AppendLine($"  TOTAL DUE:      ${total:F2}");
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
                    Console.WriteLine($"  [{rental.GetStatus()}] Rental ID: {rental.GetRentalID()}");
                    Console.WriteLine($"    Car: {car.GetModel()}");
                    Console.WriteLine($"    Period: {rental.GetRentalStartTime():yyyy-MM-dd} to {(rental.GetStatus() == "Completed" ? rental.GetRentalEndTime().ToString("yyyy-MM-dd") : "Ongoing")}");
                    Console.WriteLine($"    Total: ${rental.GetTotalCost():F2}");
                    Console.WriteLine("  ─────────────────────────────────────────────────────────────────");
                }
            }

            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        // 🏢 ADMIN DASHBOARD
        // ═══════════════════════════════════════════════════════════

        private void AdminDashboard()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║  ADMIN DASHBOARD - Welcome, {currentAgent.GetName(),-29} ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\n  1. View Fleet Status");
                Console.WriteLine("  2. Process Car Return");
                Console.WriteLine("  3. Add Maintenance Record");
                Console.WriteLine("  4. Complete Maintenance");
                Console.WriteLine("  5. View Maintenance Records");
                Console.WriteLine("  6. Add Cars");
                Console.WriteLine("  7. View Total Revenue");
                Console.WriteLine("  8. Logout");

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
                    AddCarsMenu();
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

            int available = 0, rented = 0, maintenance = 0;

            Console.WriteLine($"  {"ID",-6} | {"Model",-17} | {"Category",-8} | {"Status",-18}");
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────");

            foreach (Car car in allCars)
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

                Console.Write($"  {car.GetCarID(),-6} | {car.GetModel(),-17} | {car.GetCategory(),-8} | ");
                Console.ForegroundColor = statusColor;
                Console.WriteLine($"{status,-18}");
                Console.ResetColor();
            }

            Console.WriteLine("  ─────────────────────────────────────────────────────────────────");
            Console.WriteLine($"  Total: {allCars.Count} | Available: {available} | Rented: {rented} | Maintenance: {maintenance}");

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
            if (maintenanceManager.AddMaintenance(carID, techName, description, currentAgent.GetAgentID(), out message))
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
                    Console.WriteLine($"  [{m.GetStatus()}] {m.GetMaintenanceID()}");
                    Console.ResetColor();
                    Console.WriteLine($"    Car: {m.GetCarID()}");
                    Console.WriteLine($"    Technician: {m.GetTechnicianName()}");
                    Console.WriteLine($"    Date: {m.GetMaintenanceDate():yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"    Description: {m.GetDescription()}");
                    Console.WriteLine("  ─────────────────────────────────────────────────────────────────");
                }
            }

            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        // 🚗 ADD CARS MENU (SINGLE & BULK WITH VALIDATION)
        // ═══════════════════════════════════════════════════════════

        private void AddCarsMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        ADD CARS                                ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\n  1. Add Single Car");
            Console.WriteLine("  2. Add Cars from File/CSV Text");
            Console.WriteLine("  3. Cancel");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n  Choice: ");
            Console.ResetColor();

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                AddSingleCar();
            }
            else if (choice == "2")
            {
                AddBulkCars();
            }
        }

        private void AddSingleCar()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                          ADD SINGLE CAR                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            List<Car> cars = fileHandler.ReadCars();

            // --- SMART ID START ---
            int maxFound = 10;
            foreach (Car existingCar in cars)
            {
                string idPart = existingCar.GetCarID().Replace("C", "");
                if (int.TryParse(idPart, out int currentNum))
                {
                    if (currentNum > maxFound) maxFound = currentNum;
                }
            }
            string carID = $"C{(maxFound + 1):D3}";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  [SYSTEM] Auto-Assigned ID: {carID}");
            Console.ResetColor();
            // --- SMART ID END ---

            Console.Write("  Model: ");
            string model = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(model))
            {
                ShowError("Model cannot be empty!");
                return;
            }

            // IDIOT-PROOF CATEGORY VALIDATION
            string category = "";
            bool validCategory = false;
            while (!validCategory)
            {
                Console.WriteLine("\n  Category:");
                Console.WriteLine("    1. SUV");
                Console.WriteLine("    2. Sedan");
                Console.WriteLine("    3. Van");
                Console.Write("\n  Choice: ");
                string catChoice = Console.ReadLine();

                switch (catChoice)
                {
                    case "1": category = "SUV"; validCategory = true; break;
                    case "2": category = "Sedan"; validCategory = true; break;
                    case "3": category = "Van"; validCategory = true; break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n  ✗ Invalid choice! Select 1-3.");
                        Console.ResetColor();
                        break;
                }
            }

            // Fuel Type Validation
            string fuelType = "";
            bool validFuel = false;
            while (!validFuel)
            {
                Console.WriteLine("\n  Fuel Type:");
                Console.WriteLine("    1. Dual Motor");
                Console.WriteLine("    2. Standard Engine");
                Console.WriteLine("    3. EV");
                Console.Write("\n  Choice: ");
                string fuelChoice = Console.ReadLine();

                switch (fuelChoice)
                {
                    case "1": fuelType = "Dual Motor"; validFuel = true; break;
                    case "2": fuelType = "Standard Engine"; validFuel = true; break;
                    case "3": fuelType = "EV"; validFuel = true; break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n  ✗ Invalid choice! Select 1-3.");
                        Console.ResetColor();
                        break;
                }
            }

            Console.Write("\n  Hourly Rate: $");
            string rateInput = Console.ReadLine();

            if (!decimal.TryParse(rateInput, out decimal hourlyRate) || hourlyRate <= 0)
            {
                ShowError("Invalid hourly rate!");
                return;
            }

            // Final Creation
            Car newCar = new Car(carID, model, category, fuelType, hourlyRate);
            cars.Add(newCar);
            fileHandler.SaveCars(cars);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✓ Car '{model}' saved with ID {carID}!");
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

            // Note: 
            Console.WriteLine("\nPaste CSV data (format: Model,Category,FuelType,Rate)");
            Console.WriteLine("Example: Toyota Fortuner,SUV,Standard Engine,50.00");
            Console.WriteLine("\nValid Categories: SUV, Sedan, Van");
            Console.WriteLine("\nType 'END' on a new line when done:\n");

            List<string> csvLines = new List<string>();
            while (true)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.ToUpper() == "END") break;
                csvLines.Add(line);
            }

            if (csvLines.Count == 0) { ShowError("No data provided!"); return; }

            Console.WriteLine("\n  ═══════════════ VALIDATION RESULTS ═══════════════\n");

            List<Car> existingCars = fileHandler.ReadCars();
            int successCount = 0;
            int failCount = 0;

            // --- SMART ID START ---

            // Set the absolute minimum starting number
            int maxFound = 10;

            //  Look at the existing cars to see if we should start higher
            if (existingCars.Count > 0)
            {
                foreach (Car car in existingCars)
                {
                    // Get the "020" from "C020" and turn it into a number
                    string idPart = car.GetCarID().Replace("C", "");
                    if (int.TryParse(idPart, out int currentNum))
                    {
                        if (currentNum > maxFound)
                        {
                            maxFound = currentNum; // Update if we find a higher ID
                        }
                    }
                }
            }

            // The first new car will be maxFound + 1 (e.g., 11 if list was empty)
            int nextIdNumber = maxFound + 1;

            foreach (string line in csvLines)
            {
                string[] parts = line.Split(',');

                // Now checking for 4 columns instead of 5 (No ID provided)
                if (parts.Length < 4)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAILED] Missing Columns: {line}");
                    Console.ResetColor();
                    failCount++;
                    continue;
                }

                // Generate the ID automatically
                string carID = $"C{nextIdNumber:D3}"; // Formats to C011, C012, etc.
                string model = parts[0].Trim();
                string category = parts[1].Trim();
                string fuelType = parts[2].Trim();
                string rateStr = parts[3].Trim();

                //  IDIOT-PROOF Category Check
                string upperCat = category.ToUpper();
                if (upperCat != "SUV" && upperCat != "SEDAN" && upperCat != "VAN")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAILED] Invalid Category '{category}': {line}");
                    Console.ResetColor();
                    failCount++;
                    continue;
                }

                // Fuel Type Check
                if (fuelType != "Dual Motor" && fuelType != "Standard Engine" && fuelType != "EV")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAILED] Invalid Fuel Type '{fuelType}': {line}");
                    Console.ResetColor();
                    failCount++;
                    continue;
                }

                //  Rate Check
                if (!decimal.TryParse(rateStr, out decimal hourlyRate) || hourlyRate <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAILED] Invalid Rate '{rateStr}': {line}");
                    Console.ResetColor();
                    failCount++;
                    continue;
                }

                // ALL VALIDATIONS PASSED - ADD CAR
                Car newCar = new Car(carID, model, category, fuelType, hourlyRate);
                existingCars.Add(newCar);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  [SUCCESS] Assigned {carID} -> {model}");
                Console.ResetColor();

                successCount++;
                nextIdNumber++; // Increment number for the NEXT car in the loop
            }

            // Save results
            if (successCount > 0)
            {
                fileHandler.SaveCars(existingCars);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  ✓ {successCount} car(s) added and saved to Admin directory!");
            }

            Console.ResetColor();
            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        // 💰 VIEW TOTAL REVENUE
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
        // 🛠️ UTILITY METHODS
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
} // End of MenuSystem class

     
    
