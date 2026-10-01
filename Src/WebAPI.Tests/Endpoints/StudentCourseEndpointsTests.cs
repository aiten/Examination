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

public class StudentCourseEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient            _client;
    private readonly IUnitOfWork           _uow;
    private readonly IStudentCourseService _studentCourseService;

    public StudentCourseEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client                = factory.CreateClient();
        _uow                   = factory.UnitOfWork;
        _studentCourseService  = factory.StudentCourseService;
        _uow.ClearReceivedCalls();
        _studentCourseService.ClearSubstitute();
    }

    private static StudentCourse MakeEntity(int id, int courseId, int studentId, string registrationCode = "12345") =>
        new()
        {
            Id               = id,
            CourseId         = courseId,
            StudentId        = studentId,
            RegistrationCode = registrationCode,
            Student          = new Student { Id = studentId, FirstName = "Alice", LastName = "Smith", Classes = new List<Class>() }
        };

    [Fact]
    public async Task GetCourseStudents_ReturnsOkWithList()
    {
        var list = new List<StudentCourse>
        {
            MakeEntity(1, 5, 10),
            MakeEntity(2, 5, 11)
        };
        _studentCourseService.GetByCourseAsync(default, null!).ReturnsForAnyArgs(list);

        var response = await _client.GetAsync("/api/course/5/students");
        var result   = await response.Content.ReadFromJsonAsync<List<StudentCourseDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(2);
        result![0].CourseId.Should().Be(5);
    }

    [Fact]
    public async Task GetCourseStudent_ExistingId_ReturnsOk()
    {
        var entity = MakeEntity(1, 5, 10);
        _studentCourseService.SingleStudentCourseAsync(default, null!).ReturnsForAnyArgs(entity);

        var response = await _client.GetAsync("/api/course/5/students/1");
        var result   = await response.Content.ReadFromJsonAsync<StudentCourseDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.StudentId.Should().Be(10);
    }

    [Fact]
    public async Task GetCourseStudent_BelongsToDifferentCourse_Returns404()
    {
        var entity = MakeEntity(1, 99, 10);
        _studentCourseService.SingleStudentCourseAsync(default, null!).ReturnsForAnyArgs(entity);

        var response = await _client.GetAsync("/api/course/5/students/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCourseStudent_NonExistingId_Returns404()
    {
        _studentCourseService.SingleStudentCourseAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<StudentCourse>(new NotFoundException("StudentCourse 99 not found")));

        var response = await _client.GetAsync("/api/course/5/students/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostCourseStudent_ValidDto_ReturnsCreated()
    {
        var dto     = new StudentCourseDto(0, 5, 10, "Alice", "Smith", "12345");
        var created = MakeEntity(1, 5, 10);
        var trans   = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);
        _studentCourseService.AddStudentCourseAsync(10, 5, "12345").Returns(created);
        _studentCourseService.GetStudentCourseByIdAsync(1, Arg.Any<string[]>()).Returns(created);

        var response = await _client.PostAsJsonAsync("/api/course/5/students", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await _studentCourseService.Received(1).AddStudentCourseAsync(10, 5, "12345");
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PostCourseStudent_NonZeroId_ReturnsBadRequest()
    {
        var dto = new StudentCourseDto(5, 5, 10, "Alice", "Smith", "12345");

        var response = await _client.PostAsJsonAsync("/api/course/5/students", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostCourseStudent_InvalidStudentId_ReturnsBadRequest()
    {
        var dto = new StudentCourseDto(0, 5, 0, "Alice", "Smith", "12345");

        var response = await _client.PostAsJsonAsync("/api/course/5/students", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutCourseStudent_ValidUpdate_ReturnsNoContent()
    {
        var existing = MakeEntity(1, 5, 10, "12345");
        var dto      = new StudentCourseDto(1, 5, 10, "Alice", "Smith", "54321");
        var trans    = Substitute.For<ITransaction>();
        _studentCourseService.SingleStudentCourseAsync(1).Returns(existing);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/course/5/students/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _studentCourseService.Received(1).UpdateStudentCourseAsync(1, "54321");
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutCourseStudent_IdMismatch_ReturnsBadRequest()
    {
        var dto = new StudentCourseDto(2, 5, 10, "Alice", "Smith", "54321");

        var response = await _client.PutAsJsonAsync("/api/course/5/students/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutCourseStudent_BelongsToDifferentCourse_Returns404()
    {
        var existing = MakeEntity(1, 99, 10);
        var dto      = new StudentCourseDto(1, 5, 10, "Alice", "Smith", "54321");
        _studentCourseService.SingleStudentCourseAsync(1).Returns(existing);

        var response = await _client.PutAsJsonAsync("/api/course/5/students/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCourseStudent_Existing_ReturnsNoContent()
    {
        var existing = MakeEntity(1, 5, 10);
        var trans    = Substitute.For<ITransaction>();
        _studentCourseService.SingleStudentCourseAsync(1).Returns(existing);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.DeleteAsync("/api/course/5/students/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _studentCourseService.Received(1).DeleteStudentCourseAsync(1);
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task DeleteCourseStudent_BelongsToDifferentCourse_Returns404()
    {
        var existing = MakeEntity(1, 99, 10);
        _studentCourseService.SingleStudentCourseAsync(1).Returns(existing);

        var response = await _client.DeleteAsync("/api/course/5/students/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCourseStudent_NotFound_Returns404()
    {
        _studentCourseService.SingleStudentCourseAsync(99)
            .Returns(Task.FromException<StudentCourse>(new NotFoundException("StudentCourse 99 not found")));

        var response = await _client.DeleteAsync("/api/course/5/students/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
