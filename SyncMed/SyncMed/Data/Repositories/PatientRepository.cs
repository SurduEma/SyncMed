using Microsoft.EntityFrameworkCore;
using SyncMed.Models;

namespace SyncMed.Data.Repositories;

public interface IPatientRepository : IGenericRepository<Patient>
{
    Task<IList<Patient>> GetAllWithUserAsync();
}

public class PatientRepository : IPatientRepository
{
    private readonly SyncMedDbContext _context;

    public PatientRepository(SyncMedDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Patient>> GetAllAsync() //get all patients from db
    {
        return await _context.Patients.ToListAsync();
    }

    public async Task<IList<Patient>> GetAllWithUserAsync()
    {
        return await _context.Patients
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(int id) //get patient by id from db
    {
        return await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PatientId == id);
    }

    public async Task AddAsync(Patient entity) 
    {
        _context.Patients.Add(entity);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(Patient entity)
    {
        _context.Patients.Update(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var patient = await GetByIdAsync(id);
        if (patient != null)
        {
            _context.Patients.Remove(patient);
            await SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
