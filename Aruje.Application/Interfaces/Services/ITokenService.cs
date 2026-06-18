using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}