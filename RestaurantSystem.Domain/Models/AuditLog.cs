using System;

namespace RestaurantSystem.Domain.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ActionType { get; set; } = string.Empty; // e.g., "Add", "Update", "Delete", "Login"
    public string EntityName { get; set; } = string.Empty; // e.g., "Orders", "Users"
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public User? User { get; set; }
}
