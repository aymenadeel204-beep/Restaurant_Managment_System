using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal DebtBalance { get; set; }
    public decimal CreditLimit { get; set; } = 5000.00m;
    public string Notes { get; set; } = string.Empty;
}
