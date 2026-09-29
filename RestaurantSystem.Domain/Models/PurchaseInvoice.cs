using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class PurchaseInvoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string SupplierInvoiceNumber { get; set; } = string.Empty;

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.Now;

    public decimal SubTotal { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }

    public decimal PaidCash { get; set; } = 0;
    public decimal PaidBank { get; set; } = 0;
    public decimal PaidOther { get; set; } = 0;
    public decimal PaidAmount { get; set; } = 0;
    public decimal RemainingAmount { get; set; } = 0;

    public string Status { get; set; } = "مكتملة"; // مكتملة, آجلة, ملغاة
    public string PaymentStatus { get; set; } = "مدفوعة"; // مدفوعة, جزئياً, آجلة, ملغاة
    public string PaymentMethod { get; set; } = "تحويل بنكي"; // نقدي, تحويل بنكي, آجل, متعدد
    public string Notes { get; set; } = string.Empty;

    public int? UserId { get; set; }
    public User? User { get; set; }

    public List<PurchaseInvoiceItem> Items { get; set; } = new();
    public List<PurchaseReturn> PurchaseReturns { get; set; } = new();
}
