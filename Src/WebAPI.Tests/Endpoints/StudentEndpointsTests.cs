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

public class StudentEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient      _client;
    private readonly IUnitOfWork     _uow;
    private readonly IStudentService _studentService;

    public StudentEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client         = factory.CreateClient();
        _uow            = factory.UnitOfWork;
        _studentService = factory.StudentService;
        _uow.ClearReceivedCalls();
        _studentService.ClearSubstitute();
    }

    [Fact]
    public async Task GetStudents_ReturnsOkWithList()
    {
        var students = new List<Student>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Smith", Classes = new List<Class>() },
            new() { Id = 2, FirstName = "Bob",   LastName = "Jones", Classes = new List<Class>() }
        };
        _studentService.GetStudentsAsync(null!).ReturnsForAnyArgs(students);

        var response = await _client.GetAsync("/api/student");
        var result   = await response.Content.ReadFromJsonAsync<List<StudentDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(2);
        result![0].FirstName.Should().Be("Alice");
        result![1].FirstName.Should().Be("Bob");
    }

    [Fact]
    public async Task GetStudent_ExistingId_ReturnsOk()
    {
        var student = new Student { Id = 1, FirstName = "Alice", LastName = "Smith", Classes = new List<Class>() };
        _studentService.SingleStudentAsync(default, null!).ReturnsForAnyArgs(student);

        var response = await _client.GetAsync("/api/student/1");
        var result   = await response.Content.ReadFromJsonAsync<StudentDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetStudent_NonExistingId_Returns404()
    {
        _studentService.SingleStudentAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<Student>(new NotFoundException("Student 99 not found")));

        var response = await _client.GetAsync("/api/student/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostStudent_ValidDto_ReturnsCreated()
    {
        var dto     = new StudentDto(0, "Alice", "Smith", new[] { 1 });
        var created = new Student { Id = 1, FirstName = "Alice", LastName = "Smith", Classes = new List<Class>() };
        _studentService.AddStudentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ICollection<int>>())
            .Returns(created);

        var response = await _client.PostAsJsonAsync("/api/student", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostStudent_NonZeroId_ReturnsBadRequest()
    {
        var dto = new StudentDto(5, "Alice", "Smith", Array.Empty<int>());

        var response = await _client.PostAsJsonAsync("/api/student", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostStudent_EmptyName_ReturnsBadRequest()
    {
        var dto = new StudentDto(0, "", "Smith", Array.Empty<int>());

        var response = await _client.PostAsJsonAsync("/api/student", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutStudent_ValidUpdate_ReturnsNoContent()
    {
        var student = new Student { Id = 1, FirstName = "OldFirst", LastName = "OldLast", Classes = new List<Class>() };
        var dto     = new StudentDto(1, "NewFirst", "NewLast", new[] { 1 });
        var trans   = Substitute.For<ITransaction>();
        _studentService.SingleStudentAsync(default, null!).ReturnsForAnyArgs(student);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/student/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _studentService.Received(1).UpdateStudentAsync(1, "NewFirst", "NewLast", Arg.Any<ICollection<int>>());
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutStudent_PassesCorrectClassIdsToService()
    {
        var student = new Student { Id = 1, FirstName = "Alice", LastName = "Smith", Classes = new List<Class>() };
        var dto     = new StudentDto(1, "Alice", "Smith", new[] { 3 });
        _studentService.SingleStudentAsync(default, null!).ReturnsForAnyArgs(student);

        var response = await _client.PutAsJsonAsync("/api/student/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _studentService.Received(1).UpdateStudentAsync(
            1, "Alice", "Smith",
            Arg.Is<ICollection<int>>(ids => ids.Contains(3)));
    }

    [Fact]
    public async Task PutStudent_IdMismatch_ReturnsBadRequest()
    {
        var dto = new StudentDto(2, "Alice", "Smith", Array.Empty<int>());

        var response = await _client.PutAsJsonAsync("/api/student/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutStudent_NotFound_ReturnsNotFound()
    {
        var dto = new StudentDto(99, "Alice", "Smith", Array.Empty<int>());
        _studentService.UpdateStudentAsync(99, "Alice", "Smith", Array.Empty<int>())
            .ReturnsForAnyArgs(Task.FromException<Student>(new NotFoundException("Student 99 not found")));

        var response = await _client.PutAsJsonAsync("/api/student/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteStudent_Existing_ReturnsNoContent()
    {
        var trans = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.DeleteAsync("/api/student/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _studentService.Received(1).DeleteStudentAsync(1);
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task DeleteStudent_NotFound_ReturnsNotFound()
    {
        _studentService.When(s => s.DeleteStudentAsync(99))
            .Throw(new NotFoundException("Student 99 not found"));

        var response = await _client.DeleteAsync("/api/student/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
