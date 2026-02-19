using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace TicketManagerAssignment
{
    public class TicketManager
    {
        private readonly List<Ticket> _tickets = new();

        public void AddTicket(Ticket t)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            if (FindTicket(t.Id) != null)
                throw new InvalidOperationException($"A ticket with {t.Id} already exists");

            _tickets.Add(t);
        }

        public bool RemoveTicket(string id)
        {
            var t = FindTicket(id);
            if (t == null) return false;

            _tickets.Remove(t);
            return true;
        }

        public Ticket? FindTicket(string id)
        {
            foreach (var t in _tickets)
            {
                if (string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase))
                    return t;
            }
            return null;
        }

        public void DisplayAllTickets()
        {
            if (_tickets.Count == 0)
            {
                Console.WriteLine("No tickets found.");
                return;
            }

            Console.WriteLine("\n--- Ticket List ---");
            foreach (var t in _tickets)
                Console.WriteLine(t.GetSummary());
        }

        public int GetOpenCount()
        {
            int count = 0;
            foreach (var t in _tickets)
                if (!string.Equals(t.Status, "Closed", StringComparison.OrdinalIgnoreCase))
                    count++;
            return count;
        }

        // ---------- CSV Persistence ----------

        public void SaveTickets(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

            using var sw = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            sw.WriteLine("Id,Description,Priority,Status,DateCreated");
            foreach (var t in _tickets)
            {
                string line = string.Join(",",
                    CsvEscape(t.Id),
                    CsvEscape(t.Description),
                    CsvEscape(t.Priority),
                    CsvEscape(t.Status),
                    CsvEscape(t.DateCreated.ToString("o", CultureInfo.InvariantCulture)) // ISO 8601
                );
                sw.WriteLine(line);
            }
        }

        public void LoadTickets(string path)
        {
            using var sr = new StreamReader(path, Encoding.UTF8);

            _tickets.Clear();

            string? header = sr.ReadLine();
            if (header is null)
                throw new InvalidDataException("File is empty.");

            int lineNo = 1;
            int loaded = 0, skipped = 0;

            while (!sr.EndOfStream)
            {
                string? line = sr.ReadLine();
                lineNo++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var cols = CsvParse(line);
                    if (cols.Count != 5)
                        throw new InvalidDataException($"Expected 5 columns, found {cols.Count}.");

                    string id = cols[0].Trim();
                    string description = cols[1].Trim();
                    string priority = cols[2].Trim();
                    string status = cols[3].Trim();
                    string createdStr = cols[4].Trim();

                    if (!DateTime.TryParse(createdStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var when))
                        throw new InvalidDataException("Invalid DateCreated format.");

                    var t = new Ticket(id, description, priority, status);

                    // Override the auto-generated date with the one from CSV
                    typeof(Ticket).GetProperty(nameof(Ticket.DateCreated))!
                                  .SetValue(t, when);

                    AddTicket(t);
                    loaded++;
                }
                catch (Exception ex)
                {
                    skipped++;
                    Console.WriteLine($"Skipped line {lineNo}: {ex.Message}");
                }
            }

            Console.WriteLine($"Load complete. Loaded: {loaded}, Skipped: {skipped}.");
        }

        // CSV helpers (unchanged)
        private static string CsvEscape(string input)
        {
            bool needsQuotes = input.Contains(',') || input.Contains('"') || input.Contains('\n') || input.Contains('\r');
            if (!needsQuotes) return input;
            return "\"" + input.Replace("\"", "\"\"") + "\"";
        }

        private static List<string> CsvParse(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            result.Add(sb.ToString());
            return result;
        }
    }
}