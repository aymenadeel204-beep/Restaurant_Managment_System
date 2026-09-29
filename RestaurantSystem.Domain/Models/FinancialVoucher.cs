using System;

namespace RestaurantSystem.Domain.Models;

public class FinancialVoucher
{
    public int Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public string Type { get; set; } = "سند قبض"; // سند قبض, سند صرف
    public decimal Amount { get; set; }
    public string PaidToOrFrom { get; set; } = string.Empty; // استلمنا من / صرفنا إلى
    public string Reason { get; set; } = string.Empty; // وذلك مقابل...
    public string PaymentMethod { get; set; } = "نقدي"; // نقدي, تحويل بنكي, شبكة
    public DateTime VoucherDate { get; set; } = DateTime.Now;

    public int UserId { get; set; }
    public User? User { get; set; }
}
