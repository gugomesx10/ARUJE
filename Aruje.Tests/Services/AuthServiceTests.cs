using Aruje.Application.DTOs.Auth;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Application.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Aruje.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        var request = new LoginRequest(
            "gustavo@aruje.com",
            "Senha123@"
        );

        var user = new User(
            "Gustavo Gomes",
            request.Email,
            "HASHED_PASSWORD",
            UserRole.Admin
        );

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher => hasher.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        _tokenServiceMock
            .Setup(service => service.GenerateToken(user))
            .Returns("JWT_TOKEN");

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.FullName.Should().Be(user.FullName);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(user.Role);
        result.Token.Should().Be("JWT_TOKEN");

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailAsync(request.Email),
            Times.Once
        );

        _passwordHasherMock.Verify(
            hasher => hasher.Verify(request.Password, user.PasswordHash),
            Times.Once
        );

        _tokenServiceMock.Verify(
            service => service.GenerateToken(user),
            Times.Once
        );
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenEmailIsEmpty()
    {
        var request = new LoginRequest(
            "",
            "Senha123@"
        );

        var action = async () => await _authService.LoginAsync(request);

        await action.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenPasswordIsEmpty()
    {
        var request = new LoginRequest(
            "gustavo@aruje.com",
            ""
        );

        var action = async () => await _authService.LoginAsync(request);

        await action.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenUserDoesNotExist()
    {
        var request = new LoginRequest(
            "gustavo@aruje.com",
            "Senha123@"
        );

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        var action = async () => await _authService.LoginAsync(request);

        await action.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        _passwordHasherMock.Verify(
            hasher => hasher.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );

        _tokenServiceMock.Verify(
            service => service.GenerateToken(It.IsAny<User>()),
            Times.Never
        );
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenPasswordIsInvalid()
    {
        var request = new LoginRequest(
            "gustavo@aruje.com",
            "SenhaErrada"
        );

        var user = new User(
            "Gustavo Gomes",
            request.Email,
            "HASHED_PASSWORD",
            UserRole.Admin
        );

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher => hasher.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        var action = async () => await _authService.LoginAsync(request);

        await action.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");

        _tokenServiceMock.Verify(
            service => service.GenerateToken(It.IsAny<User>()),
            Times.Never
        );
    }
}