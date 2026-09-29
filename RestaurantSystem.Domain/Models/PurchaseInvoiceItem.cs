using System;

namespace RestaurantSystem.Domain.Models;

public class PurchaseInvoiceItem
{
    public int Id { get; set; }
    public int PurchaseInvoiceId { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    public int RawMaterialId { get; set; }
    public RawMaterial? RawMaterial { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; } = 0;
    public decimal TaxRate { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalPrice { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}
