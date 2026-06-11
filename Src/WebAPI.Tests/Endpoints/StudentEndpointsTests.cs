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

public class StudentEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient         _client;
    private readonly IUnitOfWork        _uow;
    private readonly IStudentRepository _studentRepo;
    private readonly IClassRepository   _classRepo;

    public StudentEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _uow    = factory.UnitOfWork;
        _uow.ClearReceivedCalls();
        _studentRepo = Substitute.For<IStudentRepository>();
        _classRepo   = Substitute.For<IClassRepository>();
        _uow.Students.Returns(_studentRepo);
        _uow.Classes.Returns(_classRepo);
    }

    [Fact]
    public async Task GetStudents_ReturnsOkWithList()
    {
        var students = new List<Student>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Smith", Classes = new List<Class>() },
            new() { Id = 2, FirstName = "Bob", LastName   = "Jones", Classes = new List<Class>() }
        };
        _studentRepo.GetNoTrackingAsync().ReturnsForAnyArgs(students);

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
        _studentRepo.GetByIdAsync(1).ReturnsForAnyArgs(student);

        var response = await _client.GetAsync("/api/student/1");
        var result   = await response.Content.ReadFromJsonAsync<StudentDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetStudent_NonExistingId_Returns404()
    {
        _studentRepo.GetByIdAsync(99).ReturnsForAnyArgs((Student?)null);

        var response = await _client.GetAsync("/api/student/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostStudent_ValidDto_ReturnsCreated()
    {
        var cls     = new Class { Id = 1, Description = "4AHIF", Year = 2024 };
        var dto     = new StudentDto(0, "Alice", "Smith", new[] { 1 });
        var created = new Student { Id = 1, FirstName = "Alice", LastName = "Smith", Classes = new List<Class> { cls } };

        _classRepo.GetAsync(default).ReturnsForAnyArgs(new List<Class> { cls });
        _studentRepo.AddAsync(Arg.Any<Student>()).Returns(Task.FromResult<EntityEntry<Student>>(null!));
        _studentRepo.GetByIdAsync(Arg.Any<int>()).ReturnsForAnyArgs(created);

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
        var cls      = new Class { Id   = 1, Description = "4AHIF", Year        = 2024 };
        var existing = new Student { Id = 1, FirstName   = "OldFirst", LastName = "OldLast", Classes = new List<Class>() };
        var dto      = new StudentDto(1, "NewFirst", "NewLast", new[] { 1 });
        var trans    = Substitute.For<ITransaction>();

        _studentRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);
        _classRepo.GetAsync(default).ReturnsForAnyArgs(new List<Class> { cls });
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/student/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        existing.FirstName.Should().Be("NewFirst");
        existing.LastName.Should().Be("NewLast");
        existing.Classes.Should().ContainSingle(c => c.Id == 1);
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutStudent_ClearsOldClasses_AndAssignsNew()
    {
        var oldClass = new Class { Id   = 2, Description = "3AHIF", Year     = 2023 };
        var newClass = new Class { Id   = 3, Description = "4AHIF", Year     = 2024 };
        var existing = new Student { Id = 1, FirstName   = "Alice", LastName = "Smith", Classes = new List<Class> { oldClass } };
        var dto      = new StudentDto(1, "Alice", "Smith", new[] { 3 });

        _studentRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);
        _classRepo.GetAsync(default).ReturnsForAnyArgs(new List<Class> { newClass });

        var response = await _client.PutAsJsonAsync("/api/student/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        existing.Classes.Should().ContainSingle(c => c.Id == 3);
        existing.Classes.Should().NotContain(c => c.Id == 2);
    }

    [Fact]
    public async Task PutStudent_IdMismatch_ReturnsBadRequest()
    {
        var dto = new StudentDto(2, "Alice", "Smith", Array.Empty<int>());

        var response = await _client.PutAsJsonAsync("/api/student/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutStudent_NotFound_ReturnsBadRequest()
    {
        var dto = new StudentDto(99, "Alice", "Smith", Array.Empty<int>());
        _studentRepo.GetByIdAsync(99).ReturnsForAnyArgs((Student?)null);

        var response = await _client.PutAsJsonAsync("/api/student/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteStudent_Existing_ReturnsNoContent()
    {
        var existing = new Student { Id = 1, FirstName = "Alice", LastName = "Smith", Classes = new List<Class>() };
        _studentRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);

        var response = await _client.DeleteAsync("/api/student/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _studentRepo.Received(1).Remove(existing);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteStudent_NotFound_ReturnsBadRequest()
    {
        _studentRepo.GetByIdAsync(99).ReturnsForAnyArgs((Student?)null);

        var response = await _client.DeleteAsync("/api/student/99");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}