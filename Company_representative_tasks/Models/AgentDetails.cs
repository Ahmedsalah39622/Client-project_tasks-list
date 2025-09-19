
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Argent_Company.Models
{
    public class AgentDetails
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        public string? AgentName { get; set; }
        public string? AgentZone { get; set; }
        public string? AgentPhone { get; set; }
    [Precision(18, 2)]
    public decimal MoneyCollected { get; set; }
    }
}

