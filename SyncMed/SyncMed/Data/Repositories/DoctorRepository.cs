using Microsoft.EntityFrameworkCore;
using SyncMed.Models;

namespace SyncMed.Data.Repositories;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<IList<Doctor>> GetAllWithUserAsync();
}

public class DoctorRepository : IDoctorRepository
{
    private readonly SyncMedDbContext _context;

    public DoctorRepository(SyncMedDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Doctor>> GetAllAsync()
    {
        return await _context.Doctors.ToListAsync();
    }

    public async Task<IList<Doctor>> GetAllWithUserAsync()
    {
        return await _context.Doctors
            .Include(d => d.User)
            .ToListAsync();
    }

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        return await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.DoctorId == id);
    }

    public async Task AddAsync(Doctor entity)
    {
        _context.Doctors.Add(entity);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(Doctor entity)
    {
        _context.Doctors.Update(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var doctor = await GetByIdAsync(id);
        if (doctor != null)
        {
            _context.Doctors.Remove(doctor);
            await SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
