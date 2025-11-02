//using Clinix.Infrastructure.Events;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using Microsoft.Extensions.DependencyInjection;

//namespace Clinix.Infrastructure.Persistence;

///// <summary>
///// Intercepts SaveChanges to dispatch domain events to outbox BEFORE committing.
///// Uses lazy resolution to avoid circular dependency with DbContext.
///// </summary>
//public sealed class DomainEventSaveChangesInterceptor : SaveChangesInterceptor
//    {
//    private readonly IServiceProvider _serviceProvider;


//    public DomainEventSaveChangesInterceptor(IServiceProvider serviceProvider)
//        => _serviceProvider = serviceProvider;

//    public override InterceptionResult<int> SavingChanges(
//        DbContextEventData eventData,
//        InterceptionResult<int> result)
//        {
//        DispatchEventsIfNeeded(eventData);
//        return base.SavingChanges(eventData, result);
//        }

//    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
//        DbContextEventData eventData,
//        InterceptionResult<int> result,
//        CancellationToken ct = default)
//        {
//        DispatchEventsIfNeeded(eventData);
//        return await base.SavingChangesAsync(eventData, result, ct);
//        }

//    private void DispatchEventsIfNeeded(DbContextEventData eventData)
//        {
//        if (eventData.Context == null) return;

//        using var scope = _serviceProvider.CreateScope();
//        var dispatcher = scope.ServiceProvider.GetRequiredService<DomainEventDispatcher>();
//        dispatcher.DispatchEvents();
//        }
//    }
