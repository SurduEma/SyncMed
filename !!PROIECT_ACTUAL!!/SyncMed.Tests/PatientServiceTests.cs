using Moq;
using SyncMed.Data.Repositories;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Tests;

public class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _repo = new();
    private readonly PatientService _sut;

    public PatientServiceTests()
    {
        _sut = new PatientService(_repo.Object);
    }

    [Fact]
    public async Task AddPatient_ThrowsWhenDOBDefault()
    {
        var patient = new Patient { DateOfBirth = default };
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPatientAsync(patient));
    }

    [Fact]
    public async Task AddPatient_ThrowsWhenDOBFuture()
    {
        var patient = new Patient { DateOfBirth = DateOnly.FromDateTime(DateTime.Today).AddDays(1) };
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPatientAsync(patient));
    }
}
