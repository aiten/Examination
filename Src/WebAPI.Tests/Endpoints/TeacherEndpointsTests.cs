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

public class TeacherEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient      _client;
    private readonly IUnitOfWork     _uow;
    private readonly ITeacherService _teacherService;

    public TeacherEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client         = factory.CreateClient();
        _uow            = factory.UnitOfWork;
        _teacherService = factory.TeacherService;
        _uow.ClearReceivedCalls();
        _teacherService.ClearSubstitute();
    }

    [Fact]
    public async Task GetTeachers_ReturnsOkWithList()
    {
        var teachers = new List<Teacher>
        {
            new() { Id = 1, LastName = "Mustermann", FirstName = "Max" },
            new() { Id = 2, LastName = "Schmidt", FirstName    = "Anna" }
        };
        _teacherService.GetTeachersAsync(null!).ReturnsForAnyArgs(teachers);

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
        _teacherService.SingleTeacherAsync(default, null!).ReturnsForAnyArgs(teacher);

        var response = await _client.GetAsync("/api/teacher/1");
        var result   = await response.Content.ReadFromJsonAsync<TeacherDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.LastName.Should().Be("Mustermann");
    }

    [Fact]
    public async Task GetTeacher_NonExistingId_Returns404()
    {
        _teacherService.SingleTeacherAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<Teacher>(new NotFoundException("Teacher 99 not found")));

        var response = await _client.GetAsync("/api/teacher/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostTeacher_ValidDto_ReturnsCreated()
    {
        var dto     = new TeacherDto(0, "Max", "Mustermann", null, "MUS");
        var created = new Teacher { Id = 1, LastName = "Mustermann", FirstName = "Max", Abbreviation = "MUS" };
        _teacherService.AddTeacherAsync(Arg.Any<Teacher>()).Returns(created);

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
        var dto = new TeacherDto(0, "Max", "X", null, null);

        var response = await _client.PostAsJsonAsync("/api/teacher", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutTeacher_ValidUpdate_ReturnsNoContent()
    {
        var dto   = new TeacherDto(1, "Max", "Mustermann", "Muster", "MUS");
        var trans = Substitute.For<ITransaction>();
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
    public async Task PutTeacher_NotFound_ReturnsNotFound()
    {
        var dto   = new TeacherDto(99, "Max", "Mustermann", null, null);
        var trans = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);
        _teacherService.When(s => s.UpdateTeacherAsync(99, Arg.Any<Teacher>()))
            .Throw(new NotFoundException("Teacher 99 not found"));

        var response = await _client.PutAsJsonAsync("/api/teacher/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTeacher_Existing_ReturnsNoContent()
    {
        var response = await _client.DeleteAsync("/api/teacher/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _teacherService.Received(1).DeleteTeacherAsync(1);
    }

    [Fact]
    public async Task DeleteTeacher_NotFound_ReturnsNotFound()
    {
        _teacherService.When(s => s.DeleteTeacherAsync(99))
            .Throw(new NotFoundException("Teacher 99 not found"));

        var response = await _client.DeleteAsync("/api/teacher/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
