using Moq;
using SyncMed.Data.Repositories;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Tests;

public class DoctorServiceTests
{
    private readonly Mock<IDoctorRepository> _repo = new();
    private readonly DoctorService _sut;

    public DoctorServiceTests()
    {
        _sut = new DoctorService(_repo.Object);
    }

    [Fact]
    public async Task AddDoctor_ThrowsWhenSpecialtyEmpty()
    {
        var doctor = new Doctor { Specialty = "", DoctorLicenseId = "LIC-001" };
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddDoctorAsync(doctor));
    }

    [Fact]
    public async Task AddDoctor_ThrowsWhenLicenseEmpty()
    {
        var doctor = new Doctor { Specialty = "Cardiology", DoctorLicenseId = "" };
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddDoctorAsync(doctor));
    }
}
