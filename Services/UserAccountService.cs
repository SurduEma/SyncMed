using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SyncMed.Authorization;
using SyncMed.Data.Repositories;
using SyncMed.Models;

namespace SyncMed.Services;

public interface IUserAccountService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<(bool Success, string Message, User? User)> RegisterPatientAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        DateOnly dateOfBirth,
        string? phoneNumber);
    Task<(bool Success, string Message, User? User)> ValidateCredentialsAsync(string email, string password);
    ClaimsPrincipal CreatePrincipal(User user);
    string HashPassword(User user, string password);
}

public class UserAccountService : IUserAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserAccountService(
        IUserRepository userRepository,
        IPatientRepository patientRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdWithProfilesAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<(bool Success, string Message, User? User)> RegisterPatientAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        DateOnly dateOfBirth,
        string? phoneNumber)
    {
        if (dateOfBirth == default)
            return (false, "Date of birth is required.", null);

        if (dateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            return (false, "Date of birth cannot be in the future.", null);

        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            return (false, "An account with this email already exists.", null);

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim(),
            Role = AppRoles.Patient,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = HashPassword(user, password);

        var patient = new Patient
        {
            User = user,
            DateOfBirth = dateOfBirth,
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim()
        };

        await _patientRepository.AddAsync(patient);
        return (true, "Account created successfully.", user);
    }

    public async Task<(bool Success, string Message, User? User)> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return (false, "Invalid email or password.", null);

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return (false, "Invalid email or password.", null);

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = HashPassword(user, password);
            await _userRepository.UpdateAsync(user);
        }

        return (true, "Signed in successfully.", user);
    }

    public ClaimsPrincipal CreatePrincipal(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }
}
