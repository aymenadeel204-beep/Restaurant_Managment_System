using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string SecondaryPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; } = 0;

    public List<PurchaseInvoice> PurchaseInvoices { get; set; } = new();
    public List<PurchaseReturn> PurchaseReturns { get; set; } = new();
}
