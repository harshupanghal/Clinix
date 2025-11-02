using Clinix.Infrastructure.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Clinix.Infrastructure.Persistence;

public sealed class DomainEventSaveChangesInterceptor : SaveChangesInterceptor
    {
    [ThreadStatic]
    private static bool _isProcessingEvents;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
        {
        DispatchEventsIfNeeded(eventData.Context);
        return base.SavingChanges(eventData, result);
        }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
        {
        DispatchEventsIfNeeded(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, ct);
        }

    private void DispatchEventsIfNeeded(DbContext? context)
        {
        if (_isProcessingEvents) return;
        if (context is not ClinixDbContext clinixContext) return;

        try
            {
            _isProcessingEvents = true;

            var dispatcher = new DomainEventDispatcher(clinixContext);
            dispatcher.DispatchEvents();
            }
        finally
            {
            _isProcessingEvents = false;
            }
        }
    }
