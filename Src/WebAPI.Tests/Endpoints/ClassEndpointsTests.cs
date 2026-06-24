using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ClearExtensions;

using WebAPI.Endpoints;

namespace WebAPI.Tests.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

using Shared.Exceptions;

public class ClassEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient    _client;
    private readonly IUnitOfWork   _uow;
    private readonly IClassService _classService;

    public ClassEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client       = factory.CreateClient();
        _uow          = factory.UnitOfWork;
        _classService = factory.ClassService;
        _uow.ClearReceivedCalls();
        _classService.ClearSubstitute();
    }

    [Fact]
    public async Task GetClasses_ReturnsOkWithList()
    {
        var classes = new List<Class>
        {
            new() { Id = 1, Description = "4AHITM", Year = 2024 },
            new() { Id = 2, Description = "3BHITM", Year = 2023 }
        };
        _classService.GetClassesAsync(null!).ReturnsForAnyArgs(classes);

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
        _classService.SingleClassAsync(default, null!).ReturnsForAnyArgs(cls);

        var response = await _client.GetAsync("/api/class/1");
        var result   = await response.Content.ReadFromJsonAsync<ClassDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.Description.Should().Be("4AHITM");
    }

    [Fact]
    public async Task GetClass_NonExistingId_Returns404()
    {
        _classService.SingleClassAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<Class>(new NotFoundException("Class 99 not found")));

        var response = await _client.GetAsync("/api/class/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostClass_ValidDto_ReturnsCreated()
    {
        var dto     = new ClassDto(0, "4AHITM", 2024, null);
        var created = new Class { Id = 1, Description = "4AHITM", Year = 2024 };
        _classService.AddClassAsync(Arg.Any<Class>()).Returns(created);

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
        var dto   = new ClassDto(1, "4AHITM", 2024, null);
        var trans = Substitute.For<ITransaction>();
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
    public async Task PutClass_NotFound_ReturnsNotFound()
    {
        var dto   = new ClassDto(99, "4AHITM", 2024, null);
        var trans = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);
        _classService.SingleClassAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<Class>(new NotFoundException("Class 99 not found")));

        var response = await _client.PutAsJsonAsync("/api/class/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteClass_Existing_ReturnsNoContent()
    {
        var response = await _client.DeleteAsync("/api/class/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _classService.Received(1).DeleteClassAsync(1);
    }

    [Fact]
    public async Task DeleteClass_NotFound_ReturnsNotFound()
    {
        _classService.When(s => s.DeleteClassAsync(99))
            .Throw(new NotFoundException("Class 99 not found"));

        var response = await _client.DeleteAsync("/api/class/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
