using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class SalesReturn
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;  // SRET-yyyyMMdd-XXXX
    public DateTime ReturnDate { get; set; } = DateTime.Now;

    // الطلب الأصلي
    public int? OriginalOrderId { get; set; }
    public Order? OriginalOrder { get; set; }
    public string OriginalOrderNumber { get; set; } = string.Empty;

    // العميل (اختياري)
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public decimal TotalAmount { get; set; }

    // طريقة الاسترداد: نقدي، رصيد في الحساب، بطاقة
    public string RefundMethod { get; set; } = "نقدي";

    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<SalesReturnItem> Items { get; set; } = new();
}
