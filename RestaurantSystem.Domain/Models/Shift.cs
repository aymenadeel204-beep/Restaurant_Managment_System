using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class Shift
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal CashSales { get; set; }
    public decimal CardSales { get; set; }
    public decimal BankSales { get; set; }
    public decimal TotalExpenses { get; set; }

    public decimal ExpectedCash { get; set; }
    public decimal ActualCash { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal Discrepancy { get; set; }
    public bool IsClosed { get; set; } = false;
    public string Notes { get; set; } = string.Empty;

    public List<Order> Orders { get; set; } = new();
}
