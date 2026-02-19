using System;
using System.IO;

namespace TicketManagerAssignment
{
    internal static class Program
    {
        private static void Main()
        {
            var manager = new TicketManager();

            Console.WriteLine("=== IT Support Ticket Manager ===");
            Console.WriteLine("Welcome! Manage your support tickets below.\n");

            bool running = true;
            while (running)
            {
                Console.WriteLine("\nMenu:");
                Console.WriteLine("1. Add Ticket");
                Console.WriteLine("2. Remove Ticket");
                Console.WriteLine("3. Display All Tickets");
                Console.WriteLine("4. Close Ticket");
                Console.WriteLine("5. Reopen Ticket");
                Console.WriteLine("6. Save Tickets to File");
                Console.WriteLine("7. Load Tickets from File");
                Console.WriteLine("8. Show Open Ticket Count");
                Console.WriteLine("9. Exit");
                Console.Write("Choose: ");
                string? choice = Console.ReadLine()?.Trim();

                try
                {
                    switch (choice)
                    {
                        case "1": AddTicketMenu(manager); break;
                        case "2": RemoveTicketMenu(manager); break;
                        case "3": manager.DisplayAllTickets(); break;
                        case "4":
                        case "5": CloseOrReopenTicketMenu(manager, choice == "4"); break;
                        case "6": SaveMenu(manager); break;
                        case "7": LoadMenu(manager); break;
                        case "8": Console.WriteLine($"Open tickets: {manager.GetOpenCount()}"); break;
                        case "9": running = false; break;
                        default: Console.WriteLine("Invalid option."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            Console.WriteLine("\nThank you for using the IT Support Ticket Manager. Goodbye!");
        }

        private static void AddTicketMenu(TicketManager manager)
        {
            Console.WriteLine("\n--- Add New Ticket ---");
            Console.WriteLine("(Enter 'cancel' at any prompt to return to menu)\n");

            while (true)
            {
                // 1. Ticket ID
                string id;
                while (true)
                {
                    Console.Write("Enter Ticket ID (e.g., T1001): ");
                    id = Console.ReadLine()?.Trim() ?? "";

                    if (string.Equals(id, "cancel", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (string.IsNullOrWhiteSpace(id))
                    {
                        Console.WriteLine("Error: ID cannot be empty. Try again.");
                        continue;
                    }

                    if (manager.FindTicket(id) != null)
                    {
                        Console.WriteLine($"Error: A ticket with ID '{id}' already exists. Choose a different ID.");
                        continue;
                    }

                    break; // ID looks good
                }

                // 2. Description
                string desc;
                while (true)
                {
                    Console.Write("Enter Description: ");
                    desc = Console.ReadLine()?.Trim() ?? "";

                    if (string.Equals(desc, "cancel", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (string.IsNullOrWhiteSpace(desc))
                    {
                        Console.WriteLine("Error: Description cannot be empty. Try again.");
                        continue;
                    }

                    break;
                }

                // 3. Priority
                string priority;
                while (true)
                {
                    Console.Write("Enter Priority (Low / Medium / High): ");
                    string input = Console.ReadLine()?.Trim() ?? "";
                    priority = NormalizeCase(input);

                    if (string.Equals(input, "cancel", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (Array.IndexOf(Ticket.AllowedPriorities, priority) < 0)
                    {
                        Console.WriteLine($"Error: Priority must be one of: Low, Medium, High. Try again.");
                        continue;
                    }

                    break;
                }

                // 4. Status
                string status;
                while (true)
                {
                    Console.Write("Enter Status (Open / In Progress / Closed): ");
                    string input = Console.ReadLine()?.Trim() ?? "";
                    status = NormalizeCase(input);

                    if (string.Equals(input, "cancel", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (Array.IndexOf(Ticket.AllowedStatuses, status) < 0)
                    {
                        Console.WriteLine($"Error: Status must be one of: Open, In Progress, Closed. Try again.");
                        continue;
                    }

                    break;
                }

                // All inputs are valid → create and add the ticket
                try
                {
                    var ticket = new Ticket(id, desc, priority, status);
                    manager.AddTicket(ticket);
                    Console.WriteLine("\nTicket added successfully!");
                    return; // Done — back to main menu
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nUnexpected error: {ex.Message}");
                    Console.WriteLine("Please try again.\n");
                    // Loop continues → user gets to re-enter everything
                }
            }
        }

        private static void RemoveTicketMenu(TicketManager manager)
        {
            Console.Write("Enter Ticket ID to remove: ");
            string id = Console.ReadLine()?.Trim() ?? "";

            bool removed = manager.RemoveTicket(id);
            Console.WriteLine(removed
                ? $"Ticket {id} removed successfully."
                : $"Ticket {id} not found.");
        }

        private static void CloseOrReopenTicketMenu(TicketManager manager, bool shouldClose)
        {
            while (true)
            {
                Console.Write($"Enter Ticket ID to {(shouldClose ? "close" : "reopen")}: ");
                string id = Console.ReadLine()?.Trim() ?? "";

                var ticket = manager.FindTicket(id);
                if (ticket == null)
                {
                    Console.WriteLine($"Ticket {id} not found.");
                    continue;
                }

                if (shouldClose)
                {
                    ticket.CloseTicket();
                    Console.WriteLine($"Ticket {id} closed.");
                    break;
                }
                else
                {
                    ticket.ReopenTicket();
                    Console.WriteLine($"Ticket {id} reopened.");
                    break;
                }
            }
            
        }

        private static void SaveMenu(TicketManager manager)
        {
            Console.Write("Enter path to save CSV (e.g., tickets.csv): ");
            Console.Write("Enter full path to save CSV (e.g., tickets.csv or C:\\Users\\YourName\\Desktop\\tickets.csv): ");
            string path = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("No path entered. Save cancelled.");
                return;
            }

            // if it looks like a directory (ends with \ or / or no extension)
            if (path.EndsWith(@"\") || path.EndsWith("/") || !Path.HasExtension(path))
            {
                Console.WriteLine("Error: Please include a filename with .csv extension (e.g., tickets.csv).");
                Console.WriteLine("Save cancelled. Try again from the menu.");
                return;
            }

            try
            {
                manager.SaveTickets(path);
                Console.WriteLine($"Saved to {Path.GetFullPath(path)}");
            }

            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Access denied. Common reasons:");
                Console.WriteLine("- You entered a folder path instead of a file (missing filename like tickets.csv)");
                Console.WriteLine("- No permission to write there (try Desktop or Documents)");
                Console.WriteLine("- File/folder is open in another program");
                Console.WriteLine($"Details: {ex.Message}");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("The folder part of the path does not exist. Please check and try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save failed: {ex.Message}");
            }
        }

        private static void LoadMenu(TicketManager manager)
        {
            Console.Write("Enter path to load CSV (e.g., tickets.csv): ");
            string path = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(path)) return;

            if (!File.Exists(path))
            {
                Console.WriteLine("File not found.");
                return;
            }

            try
            {
                manager.LoadTickets(path);
                Console.WriteLine("Load successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load failed: {ex.Message}");
            }
        }

        private static string NormalizeCase(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            var s = input.Trim().ToLowerInvariant();
            if (s == "low") return "Low";
            if (s == "medium" || s == "med") return "Medium";
            if (s == "high") return "High";
            if (s == "open") return "Open";
            if (s == "in progress" || s == "in-progress" || s == "progress") return "In Progress";
            if (s == "closed" || s == "close") return "Closed";
            return input.Trim();
        }
    }
}