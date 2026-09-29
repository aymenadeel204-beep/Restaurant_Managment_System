using System;

namespace RestaurantSystem.Domain.Models;

public class StockMovement
{
    public int Id { get; set; }
    public int RawMaterialId { get; set; }
    public RawMaterial RawMaterial { get; set; } = null!;

    // MovementType: "PURCHASE", "SALE", "SALE_CANCEL", "RETURN", "ADJUSTMENT", "WASTE", "OPENING_BALANCE"
    public string MovementType { get; set; } = string.Empty;

    public decimal QuantityChanged { get; set; }
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
