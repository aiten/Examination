using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.EntityFrameworkCore.ChangeTracking;

using NSubstitute;

using WebAPI.Endpoints;

namespace WebAPI.Tests.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;
using Persistence.Repositories;

public class ClassEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient       _client;
    private readonly IUnitOfWork      _uow;
    private readonly IClassRepository _classRepo;

    public ClassEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _uow    = factory.UnitOfWork;
        _uow.ClearReceivedCalls();
        _classRepo = Substitute.For<IClassRepository>();
        _uow.Classes.Returns(_classRepo);
    }

    [Fact]
    public async Task GetClasses_ReturnsOkWithList()
    {
        var classes = new List<Class>
        {
            new() { Id = 1, Description = "4AHITM", Year = 2024 },
            new() { Id = 2, Description = "3BHITM", Year = 2023 }
        };
        _classRepo.GetNoTrackingAsync().ReturnsForAnyArgs(classes);

        var response = await _client.GetAsync("/api/class");
        var result   = await response.Content.ReadFromJsonAsync<List<ClassDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(2);
        result![0].Description.Should().Be("4AHITM");
    }

    [Fact]
    public async Task GetClass_ExistingId_ReturnsOk()
    {
        var cls = new Class { Id = 1, Description = "4AHITM", Year = 2024 };
        _classRepo.GetByIdAsync(1).ReturnsForAnyArgs(cls);

        var response = await _client.GetAsync("/api/class/1");
        var result   = await response.Content.ReadFromJsonAsync<ClassDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.Description.Should().Be("4AHITM");
    }

    [Fact]
    public async Task GetClass_NonExistingId_Returns404()
    {
        _classRepo.GetByIdAsync(99).ReturnsForAnyArgs((Class?)null);

        var response = await _client.GetAsync("/api/class/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostClass_ValidDto_ReturnsCreated()
    {
        var dto     = new ClassDto(0, "4AHITM", 2024, null);
        var created = new Class { Id = 1, Description = "4AHITM", Year = 2024 };

        _classRepo.AddAsync(Arg.Any<Class>())
            .Returns(Task.FromResult<EntityEntry<Class>>(null!));
        _classRepo.GetByIdAsync(Arg.Any<int>()).ReturnsForAnyArgs(created);

        var response = await _client.PostAsJsonAsync("/api/class", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostClass_NonZeroId_ReturnsBadRequest()
    {
        var dto = new ClassDto(5, "4AHITM", 2024, null);

        var response = await _client.PostAsJsonAsync("/api/class", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutClass_ValidUpdate_ReturnsNoContent()
    {
        var existing = new Class { Id = 1, Description = "Old", Year = 2023 };
        var dto      = new ClassDto(1, "4AHITM", 2024, null);
        var trans    = Substitute.For<ITransaction>();
        _classRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/class/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutClass_IdMismatch_ReturnsBadRequest()
    {
        var dto = new ClassDto(2, "4AHITM", 2024, null);

        var response = await _client.PutAsJsonAsync("/api/class/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutClass_NotFound_ReturnsBadRequest()
    {
        var dto = new ClassDto(99, "4AHITM", 2024, null);
        _classRepo.GetByIdAsync(99).ReturnsForAnyArgs((Class?)null);

        var response = await _client.PutAsJsonAsync("/api/class/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteClass_Existing_ReturnsNoContent()
    {
        var existing = new Class { Id = 1, Description = "4AHITM", Year = 2024 };
        _classRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);

        var response = await _client.DeleteAsync("/api/class/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _classRepo.Received(1).Remove(existing);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteClass_NotFound_ReturnsBadRequest()
    {
        _classRepo.GetByIdAsync(99).ReturnsForAnyArgs((Class?)null);

        var response = await _client.DeleteAsync("/api/class/99");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}