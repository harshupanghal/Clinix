using Clinix.Domain.Abstractions;
using Clinix.Domain.Entities;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Clinix.Infrastructure.Events;

/// <summary>
/// Captures domain events from entities and serializes them to OutboxMessages.
/// Does NOT call SaveChanges - that's handled by EF Core after this runs.
/// </summary>
public sealed class DomainEventDispatcher
    {
    private readonly ClinixDbContext _db;

    public DomainEventDispatcher(ClinixDbContext db) => _db = db;

    /// <summary>
    /// Flag to disable event dispatch during seeding.
    /// Set to true in DataSeeder to prevent notifications for demo data.
    /// </summary>
    public static bool IsSeeding { get; set; } = false;

    /// <summary>
    /// Extracts domain events and adds them to OutboxMessages (in-memory).
    /// EF Core will save everything together in one transaction.
    /// </summary>
    public void DispatchEvents()
        {
        // Skip event dispatch during seeding to avoid demo notifications
        if (IsSeeding) return;

        var entities = _db.ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        if (!entities.Any()) return;

        var events = entities.SelectMany(e => e.DomainEvents).ToList();

        foreach (var evt in events)
            {
            var outboxMsg = new OutboxMessage
                {
                Type = evt.GetType().Name,
                PayloadJson = JsonSerializer.Serialize(evt, evt.GetType()),
                OccurredAtUtc = DateTime.UtcNow,
                Processed = false,
                Channel = "Notification"
                };

            // Add to context (in-memory) - SaveChanges called by EF Core later
            _db.OutboxMessages.Add(outboxMsg);
            }

        // Clear events after adding to outbox
        entities.ForEach(e => e.ClearDomainEvents());
        }
    }
