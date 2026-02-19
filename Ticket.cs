using System;

namespace TicketManagerAssignment
{
    public class Ticket
    {
        public static readonly string[] AllowedPriorities = { "Low", "Medium", "High" };
        public static readonly string[] AllowedStatuses = { "Open", "In Progress", "Closed" };

        private string _id = "";
        private string _description = "";
        private string _priority = "Low";
        private string _status = "Open";

        public string Id
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("ID cannot be empty or whitespace.");
                _id = value.Trim();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Description cannot be empty or whitespace.");
                _description = value.Trim();
            }
        }

        public string Priority
        {
            get => _priority;
            set
            {
                var v = (value ?? "").Trim();
                if (Array.IndexOf(AllowedPriorities, v) < 0)
                    throw new ArgumentException($"Priority must be one of: {string.Join(", ", AllowedPriorities)}");
                _priority = v;
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                var v = (value ?? "").Trim();
                if (Array.IndexOf(AllowedStatuses, v) < 0)
                    throw new ArgumentException($"Status must be one of: {string.Join(", ", AllowedStatuses)}");
                _status = v;
            }
        }

        public DateTime DateCreated { get; private set; } = DateTime.UtcNow;

        // Default constructor
        public Ticket() { }

        // Overloaded constructor
        public Ticket(string id, string description, string priority, string status)
        {
            Id = id;
            Description = description;
            Priority = priority;
            Status = status;
            // DateCreated is already set to UtcNow by property initializer
        }

        public void CloseTicket()
        {
            Status = "Closed";
        }

        public void ReopenTicket()
        {
            Status = "Open";
        }

        public string GetSummary()
        {
            return $"[{Id}] ({Priority}) - \"{Description}\" | Status: {Status} | Created: {DateCreated:yyyy-MM-dd}";
        }
    }
}