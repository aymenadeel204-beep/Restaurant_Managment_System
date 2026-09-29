using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class RawMaterial
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "عام"; // لحوم، خضروات، بقوليات، بهارات، معلبات، زيوت، مشروبات، مواد تغليف
    public string Unit { get; set; } = "كجم"; // كجم, لتر, علبة, قطعة, كرتون
    public decimal CurrentQuantity { get; set; }
    public decimal MinLimit { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal AverageCost { get; set; } = 0;
    public decimal SellingPrice { get; set; } = 0;

    public int? PrimarySupplierId { get; set; }
    public Supplier? PrimarySupplier { get; set; }

    public DateTime? LastPurchaseDate { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = "نشط"; // نشط, موقوف

    public List<RecipeItem> RecipeItems { get; set; } = new();
    public List<PurchaseInvoiceItem> PurchaseItems { get; set; } = new();
}
