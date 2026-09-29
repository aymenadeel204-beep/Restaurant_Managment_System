using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Type { get; set; } = "صالة"; // صالة، سفري، توصيل
    public string TableOrCustomer { get; set; } = "طاولة 1";

    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }

    public string Status { get; set; } = "جديد"; // جديد، قيد التحضير، جاهز، مكتمل، ملغى
    public string PaymentMethod { get; set; } = "نقدي"; // نقدي، بطاقة، تحويل بنكي، محفظة، آجل
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int ShiftId { get; set; }
    public Shift? Shift { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? TableId { get; set; }
    public RestaurantTable? Table { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();
}
