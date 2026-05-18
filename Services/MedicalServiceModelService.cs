using SyncMed.Data.Repositories;
using SyncMed.Models;

namespace SyncMed.Services;

public interface IMedicalServiceModelService
{
    Task<IList<MedicalService>> GetAllServicesAsync();
    Task<MedicalService?> GetServiceByIdAsync(int id);
    Task AddServiceAsync(MedicalService service);
    Task UpdateServiceAsync(MedicalService service);
    Task DeleteServiceAsync(int id);
}

public class MedicalServiceModelService : IMedicalServiceModelService
{
    private readonly IMedicalServiceRepository _repository;

    public MedicalServiceModelService(IMedicalServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<MedicalService>> GetAllServicesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<MedicalService?> GetServiceByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddServiceAsync(MedicalService service)
    {
        await _repository.AddAsync(service);
    }

    public async Task UpdateServiceAsync(MedicalService service)
    {
        await _repository.UpdateAsync(service);
    }

    public async Task DeleteServiceAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}
