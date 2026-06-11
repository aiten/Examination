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

public class TeacherEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient         _client;
    private readonly IUnitOfWork        _uow;
    private readonly ITeacherRepository _teacherRepo;

    public TeacherEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _uow    = factory.UnitOfWork;
        _uow.ClearReceivedCalls();
        _teacherRepo = Substitute.For<ITeacherRepository>();
        _uow.Teachers.Returns(_teacherRepo);
    }

    [Fact]
    public async Task GetTeachers_ReturnsOkWithList()
    {
        var teachers = new List<Teacher>
        {
            new() { Id = 1, LastName = "Mustermann", FirstName = "Max" },
            new() { Id = 2, LastName = "Schmidt", FirstName    = "Anna" }
        };
        _teacherRepo.GetNoTrackingAsync().ReturnsForAnyArgs(teachers);

        var response = await _client.GetAsync("/api/teacher");
        var result   = await response.Content.ReadFromJsonAsync<List<TeacherDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(2);
        result![0].LastName.Should().Be("Mustermann");
        result![1].LastName.Should().Be("Schmidt");
    }

    [Fact]
    public async Task GetTeacher_ExistingId_ReturnsOk()
    {
        var teacher = new Teacher { Id = 1, LastName = "Mustermann", FirstName = "Max" };
        _teacherRepo.GetByIdAsync(1).ReturnsForAnyArgs(teacher);

        var response = await _client.GetAsync("/api/teacher/1");
        var result   = await response.Content.ReadFromJsonAsync<TeacherDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.LastName.Should().Be("Mustermann");
    }

    [Fact]
    public async Task GetTeacher_NonExistingId_Returns404()
    {
        _teacherRepo.GetByIdAsync(99).ReturnsForAnyArgs((Teacher?)null);

        var response = await _client.GetAsync("/api/teacher/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostTeacher_ValidDto_ReturnsCreated()
    {
        var dto     = new TeacherDto(0, "Max", "Mustermann", null, "MUS");
        var created = new Teacher { Id = 1, LastName = "Mustermann", FirstName = "Max", Abbreviation = "MUS" };

        _teacherRepo.AddAsync(Arg.Any<Teacher>())
            .Returns(Task.FromResult<EntityEntry<Teacher>>(null!));
        _teacherRepo.GetByIdAsync(Arg.Any<int>()).ReturnsForAnyArgs(created);

        var response = await _client.PostAsJsonAsync("/api/teacher", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostTeacher_NonZeroId_ReturnsBadRequest()
    {
        var dto = new TeacherDto(5, "Max", "Mustermann", null, null);

        var response = await _client.PostAsJsonAsync("/api/teacher", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostTeacher_InvalidLastName_ReturnsValidationProblem()
    {
        var dto = new TeacherDto(0, "Max", "X", null, null); // LastName too short

        var response = await _client.PostAsJsonAsync("/api/teacher", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutTeacher_ValidUpdate_ReturnsNoContent()
    {
        var existing = new Teacher { Id = 1, LastName = "Alt", FirstName = "Max" };
        var dto      = new TeacherDto(1, "Max", "Mustermann", "Muster", "MUS");
        var trans    = Substitute.For<ITransaction>();
        _teacherRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/teacher/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutTeacher_IdMismatch_ReturnsBadRequest()
    {
        var dto = new TeacherDto(2, "Max", "Mustermann", null, null);

        var response = await _client.PutAsJsonAsync("/api/teacher/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutTeacher_NotFound_ReturnsBadRequest()
    {
        var dto = new TeacherDto(99, "Max", "Mustermann", null, null);
        _teacherRepo.GetByIdAsync(99).ReturnsForAnyArgs((Teacher?)null);

        var response = await _client.PutAsJsonAsync("/api/teacher/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteTeacher_Existing_ReturnsNoContent()
    {
        var existing = new Teacher { Id = 1, LastName = "Mustermann" };
        _teacherRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);

        var response = await _client.DeleteAsync("/api/teacher/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _teacherRepo.Received(1).Remove(existing);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteTeacher_NotFound_ReturnsBadRequest()
    {
        _teacherRepo.GetByIdAsync(99).ReturnsForAnyArgs((Teacher?)null);

        var response = await _client.DeleteAsync("/api/teacher/99");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}