using System;

namespace Clinix.Domain.Entities.Inventory;

/// <summary>
/// Represents a stock movement (IN / OUT / Adjustment)
/// </summary>
public class InventoryTransaction
    {
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    public string TransactionType { get; set; } = string.Empty; // IN / OUT / ADJUSTMENT
    public int Quantity { get; set; }
    public string? Description { get; set; } 
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty; 
    }

