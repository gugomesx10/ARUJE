using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;

namespace Aruje.Infrastructure.Persistence.Repositories;

public class FarmRepository : Repository<Farm>, IFarmRepository
{
    public FarmRepository(ArujeDbContext context) : base(context)
    {
    }
}