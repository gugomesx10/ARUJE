using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Repositories;

public class OutboxMessageRepository : Repository<OutboxMessage>, IOutboxMessageRepository
{
    public OutboxMessageRepository(ArujeDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int quantity)
    {
        return await Context.Set<OutboxMessage>()
            .Where(message => message.IsActive && message.ProcessedAt == null)
            .OrderBy(message => message.OccurredAt)
            .Take(quantity)
            .ToListAsync();
    }
}