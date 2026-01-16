using System;
using System.Collections.Generic;
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

        //  Constructor
        public MenuSystem()
        {
            Console.OutputEncoding = Encoding.UTF8; // so special character is recognized

            fileHandler = new FileHandler();
            rentalManager = new RentalManager(fileHandler);
            maintenanceManager = new MaintenanceManager(fileHandler);
            customers = fileHandler.ReadCustomers();
            agents = fileHandler.ReadAgents();
            currentCustomer = null;
            currentAgent = null;
            isCustomerLoggedIn = false;
        }

        //  Start the system
        public void Start()
        {
            UTF32Encoding utf32 = new UTF32Encoding(); // To support special characters
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

            Console.SetCursorPosition(46, 27);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();

            Console.SetCursorPosition(50, 29);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nLoading");

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

                Console.WriteLine("\n1. Customer Login");
                Console.WriteLine("\n2. Company Agent Login");
                Console.WriteLine("\n3. Register as New Customer");
                Console.WriteLine("\n4. Exit");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nChoice: ");
                Console.ResetColor();

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        if (LoginUser("Customer"))
                        {
                            CustomerDashboard();
                        }
                        break;
                    case "2":
                        if (LoginUser("Agent"))
                        {
                            AdminDashboard();
                        }
                        break;
                    case "3":
                        RegisterNewCustomer();
                        break;
                    case "4":
                        ExitSystem();
                        return;
                    default:
                        ShowError("Invalid choice!");
                        break;
                }
            }
        }

        private bool LoginUser(string userType)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║                    {userType.ToUpper()} LOGIN                          ║");
            Console.WriteLine($"╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            Console.Write("User ID: ");
            string userID = Console.ReadLine();

            Console.Write("Password: ");
            string password = Console.ReadLine();

            if (userType == "Customer")
            {
                Customer customer = null;
                foreach (Customer c in customers)
                {
                    if (c.GetCustomerID() == userID)
                    {
                        customer = c;
                        break; // Stop searching once found
                    }
                }
                if (customer != null && customer.ValidatePassword(password))
                {
                    currentCustomer = customer;
                    isCustomerLoggedIn = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Welcome, {customer.GetName()}!");
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
                        break; // Stop searching once found
                    }
                }

                if (agent != null && agent.ValidatePassword(password))
                {
                    currentAgent = agent;
                    isCustomerLoggedIn = false;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Welcome, {agent.GetName()}!");
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

            Console.Write("Name: ");
            string name = Console.ReadLine();

            Console.Write("Password: ");
            string password = Console.ReadLine();

            // Generate customer ID
            string customerID = "C" + (customers.Count + 1).ToString("D3");

            // Create new customer
            Customer newCustomer = new Customer(customerID, name, password);
            customers.Add(newCustomer);

            // Save to file
            fileHandler.SaveCustomers(customers);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✓ Registration successful!");
            Console.WriteLine($"Your Customer ID is: {customerID}");
            Console.ResetColor();
            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        // 👤 CUSTOMER DASHBOARD
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

                Console.WriteLine("\n1. Browse Available Cars");
                Console.WriteLine("\n2. Rent a Car");
                Console.WriteLine("\n3. View My Active Rentals");
                Console.WriteLine("\n4. View Rental History");
                Console.WriteLine("\n5. Logout");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nChoice: ");
                Console.ResetColor();

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        BrowseCars();
                        break;
                    case "2":
                        RentCar();
                        break;
                    case "3":
                        ViewActiveRentals();
                        break;
                    case "4":
                        ViewRentalHistory();
                        break;
                    case "5":
                        currentCustomer = null;
                        return;
                    default:
                        ShowError("Invalid choice!");
                        break;
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

            Console.WriteLine("\nCategory Filter:");
            Console.WriteLine("\n1. All");
            Console.WriteLine("\n2. SUV");
            Console.WriteLine("\n3. Sedan");
            Console.WriteLine("\n4. Van");
            Console.Write("\nChoice: ");
            string catChoice = Console.ReadLine();

            string categoryFilter = "ALL";
            switch (catChoice)
            {
                case "2": categoryFilter = "SUV"; break;
                case "3": categoryFilter = "Sedan"; break;
                case "4": categoryFilter = "Van"; break;
            }

            Console.WriteLine("\nFuel Type Filter:");
            Console.WriteLine("\n1. All");
            Console.WriteLine("\n2. Dual Motor");
            Console.WriteLine("\n3. Standard Engine");
            Console.WriteLine("\n4. EV");
            Console.Write("\nChoice: ");
            string fuelChoice = Console.ReadLine();

            string fuelFilter = "ALL";
            switch (fuelChoice)
            {
                case "2": fuelFilter = "Dual Motor"; break;
                case "3": fuelFilter = "Standard Engine"; break;
                case "4": fuelFilter = "EV"; break;
            }

            DisplayAvailableCars(categoryFilter, fuelFilter);
        }

        private void DisplayAvailableCars(string categoryFilter, string fuelFilter)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"════════════════════════════════════════════════════════════════");
            Console.WriteLine($"  AVAILABLE CARS (CATEGORY: {categoryFilter}, FUEL: {fuelFilter})");
            Console.WriteLine($"════════════════════════════════════════════════════════════════");
            Console.ResetColor();

            List<Car> availableCars = rentalManager.GetAvailableCars(categoryFilter, fuelFilter);

            if (availableCars.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n✗ No cars available with selected filters");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{"ID",-6} | {"Model",-17} | {"Category",-8} | {"Fuel Type",-15} | {"Price/Hour",-10}");
                Console.WriteLine("─────────────────────────────────────────────────────────────────");

                foreach (Car car in availableCars)
                {
                    Console.WriteLine(car.ToTableRow());
                }
                Console.WriteLine("─────────────────────────────────────────────────────────────────");
            }

            PauseScreen();
        }

        // Part 2 of MenuSystem.cs - Add these methods to the MenuSystem class

        private void RentCar()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        RENT A CAR                              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            // Show available cars first
            List<Car> availableCars = rentalManager.GetAvailableCars();

            if (availableCars.Count == 0)
            {
                ShowError("No cars available at the moment");
                return;
            }

            Console.WriteLine($"{"ID",-6} | {"Model",-17} | {"Category",-8} | {"Fuel Type",-15} | {"Price/Hour",-10}");
            Console.WriteLine("─────────────────────────────────────────────────────────────────");
            foreach (Car car in availableCars)
            {
                Console.WriteLine(car.ToTableRow());
            }
            Console.WriteLine("─────────────────────────────────────────────────────────────────\n");

            Console.Write("Enter Car ID to Rent (or 0 to cancel): ");
            string carID = Console.ReadLine();

            if (carID == "0") return;

            // Check conflict
            string conflict = rentalManager.CheckRentalConflict(carID);
            if (conflict != "OK")
            {
                ShowError($"Cannot rent: {conflict}");
                return;
            }

            Console.Write("How many hours do you plan to rent? ");
            if (!int.TryParse(Console.ReadLine(), out int hours) || hours <= 0)
            {
                ShowError("Invalid number of hours");
                return;
            }

            // Calculate estimate
            decimal basePrice, deposit;
            decimal totalDue = rentalManager.CalculateEstimate(carID, hours, out basePrice, out deposit);

            // Show estimate
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n>>> RENTAL ESTIMATE <<<");
            Console.ResetColor();
            Console.WriteLine($"Base Price:  ${basePrice:F2}");
            Console.WriteLine($"Deposit:     ${deposit:F2}");
            Console.WriteLine($"─────────────────────");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Total Due:   ${totalDue:F2}");
            Console.ResetColor();

            Console.Write("\nConfirm Booking? (Y/N): ");
            string confirm = Console.ReadLine().ToUpper();

            if (confirm != "Y")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n⚠ Booking cancelled");
                Console.ResetColor();
                PauseScreen();
                return;
            }

            // Confirm booking
            string rentalID;
            if (rentalManager.ConfirmBooking(currentCustomer.GetCustomerID(), carID, hours, out rentalID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✓ Booking confirmed!");
                Console.ResetColor();

                // Generate receipt
                Car car = rentalManager.GetCarByID(carID);
                string receipt = GenerateRentalReceipt(rentalID, car, hours, basePrice, deposit, totalDue);
                Console.WriteLine(receipt);

                // Save receipt to customer folder
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
            receipt.AppendLine($"Rental ID:      {rentalID}");
            receipt.AppendLine($"Customer:       {currentCustomer.GetName()}");
            receipt.AppendLine($"Customer ID:    {currentCustomer.GetCustomerID()}");
            receipt.AppendLine($"Car:            {car.GetModel()} ({car.GetCarID()})");
            receipt.AppendLine($"Rental Start:   {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            receipt.AppendLine($"Estimated Hours: {hours}");
            receipt.AppendLine("─────────────────────────────────────────────────────────────────");
            receipt.AppendLine($"Base Price:     ${basePrice:F2}");
            receipt.AppendLine($"Deposit:        ${deposit:F2}");
            receipt.AppendLine($"─────────────────────────────────────────────────────────────────");
            receipt.AppendLine($"TOTAL DUE:      ${total:F2}");
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
                Console.WriteLine("You have no active rentals.");
                Console.ResetColor();
            }
            else
            {
                foreach (Rental rental in activeRentals)
                {
                    Car car = rentalManager.GetCarByID(rental.GetCarID());
                    Console.WriteLine($"Rental ID:       {rental.GetRentalID()}");
                    Console.WriteLine($"Car:             {car.GetModel()} ({car.GetCarID()})");
                    Console.WriteLine($"Started:         {rental.GetRentalStartTime():yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"Estimated Hours: {rental.GetEstimatedHours()}");
                    Console.WriteLine($"Total Cost:      ${rental.GetTotalCost():F2}");
                    Console.WriteLine("─────────────────────────────────────────────────────────────────");
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
                Console.WriteLine("No rental history found.");
                Console.ResetColor();
            }
            else
            {
                foreach (Rental rental in allRentals)
                {
                    Car car = rentalManager.GetCarByID(rental.GetCarID());
                    Console.WriteLine($"[{rental.GetStatus()}] Rental ID: {rental.GetRentalID()}");
                    Console.WriteLine($"  Car: {car.GetModel()}");
                    Console.WriteLine($"  Period: {rental.GetRentalStartTime():yyyy-MM-dd} to {(rental.GetStatus() == "Completed" ? rental.GetRentalEndTime().ToString("yyyy-MM-dd") : "Ongoing")}");
                    Console.WriteLine($"  Total: ${rental.GetTotalCost():F2}");
                    Console.WriteLine("─────────────────────────────────────────────────────────────────");
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

                Console.WriteLine("\n1. View Fleet Status");
                Console.WriteLine("2. Process Car Return");
                Console.WriteLine("3. Add Maintenance Record");
                Console.WriteLine("4. Complete Maintenance");
                Console.WriteLine("5. View Maintenance Records");
                Console.WriteLine("6. Logout");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nChoice: ");
                Console.ResetColor();

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ViewFleetStatus();
                        break;
                    case "2":
                        ProcessReturn();
                        break;
                    case "3":
                        AddMaintenance();
                        break;
                    case "4":
                        CompleteMaintenance();
                        break;
                    case "5":
                        ViewMaintenanceRecords();
                        break;
                    case "6":
                        currentCustomer= null;
                        return;
                    default:
                        ShowError("Invalid choice!");
                        break;
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

            Console.WriteLine($"{"ID",-6} | {"Model",-17} | {"Category",-8} | {"Status",-18}");
            Console.WriteLine("─────────────────────────────────────────────────────────────────");

            foreach (Car car in allCars)
            {
                string status = car.GetStatus();

                if (status == "Available") available++;
                else if (status == "Rented") rented++;
                else if (status == "Under Maintenance") maintenance++;

                ConsoleColor statusColor = status == "Available" ? ConsoleColor.Green :
                                          status == "Rented" ? ConsoleColor.Yellow :
                                          ConsoleColor.Red;

                Console.Write($"{car.GetCarID(),-6} | {car.GetModel(),-17} | {car.GetCategory(),-8} | ");
                Console.ForegroundColor = statusColor;
                Console.WriteLine($"{status,-18}");
                Console.ResetColor();
            }

            Console.WriteLine("─────────────────────────────────────────────────────────────────");
            Console.WriteLine($"Total: {allCars.Count} | Available: {available} | Rented: {rented} | Maintenance: {maintenance}");

            PauseScreen();
        }
        // Part 3 of MenuSystem.cs - Add these methods to the MenuSystem class

        private void ProcessReturn()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    PROCESS CAR RETURN                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            // Show rented cars
            rentalManager.ReloadData();
            List<Car> rentedCars = rentalManager.GetAllCars().FindAll(c => c.GetStatus() == "Rented");

            if (rentedCars.Count == 0)
            {
                ShowError("No cars currently rented");
                return;
            }

            Console.WriteLine("Currently Rented Cars:");
            Console.WriteLine($"{"ID",-6} | {"Model",-17} | {"Renter ID",-12} | {"Est. Hours",-10}");
            Console.WriteLine("─────────────────────────────────────────────────────────────────");

            foreach (Car car in rentedCars)
            {
                Console.WriteLine($"{car.GetCarID(),-6} | {car.GetModel(),-17} | {car.GetCurrentRenterID(),-12} | {car.GetEstimatedHours(),-10}");
            }
            Console.WriteLine("─────────────────────────────────────────────────────────────────\n");

            Console.Write("Enter Car ID to process return (or 0 to cancel): ");
            string carID = Console.ReadLine();

            if (carID == "0") return;

            Console.Write("Enter actual hours used: ");
            if (!int.TryParse(Console.ReadLine(), out int actualHours) || actualHours <= 0)
            {
                ShowError("Invalid number of hours");
                return;
            }

            // Process return with early return formula
            decimal finalCost, discount;
            string message;

            if (rentalManager.ProcessReturn(carID, actualHours, out finalCost, out discount, out message))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✓ Return processed successfully!\n");
                Console.ResetColor();

                Car car = rentalManager.GetCarByID(carID);

                Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                      RETURN SUMMARY                            ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
                Console.WriteLine($"Car:            {car.GetModel()} ({carID})");
                Console.WriteLine($"Actual Hours:   {actualHours}");

                if (discount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Discount:       ${discount:F2}");
                    Console.ResetColor();
                }

                Console.WriteLine($"Final Cost:     ${finalCost:F2}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nStatus: {message}");
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

            // Show available cars and cars under maintenance
            maintenanceManager.ReloadData();
            rentalManager.ReloadData();
            List<Car> availableCars = rentalManager.GetAllCars().FindAll(c =>
                c.GetStatus() == "Available" || c.GetStatus() == "Under Maintenance");

            Console.WriteLine($"{"ID",-6} | {"Model",-17} | {"Status",-18}");
            Console.WriteLine("─────────────────────────────────────────────────────────────────");

            foreach (Car car in availableCars)
            {
                Console.WriteLine($"{car.GetCarID(),-6} | {car.GetModel(),-17} | {car.GetStatus(),-18}");
            }
            Console.WriteLine("─────────────────────────────────────────────────────────────────\n");

            Console.Write("Enter Car ID for maintenance (or 0 to cancel): ");
            string carID = Console.ReadLine();

            if (carID == "0") return;

            Console.Write("Technician Name: ");
            string techName = Console.ReadLine();

            Console.Write("Description: ");
            string description = Console.ReadLine();

            string message;
            if (maintenanceManager.AddMaintenance(carID, techName, description, out message))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ {message}");
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

            Console.WriteLine("In-Progress Maintenance:");
            Console.WriteLine($"{"Maint. ID",-10} | {"Car ID",-8} | {"Technician",-15} | {"Description",-25}");
            Console.WriteLine("─────────────────────────────────────────────────────────────────");

            foreach (Maintenance m in inProgress)
            {
                string desc = m.GetDescription().Length > 25 ? m.GetDescription().Substring(0, 22) + "..." : m.GetDescription();
                Console.WriteLine($"{m.GetMaintenanceID(),-10} | {m.GetCarID(),-8} | {m.GetTechnicianName(),-15} | {desc,-25}");
            }
            Console.WriteLine("─────────────────────────────────────────────────────────────────\n");

            Console.Write("Enter Maintenance ID to complete (or 0 to cancel): ");
            string maintID = Console.ReadLine();

            if (maintID == "0") return;

            string message;
            if (maintenanceManager.CompleteMaintenance(maintID, out message))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ {message}");
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
                Console.WriteLine("No maintenance records found.");
                Console.ResetColor();
            }
            else
            {
                foreach (Maintenance m in allMaintenance)
                {
                    ConsoleColor statusColor = m.GetStatus() == "Completed" ? ConsoleColor.Green : ConsoleColor.Yellow;

                    Console.ForegroundColor = statusColor;
                    Console.WriteLine($"[{m.GetStatus()}] {m.GetMaintenanceID()}");
                    Console.ResetColor();
                    Console.WriteLine($"  Car: {m.GetCarID()}");
                    Console.WriteLine($"  Technician: {m.GetTechnicianName()}");
                    Console.WriteLine($"  Date: {m.GetMaintenanceDate():yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"  Description: {m.GetDescription()}");
                    Console.WriteLine("─────────────────────────────────────────────────────────────────");
                }
            }

            PauseScreen();
        }

        // ═══════════════════════════════════════════════════════════
        // 🛠️ UTILITY METHODS
        // ═══════════════════════════════════════════════════════════

        private void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ {message}");
            Console.ResetColor();
            PauseScreen();
        }

        private void PauseScreen()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nPress any key to continue...");
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
    } // End of MenuSystem class
}
     
    
