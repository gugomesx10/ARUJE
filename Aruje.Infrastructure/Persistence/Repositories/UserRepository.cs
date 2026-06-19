using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ArujeDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet
            .FirstOrDefaultAsync(user =>
                user.IsActive &&
                user.Email == email);
    }
}