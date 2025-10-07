using System;

namespace Clinix.Domain.Entities;

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
    public string? Description { get; set; } // Optional notes
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty; // User performing the transaction
    }

