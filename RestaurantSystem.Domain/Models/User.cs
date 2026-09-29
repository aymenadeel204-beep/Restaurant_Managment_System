using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Cashier"; // Admin, Cashier, Kitchen, Inventory, Accountant
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// عند التفعيل يُجبر المستخدم على تغيير كلمة مروره عند أول دخول.
    /// يُضبط على true للمستخدمين الجدد وعند إعادة تعيين كلمة المرور.
    /// </summary>
    public bool ForcePasswordChange { get; set; } = false;

    public List<Shift> Shifts { get; set; } = new();
    public List<Expense> Expenses { get; set; } = new();
}
