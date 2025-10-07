using System;
using System.Collections.Generic;

namespace Clinix.Domain.Entities;

/// <summary>
/// Represents an inventory item (medicine, equipment, consumable)
/// </summary>
public class InventoryItem
    {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Medicine, Equipment, Consumable
    public string Category { get; set; } = string.Empty; // Analgesic, Surgical, etc.
    public string Unit { get; set; } = string.Empty; // Tablets, Bottles, Pieces
    public int MinStock { get; set; } = 0; // Alert threshold
    public string StockLocation { get; set; } = "Main Store";
    public int CurrentStock { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for stock transactions
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
    }

