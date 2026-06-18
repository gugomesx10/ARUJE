using Aruje.Application.DTOs.Users;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Domain.Entities;

namespace Aruje.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser is not null)
            throw new ConflictException("Email is already registered.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User(
            request.FullName,
            request.Email,
            passwordHash,
            request.Role
        );

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.IsActive
        );
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users.Select(user => new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.IsActive
        )).ToList();
    }
}