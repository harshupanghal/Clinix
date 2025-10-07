using System;

namespace Clinix.Application.Dtos;

public class InventoryItemDto
    {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinStock { get; set; }
    public string StockLocation { get; set; } = string.Empty;
    }