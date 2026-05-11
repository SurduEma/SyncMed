using SyncMed.Data.Repositories;
using SyncMed.Models;

namespace SyncMed.Services;

public interface IPatientService
{
    Task<IList<Patient>> GetAllPatientsAsync();
    Task<Patient?> GetPatientByIdAsync(int id);
    Task AddPatientAsync(Patient patient);
    Task UpdatePatientAsync(Patient patient);
    Task DeletePatientAsync(int id);
}

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;

    public PatientService(IPatientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<Patient>> GetAllPatientsAsync()
    {
        return await _repository.GetAllWithUserAsync();
    }

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddPatientAsync(Patient patient)
    {
        if (patient.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Invalid date of birth.");

        await _repository.AddAsync(patient);
    }

    public async Task UpdatePatientAsync(Patient patient)
    {
        var existingPatient = await _repository.GetByIdAsync(patient.PatientId);
        if (existingPatient == null)
            throw new InvalidOperationException("Patient not found.");

        await _repository.UpdateAsync(patient);
    }

    public async Task DeletePatientAsync(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient == null)
            throw new InvalidOperationException("Patient not found.");

        await _repository.DeleteAsync(id);
    }
}
