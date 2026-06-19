using Aruje.Application.DTOs.Auth;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;

namespace Aruje.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new UnauthorizedException("Invalid email or password.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
            throw new UnauthorizedException("Invalid email or password.");

        var passwordIsValid = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!passwordIsValid)
            throw new UnauthorizedException("Invalid email or password.");

        var token = _tokenService.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            token
        );
    }
}