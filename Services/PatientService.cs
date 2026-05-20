using SyncMed.Data.Repositories;
using SyncMed.Models;

namespace SyncMed.Services;

public interface IPatientService
{
    Task<IList<Patient>> GetAllPatientsAsync();
    Task<Patient?> GetPatientByIdAsync(int id);
    Task<Patient?> GetPatientDetailsAsync(int id);
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

    public async Task<Patient?> GetPatientDetailsAsync(int id)
    {
        return await _repository.GetByIdWithDetailsAsync(id);
    }

    public async Task AddPatientAsync(Patient patient)
    {
        if (patient.DateOfBirth == default)
            throw new ArgumentException("Date of birth is required.");

        if (patient.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Invalid date of birth.");

        await _repository.AddAsync(patient);
    }

    public async Task UpdatePatientAsync(Patient patient)
    {
        if (patient.DateOfBirth == default)
            throw new ArgumentException("Date of birth is required.");

        if (patient.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Invalid date of birth.");

        var existingPatient = await _repository.GetByIdAsync(patient.PatientId);
        if (existingPatient == null)
            throw new InvalidOperationException("Patient not found.");

        existingPatient.DateOfBirth = patient.DateOfBirth;
        existingPatient.PhoneNumber = patient.PhoneNumber;

        if (existingPatient.User != null && patient.User != null)
        {
            existingPatient.User.FirstName = patient.User.FirstName;
            existingPatient.User.LastName = patient.User.LastName;
            existingPatient.User.Email = patient.User.Email;
        }

        await _repository.UpdateAsync(existingPatient);
    }

    public async Task DeletePatientAsync(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient == null)
            throw new InvalidOperationException("Patient not found.");

        await _repository.DeleteAsync(id);
    }
}
