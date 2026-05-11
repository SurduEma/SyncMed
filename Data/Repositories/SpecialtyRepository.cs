using Microsoft.EntityFrameworkCore;
using SyncMed.Models;

namespace SyncMed.Data.Repositories;

public interface ISpecialtyRepository
{
    Task<IList<string>> GetAllSpecialtiesAsync();
    Task<IList<Doctor>> GetDoctorsBySpecialtyAsync(string specialty);
}

public class SpecialtyRepository : ISpecialtyRepository
{
    private readonly SyncMedDbContext _context;

    public SpecialtyRepository(SyncMedDbContext context)
    {
        _context = context;
    }

    public async Task<IList<string>> GetAllSpecialtiesAsync()
    {
        return await _context.Doctors
            .Where(d => !string.IsNullOrEmpty(d.Specialty))
            .Select(d => d.Specialty)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<IList<Doctor>> GetDoctorsBySpecialtyAsync(string specialty)
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Where(d => d.Specialty == specialty)
            .ToListAsync();
    }
}
