using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class PurchaseReturn
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; } = DateTime.Now;

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int? PurchaseInvoiceId { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    public decimal TotalAmount { get; set; }
    public string SettlementMethod { get; set; } = "خصم من رصيد المورد"; // خصم من رصيد المورد, استرداد نقدي, استرداد بنكي
    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<PurchaseReturnItem> Items { get; set; } = new();
}
