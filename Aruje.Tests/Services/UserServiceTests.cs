using Aruje.Application.DTOs.Users;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Application.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;
using FluentAssertions;
using Moq;
using AppValidationException = Aruje.Application.Exceptions.ValidationException;

namespace Aruje.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUser_WhenEmailIsAvailable()
    {
        var request = new CreateUserRequest(
            "Gustavo Gomes",
            "gustavo@aruje.com",
            "Senha123@",
            UserRole.Admin
        );

        User? capturedUser = null;

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(hasher => hasher.Hash(request.Password))
            .Returns("HASHED_PASSWORD");

        _userRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _userService.CreateAsync(request);

        result.Should().NotBeNull();
        result.FullName.Should().Be(request.FullName);
        result.Email.Should().Be(request.Email);
        result.Role.Should().Be(request.Role);
        result.IsActive.Should().BeTrue();

        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().Be("HASHED_PASSWORD");

        _passwordHasherMock.Verify(
            hasher => hasher.Hash(request.Password),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<User>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        var request = new CreateUserRequest(
            "Gustavo Gomes",
            "gustavo@aruje.com",
            "Senha123@",
            UserRole.Admin
        );

        var existingUser = new User(
            "Outro Usuário",
            "gustavo@aruje.com",
            "HASHED_PASSWORD",
            UserRole.Operator
        );

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        var action = async () => await _userService.CreateAsync(request);

        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email is already registered.");

        _passwordHasherMock.Verify(
            hasher => hasher.Hash(It.IsAny<string>()),
            Times.Never
        );

        _userRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<User>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers_WhenUsersExist()
    {
        var users = new List<User>
        {
            new("Gustavo Gomes", "gustavo@aruje.com", "HASHED_PASSWORD", UserRole.Admin),
            new("Maria Silva", "maria@aruje.com", "HASHED_PASSWORD", UserRole.Manager)
        };

        _userRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(users);

        var result = await _userService.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].FullName.Should().Be("Gustavo Gomes");
        result[1].FullName.Should().Be("Maria Silva");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();

        var user = new User(
            "Gustavo Gomes",
            "gustavo@aruje.com",
            "HASHED_PASSWORD",
            UserRole.Admin
        );

        typeof(User)
            .BaseType!
            .GetProperty("Id")!
            .SetValue(user, userId);

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var result = await _userService.GetByIdAsync(userId);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FullName.Should().Be(user.FullName);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var action = async () => await _userService.GetByIdAsync(userId);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        var email = "gustavo@aruje.com";

        var user = new User(
            "Gustavo Gomes",
            email,
            "HASHED_PASSWORD",
            UserRole.Admin
        );

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var result = await _userService.GetByEmailAsync(email);

        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.FullName.Should().Be("Gustavo Gomes");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldThrowValidationException_WhenEmailIsEmpty()
    {
        var action = async () => await _userService.GetByEmailAsync("");

        await action.Should().ThrowAsync<AppValidationException>()
            .WithMessage("Email filter is required.");

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var email = "naoexiste@aruje.com";

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        var action = async () => await _userService.GetByEmailAsync(email);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenUserExistsAndEmailIsAvailable()
    {
        var userId = Guid.NewGuid();

        var user = new User(
            "Gustavo Antigo",
            "antigo@aruje.com",
            "HASHED_PASSWORD",
            UserRole.Operator
        );

        var request = new UpdateUserRequest(
            "Gustavo Gomes",
            "gustavo@aruje.com"
        );

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _userService.UpdateAsync(userId, request);

        user.FullName.Should().Be(request.FullName);
        user.Email.Should().Be(request.Email);

        _userRepositoryMock.Verify(
            repository => repository.UpdateAsync(user),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowConflictException_WhenEmailBelongsToAnotherUser()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var user = new User(
            "Gustavo Gomes",
            "gustavo@aruje.com",
            "HASHED_PASSWORD",
            UserRole.Admin
        );

        typeof(User)
            .BaseType!
            .GetProperty("Id")!
            .SetValue(user, userId);

        var anotherUser = new User(
            "Outro Usuário",
            "outro@aruje.com",
            "HASHED_PASSWORD",
            UserRole.Operator
        );

        typeof(User)
            .BaseType!
            .GetProperty("Id")!
            .SetValue(anotherUser, otherUserId);

        var request = new UpdateUserRequest(
            "Gustavo Gomes",
            "outro@aruje.com"
        );

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(request.Email))
            .ReturnsAsync(anotherUser);

        var action = async () => await _userService.UpdateAsync(userId, request);

        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email is already registered.");

        _userRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<User>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ChangeRoleAsync_ShouldChangeUserRole_WhenUserExists()
    {
        var userId = Guid.NewGuid();

        var user = new User(
            "Gustavo Gomes",
            "gustavo@aruje.com",
            "HASHED_PASSWORD",
            UserRole.Operator
        );

        var request = new ChangeUserRoleRequest(UserRole.Admin);

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _userService.ChangeRoleAsync(userId, request);

        user.Role.Should().Be(UserRole.Admin);

        _userRepositoryMock.Verify(
            repository => repository.UpdateAsync(user),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldChangeUserPassword_WhenUserExists()
    {
        var userId = Guid.NewGuid();

        var user = new User(
            "Gustavo Gomes",
            "gustavo@aruje.com",
            "OLD_HASH",
            UserRole.Admin
        );

        var request = new ChangeUserPasswordRequest("NovaSenha123@");

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher => hasher.Hash(request.NewPassword))
            .Returns("NEW_HASH");

        _userRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _userService.ChangePasswordAsync(userId, request);

        user.PasswordHash.Should().Be("NEW_HASH");

        _passwordHasherMock.Verify(
            hasher => hasher.Hash(request.NewPassword),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repository => repository.UpdateAsync(user),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowValidationException_WhenPasswordIsEmpty()
    {
        var userId = Guid.NewGuid();

        var user = new User(
            "Gustavo Gomes",
            "gustavo@aruje.com",
            "OLD_HASH",
            UserRole.Admin
        );

        var request = new ChangeUserPasswordRequest("");

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var action = async () => await _userService.ChangePasswordAsync(userId, request);

        await action.Should().ThrowAsync<AppValidationException>()
            .WithMessage("New password is required.");

        _passwordHasherMock.Verify(
            hasher => hasher.Hash(It.IsAny<string>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();

        var user = new User(
            "Gustavo Gomes",
            "gustavo@aruje.com",
            "HASHED_PASSWORD",
            UserRole.Admin
        );

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _userService.DeleteAsync(userId);

        _userRepositoryMock.Verify(
            repository => repository.DeleteAsync(user),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}