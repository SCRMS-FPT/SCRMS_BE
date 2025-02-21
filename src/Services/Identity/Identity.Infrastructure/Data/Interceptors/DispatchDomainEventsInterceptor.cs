using Identity.Domain.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Identity.Infrastructure.Data.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator mediator;

    public DispatchDomainEventsInterceptor(IMediator mediator)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        // Ensure the context is not null
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        // Ensure the context is not null
        await DispatchDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public async Task DispatchDomainEvents(DbContext context)
    {
        if (context == null) return;

        var aggregates = context.ChangeTracker
            .Entries<IAggregate>()
            .Where(a => a.Entity.DomainEvents.Any())
            .Select(a => a.Entity);

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        aggregates.ToList().ForEach(a => a.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent);
    }
}