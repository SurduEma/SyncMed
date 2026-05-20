using Moq;
using SyncMed.Data.Repositories;
using SyncMed.Services;

namespace SyncMed.Tests;

public class SpecialtyServiceTests
{
    private readonly Mock<ISpecialtyRepository> _repo = new();
    private readonly SpecialtyService _sut;

    public SpecialtyServiceTests()
    {
        _sut = new SpecialtyService(_repo.Object);
    }

    [Fact]
    public async Task GetDoctorsBySpecialty_ThrowsWhenEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetDoctorsBySpecialtyAsync(""));
    }
}
