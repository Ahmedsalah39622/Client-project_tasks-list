using System;
using System.ComponentModel.DataAnnotations;

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
    }

    public class Task
    {
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
    public int UserId { get; set; }
    public required User User { get; set; }
    }

    public class Invoice
    {
        public int Id { get; set; }
        [Required]
        public required string Number { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    public int UserId { get; set; }
    public required User User { get; set; }
    }

    public class Note
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public required User User { get; set; }
    }
}
