using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem
{
    // Company Agent Class
    internal class CompanyAgent
    {
        private string agentID;
        private string name;
        private string password;

        // Constructor
        public CompanyAgent(string id, string name, string password)
        {
            this.agentID = id;
            this.name = name;
            this.password = password;
        }

        //  Parse from CSV
        public static bool TryParseCsv(string csvLine, out CompanyAgent agent)
        {
            agent = null;
            try
            {
                string[] parts = csvLine.Split(',');
                if (parts.Length < 3)
                    return false;

                string id = parts[0].Trim();
                string name = parts[1].Trim();
                string password = parts[2].Trim();

                agent = new CompanyAgent(id, name, password);
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
            return $"{agentID},{name},{password}";
        }

        // Validate password
        public bool ValidatePassword(string inputPassword)
        {
            return password == inputPassword;
        }

        // Getters
        public string GetAgentID() { return agentID; }
        public string GetName() { return name; }
    }
}