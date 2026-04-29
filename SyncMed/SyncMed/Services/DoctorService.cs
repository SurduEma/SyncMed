using SyncMed.Data.Repositories;
using SyncMed.Models;

namespace SyncMed.Services;

public interface IDoctorService
{
    Task<IList<Doctor>> GetAllDoctorsAsync();
    Task<Doctor?> GetDoctorByIdAsync(int id);
    Task AddDoctorAsync(Doctor doctor);
    Task UpdateDoctorAsync(Doctor doctor);
    Task DeleteDoctorAsync(int id);
}

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _repository;

    public DoctorService(IDoctorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<Doctor>> GetAllDoctorsAsync()
    {
        return await _repository.GetAllWithUserAsync();
    }

    public async Task<Doctor?> GetDoctorByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddDoctorAsync(Doctor doctor)
    {
        if (string.IsNullOrWhiteSpace(doctor.Specialty))
            throw new ArgumentException("Doctor specialty is required.");

        if (string.IsNullOrWhiteSpace(doctor.DoctorLicenseId))
            throw new ArgumentException("Doctor license ID is required.");

        await _repository.AddAsync(doctor);
    }

    public async Task UpdateDoctorAsync(Doctor doctor)
    {
        var existingDoctor = await _repository.GetByIdAsync(doctor.DoctorId);
        if (existingDoctor == null)
            throw new InvalidOperationException("Doctor not found.");

        await _repository.UpdateAsync(doctor);
    }

    public async Task DeleteDoctorAsync(int id)
    {
        var doctor = await _repository.GetByIdAsync(id);
        if (doctor == null)
            throw new InvalidOperationException("Doctor not found.");

        await _repository.DeleteAsync(id);
    }
}
