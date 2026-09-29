using System;
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Models;

public class AccountNode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // e.g. 1101, 1102
    public string Name { get; set; } = string.Empty; // e.g. الصندوق الرئيس, حساب البنك
    public string Type { get; set; } = "الأصول"; // الأصول, الالتزامات, حقوق الملكية, الإيرادات, المصروفات
    public decimal Balance { get; set; }

    public int? ParentId { get; set; }
    public AccountNode? Parent { get; set; }
    public List<AccountNode> SubAccounts { get; set; } = new();
}
