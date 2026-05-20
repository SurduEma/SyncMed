using Microsoft.AspNetCore.Identity;
using Moq;
using SyncMed.Data.Repositories;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Tests;

public class UserAccountServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPatientRepository> _patientRepo = new();
    private readonly Mock<IPasswordHasher<User>> _passwordHasher = new();
    private readonly UserAccountService _sut;

    public UserAccountServiceTests()
    {
        _sut = new UserAccountService(_userRepo.Object, _patientRepo.Object, _passwordHasher.Object);
    }

    [Fact]
    public async Task Register_RejectsFutureDOB()
    {
        var futureDob = DateOnly.FromDateTime(DateTime.Today).AddDays(1);
        var (success, message, _) = await _sut.RegisterPatientAsync("John", "Doe", "j@e.com", "password1", futureDob, null);
        Assert.False(success);
        Assert.Contains("future", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_RejectsDuplicateEmail()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("existing@e.com"))
            .ReturnsAsync(new User { Email = "existing@e.com" });

        var (success, message, _) = await _sut.RegisterPatientAsync(
            "John", "Doe", "existing@e.com", "password1", new DateOnly(1990, 1, 1), null);

        Assert.False(success);
        Assert.Contains("already exists", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_SucceedsAndHashesPassword()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.HashPassword(It.IsAny<User>(), "mypass123"))
            .Returns("hashed_value");

        var (success, _, user) = await _sut.RegisterPatientAsync(
            "Jane", "Doe", "jane@e.com", "mypass123", new DateOnly(1990, 5, 15), "555-0100");

        Assert.True(success);
        Assert.NotNull(user);
        Assert.Equal("hashed_value", user!.PasswordHash);
        _patientRepo.Verify(r => r.AddAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Fact]
    public async Task ValidateCredentials_InvalidPassword_ReturnsFalse()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("user@e.com"))
            .ReturnsAsync(new User { Email = "user@e.com", PasswordHash = "hash" });
        _passwordHasher.Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), "hash", "wrong"))
            .Returns(PasswordVerificationResult.Failed);

        var (success, _, _) = await _sut.ValidateCredentialsAsync("user@e.com", "wrong");

        Assert.False(success);
    }
}
