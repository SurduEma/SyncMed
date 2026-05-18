using Microsoft.EntityFrameworkCore;
using SyncMed.Models;

namespace SyncMed.Data.Repositories;

public interface IMedicalServiceRepository : IGenericRepository<MedicalService>
{
}

public class MedicalServiceRepository : IMedicalServiceRepository
{
    private readonly SyncMedDbContext _context;

    public MedicalServiceRepository(SyncMedDbContext context)
    {
        _context = context;
    }

    public async Task<IList<MedicalService>> GetAllAsync()
    {
        return await _context.MedicalServices.ToListAsync();
    }

    public async Task<MedicalService?> GetByIdAsync(int id)
    {
        return await _context.MedicalServices.FindAsync(id);
    }

    public async Task AddAsync(MedicalService entity)
    {
        _context.MedicalServices.Add(entity);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(MedicalService entity)
    {
        _context.MedicalServices.Update(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.MedicalServices.Remove(entity);
            await SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
