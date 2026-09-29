using System;

namespace RestaurantSystem.Domain.Models;

public class RecipeItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int RawMaterialId { get; set; }
    public RawMaterial? RawMaterial { get; set; }

    public decimal QuantityRequired { get; set; }
}
