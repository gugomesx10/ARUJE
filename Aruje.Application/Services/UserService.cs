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

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users.Select(ToResponse).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null || !user.IsActive)
            throw new NotFoundException("User not found.");

        return ToResponse(user);
    }

    public async Task<UserResponse> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email filter is required.");

        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null || !user.IsActive)
            throw new NotFoundException("User not found.");

        return ToResponse(user);
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

        return ToResponse(user);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null || !user.IsActive)
            throw new NotFoundException("User not found.");

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser is not null && existingUser.Id != id)
            throw new ConflictException("Email is already registered.");

        user.UpdateProfile(
            request.FullName,
            request.Email
        );

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeRoleAsync(
        Guid id,
        ChangeUserRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null || !user.IsActive)
            throw new NotFoundException("User not found.");

        user.ChangeRole(request.Role);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(
        Guid id,
        ChangeUserPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null || !user.IsActive)
            throw new NotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new ValidationException("New password is required.");

        var passwordHash = _passwordHasher.Hash(request.NewPassword);

        user.ChangePassword(passwordHash);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null || !user.IsActive)
            throw new NotFoundException("User not found.");

        await _userRepository.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.IsActive
        );
    }
}