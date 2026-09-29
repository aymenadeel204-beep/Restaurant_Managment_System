using System;

namespace RestaurantSystem.Domain.Models;

public class SystemSetting
{
    public int Id { get; set; }
    public string RestaurantName { get; set; } = "مطعم المقبولي";
    public string Phone { get; set; } = "0501234567";
    public string Address { get; set; } = "شارع الملك فهد، الرياض";
    public string Currency { get; set; } = "ريال سعودي (ر.س)";
    public decimal TaxRate { get; set; } = 15.0m;
    public string LogoPath { get; set; } = string.Empty;
}
