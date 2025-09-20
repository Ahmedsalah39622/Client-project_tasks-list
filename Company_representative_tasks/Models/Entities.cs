using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Argent_Company.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Users")]
    public class User
    {
    public int Id { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required, EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string PasswordHash { get; set; }
    [Required]
    public required string Role { get; set; } // "Agent" or "Admin"

    // Navigation properties for admin dashboard
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    // Agent details
    public string? AgentZone { get; set; }
    public string? AgentPhone { get; set; }
    [Precision(18, 2)]
    public decimal MoneyCollected { get; set; }
    public string? AgentName { get; set; }
    }
    
    public class Task
    {
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
        public string Status { get; set; } = "نشط"; // "نشط" or "منجز"
        [Precision(18, 2)]
        public decimal? CollectionAmount { get; set; }
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }

    public class Invoice
    {
        public int Id { get; set; }
        
        [Required]
        public required string Number { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }
        
        public DateTime Date { get; set; }
        
        public DateTime? CollectionDate { get; set; }
        
        [Required]
        public string Status { get; set; } = "معلق"; // معلق، تم التحصيل
        
        public int UserId { get; set; }
        public required User User { get; set; }
        
        public string? Notes { get; set; }
        
        public string? CustomerName { get; set; }
    }

    public class Note
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public required User User { get; set; }
        public int TaskId { get; set; }
        public required Task Task { get; set; }
    }
}
