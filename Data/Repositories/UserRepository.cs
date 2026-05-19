using Microsoft.EntityFrameworkCore;
using SyncMed.Models;

namespace SyncMed.Data.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithProfilesAsync(int id);
}

public class UserRepository : IUserRepository
{
    private readonly SyncMedDbContext _context;

    public UserRepository(SyncMedDbContext context)
    {
        _context = context;
    }

    public async Task<IList<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
            .Include(u => u.Nurse)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByIdWithProfilesAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
            .Include(u => u.Nurse)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        return await _context.Users
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
            .Include(u => u.Nurse)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task AddAsync(User entity)
    {
        _context.Users.Add(entity);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(User entity)
    {
        _context.Users.Update(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
