using System;

namespace RestaurantSystem.Domain.Models;

public class Expense
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty; // كهرباء, إيجار, مياه, إنترنت, صيانة, نثريات
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "نقدي"; // نقدي, تحويل بنكي
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int UserId { get; set; }
    public User? User { get; set; }
    public int? ShiftId { get; set; }
    public Shift? Shift { get; set; }
}
