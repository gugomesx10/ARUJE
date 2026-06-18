using Aruje.Domain.Common;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity> where TEntity : BaseEntity
{
    protected readonly ArujeDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected Repository(ArujeDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        return await DbSet
            .Where(x => x.IsActive)
            .ToListAsync();
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
    }

    public virtual Task UpdateAsync(TEntity entity)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity)
    {
        entity.Deactivate();
        DbSet.Update(entity);
        return Task.CompletedTask;
    }
}