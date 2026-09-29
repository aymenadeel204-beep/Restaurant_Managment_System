using System;

namespace RestaurantSystem.Domain.Models;

public class RestaurantTable
{
    public int Id { get; set; }
    public int TableNumber { get; set; }
    public string TableName { get; set; } = string.Empty; // e.g. طاولة 1
    public int Capacity { get; set; } = 4;
    public string Status { get; set; } = "متاحة"; // متاحة, مشغولة, محجوزة
    public string CurrentOrderNumber { get; set; } = string.Empty;
}
