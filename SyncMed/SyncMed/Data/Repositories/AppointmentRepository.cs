using Microsoft.EntityFrameworkCore;
using SyncMed.Models;

namespace SyncMed.Data.Repositories;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<IList<Appointment>> GetAllWithIncludesAsync();
    Task<Appointment?> GetByIdWithIncludesAsync(int id);
    Task<IList<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateOnly date);
}

public class AppointmentRepository : IAppointmentRepository
{
    private readonly SyncMedDbContext _context;

    public AppointmentRepository(SyncMedDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Appointment>> GetAllAsync()
    {
        return await _context.Appointments.ToListAsync();
    }

    public async Task<IList<Appointment>> GetAllWithIncludesAsync()
    {
        return await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments.FindAsync(id);
    }

    public async Task<Appointment?> GetByIdWithIncludesAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Consultation)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
    }

    public async Task<IList<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateOnly date)
    {
        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate == date && a.Status != "Cancelled")
            .ToListAsync();
    }

    public async Task AddAsync(Appointment entity)
    {
        _context.Appointments.Add(entity);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(Appointment entity)
    {
        _context.Appointments.Update(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var appointment = await GetByIdAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
