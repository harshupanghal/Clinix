using Clinix.Domain.Entities.Inventory;

namespace Clinix.Application.Interfaces.ServiceInterfaces;

public interface IInventoryService
    {
    Task<InventoryItem> AddItemAsync(InventoryItem item);
    Task<InventoryItem> UpdateItemAsync(InventoryItem item);
    Task<IEnumerable<InventoryItem>> GetInventoryAsync();
    Task<InventoryItem?> GetItemByIdAsync(int itemId);

    Task<InventoryTransaction> StockInAsync(int itemId, int quantity, string user, string? description = null);
    Task<InventoryTransaction> StockOutAsync(int itemId, int quantity, string user, string? description = null);
    Task DeleteItemAsync(int itemId);
    }

