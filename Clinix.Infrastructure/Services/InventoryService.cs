using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinix.Application.Interfaces.ServiceInterfaces;
using Clinix.Domain.Entities.Inventory;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinix.Infrastructure.Services;

public class InventoryService : IInventoryService
    {
    private readonly ClinixDbContext _db;

    public InventoryService(ClinixDbContext db)
        {
        _db = db;
        }

    public async Task<InventoryItem> AddItemAsync(InventoryItem item)
        {
        _db.InventoryItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
        }

    public async Task<InventoryItem> UpdateItemAsync(InventoryItem item)
        {
        var existing = await _db.InventoryItems.FindAsync(item.Id);
        if (existing == null) throw new Exception("Item not found");

        existing.Name = item.Name;
        existing.Type = item.Type;
        existing.Category = item.Category;
        existing.Unit = item.Unit;
        existing.MinStock = item.MinStock;
        existing.StockLocation = item.StockLocation;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
        }

    public async Task<IEnumerable<InventoryItem>> GetInventoryAsync()
        => await _db.InventoryItems.ToListAsync();

    public async Task<InventoryItem?> GetItemByIdAsync(int itemId)
        => await _db.InventoryItems.FindAsync(itemId);

    public async Task<InventoryTransaction> StockInAsync(int itemId, int quantity, string user, string? description = null)
        {
        var item = await _db.InventoryItems.FindAsync(itemId);
        if (item == null) throw new Exception("Item not found");

        item.CurrentStock += quantity;
        item.UpdatedAt = DateTime.UtcNow;

        var transaction = new InventoryTransaction
            {
            InventoryItemId = itemId,
            TransactionType = "IN",
            Quantity = quantity,
            Description = description,
            CreatedBy = user
            };

        _db.InventoryTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        return transaction;
        }

    public async Task<InventoryTransaction> StockOutAsync(int itemId, int quantity, string user, string? description = null)
        {
        var item = await _db.InventoryItems.FindAsync(itemId);
        if (item == null) throw new Exception("Item not found");
        if (item.CurrentStock < quantity) throw new Exception("Insufficient stock");

        item.CurrentStock -= quantity;
        item.UpdatedAt = DateTime.UtcNow;

        var transaction = new InventoryTransaction
            {
            InventoryItemId = itemId,
            TransactionType = "OUT",
            Quantity = quantity,
            Description = description,
            CreatedBy = user
            };

        _db.InventoryTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        return transaction;
        }
    public async Task DeleteItemAsync(int itemId)
        {
        var item = await _db.InventoryItems.FindAsync(itemId);
        if (item == null)
            throw new Exception("Item not found");

        // Optional: also delete related transactions if cascade not set
        // var transactions = _db.InventoryTransactions.Where(t => t.InventoryItemId == itemId);
        // _db.InventoryTransactions.RemoveRange(transactions);

        _db.InventoryItems.Remove(item);
        await _db.SaveChangesAsync();
        }

    }
