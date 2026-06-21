using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Repositories;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message);

    Task UpdateAsync(OutboxMessage message);

    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int quantity);
}