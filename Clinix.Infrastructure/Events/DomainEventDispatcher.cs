using Clinix.Domain.Abstractions;
using Clinix.Domain.Entities;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Clinix.Infrastructure.Events;

public sealed class DomainEventDispatcher
    {
    private readonly ClinixDbContext _db;

    public DomainEventDispatcher(ClinixDbContext db) => _db = db;

    public static bool IsSeeding { get; set; } = false;

    public void DispatchEvents()
        {
        if (IsSeeding) return;

        var entities = _db.ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        if (!entities.Any()) return;

        var events = entities.SelectMany(e => e.DomainEvents).ToList();

        foreach (var evt in events)
            {
            // ✅ Serialize with actual ID (now assigned by database)
            var payload = JsonSerializer.Serialize(evt, evt.GetType());

            var outboxMsg = new OutboxMessage
                {
                Type = evt.GetType().Name,
                PayloadJson = payload,
                OccurredAtUtc = DateTime.UtcNow,
                Processed = false,
                Channel = "Notification"
                };

            _db.OutboxMessages.Add(outboxMsg);

            // ✅ Log the event for debugging
            Console.WriteLine($"📦 Dispatched event: {evt.GetType().Name}");
            Console.WriteLine($"   Payload: {payload}");
            }

        entities.ForEach(e => e.ClearDomainEvents());
        }
    }
