using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Repositories;

public class FarmRepository : Repository<Farm>, IFarmRepository
{
    public FarmRepository(ArujeDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Farm>> SearchByNameAsync(string name)
    {
        return await DbSet
            .Where(farm =>
                farm.IsActive &&
                EF.Functions.ILike(farm.Name, $"%{name}%"))
            .ToListAsync();
    }
}