using SyncMed.Data.Repositories;
using SyncMed.Models;

namespace SyncMed.Services;

public interface ISpecialtyService
{
    Task<IList<string>> GetAllSpecialtiesAsync();
    Task<IList<Doctor>> GetDoctorsBySpecialtyAsync(string specialty);
}

public class SpecialtyService : ISpecialtyService
{
    private readonly ISpecialtyRepository _repository;

    public SpecialtyService(ISpecialtyRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<string>> GetAllSpecialtiesAsync()
    {
        return await _repository.GetAllSpecialtiesAsync();
    }

    public async Task<IList<Doctor>> GetDoctorsBySpecialtyAsync(string specialty)
    {
        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("Specialty cannot be empty.");

        return await _repository.GetDoctorsBySpecialtyAsync(specialty);
    }
}
