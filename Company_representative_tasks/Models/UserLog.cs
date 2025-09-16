using System;
namespace Argent_Company.Models
{
    public class UserLog
    {
        public int Id { get; set; }
        public int? AgentId { get; set; }
        public string? EventType { get; set; } // "Login" or "Register"
        public DateTime EventTime { get; set; }
        public string? UserEmail { get; set; }
    }
}