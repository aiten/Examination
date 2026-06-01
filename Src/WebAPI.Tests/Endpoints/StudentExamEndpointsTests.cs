using System.Net;
using System.Net.Http.Json;

using Core.Contracts;
using Core.Entities;
using Core.QueryResult;

using FluentAssertions;

using NSubstitute;

using WebAPI.Endpoints;

namespace WebAPI.Tests.Endpoints;

public class StudentExamEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient             _client;
    private readonly IUnitOfWork            _uow;
    private readonly IStudentExamRepository _studentExamRepo;

    public StudentExamEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _uow    = factory.UnitOfWork;
        _uow.ClearReceivedCalls();
        _studentExamRepo = Substitute.For<IStudentExamRepository>();
        _uow.StudentExams.Returns(_studentExamRepo);
    }

    // ── GET all ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStudentExams_ReturnsOkWithOverviewList()
    {
        var overviews = new List<StudentExamOverview>
        {
            new(1, 1, "Alice", "Smith", "alice", "ABC12", 1, 30, 2, 1),
            new(2, 2, "Bob", "Jones", "bob", "DEF34", 1, 25, 2, 2)
        };
        _studentExamRepo.GetStudentExamOverviewsAsync(1).Returns(overviews);

        var response = await _client.GetAsync("/api/exam/1/students");
        var result   = await response.Content.ReadFromJsonAsync<List<StudentExamOverview>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(2);
        result![0].FirstName.Should().Be("Alice");
        result![0].Points.Should().Be(30);
        result![1].Points.Should().Be(25);
    }

    [Fact]
    public async Task GetStudentExams_EmptyExam_ReturnsEmptyList()
    {
        _studentExamRepo.GetStudentExamOverviewsAsync(99).Returns(new List<StudentExamOverview>());

        var response = await _client.GetAsync("/api/exam/99/students");
        var result   = await response.Content.ReadFromJsonAsync<List<StudentExamOverview>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeEmpty();
    }

    // ── GET by ID ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStudentExam_ExistingId_ReturnsOkWithSubtasks()
    {
        var student  = new Student { Id = 1, FirstName   = "Alice", LastName = "Smith" };
        var subtask1 = new Subtask { Id = 1, Description = "Part 1", Points  = 20 };
        var subtask2 = new Subtask { Id = 2, Description = "Part 2", Points  = 30 };
        var entity = new StudentExam
        {
            Id               = 1,
            ExamId           = 1,
            StudentId        = 1,
            Student          = student,
            LoginName        = "alice",
            RegistrationCode = "ABC12",
            StudentSubtasks = new List<StudentSubtask>
            {
                new() { Id = 1, SubtaskId = 1, Subtask = subtask1, Result = 0.15m },
                new() { Id = 2, SubtaskId = 2, Subtask = subtask2, Result = 0.25m }
            }
        };
        _studentExamRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var response = await _client.GetAsync("/api/exam/1/students/1");
        var result   = await response.Content.ReadFromJsonAsync<StudentExamDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Smith");
        result.LoginName.Should().Be("alice");
        result.Subtasks.Should().HaveCount(2);
        result.Subtasks[0].Result.Should().Be(15);
        result.Subtasks[1].Result.Should().Be(25);
    }

    [Fact]
    public async Task GetStudentExam_NonExistingId_Returns404()
    {
        _studentExamRepo.GetByIdAsync(99).ReturnsForAnyArgs((StudentExam?)null);

        var response = await _client.GetAsync("/api/exam/1/students/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStudentExam_WrongExamId_Returns404()
    {
        var entity = new StudentExam
        {
            Id               = 1, ExamId = 2, StudentId = 1,
            Student          = new Student { Id = 1, FirstName = "Alice", LastName = "Smith" },
            LoginName        = "alice",
            RegistrationCode = "ABC12"
        };
        _studentExamRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var response = await _client.GetAsync("/api/exam/1/students/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PutStudentExamResults_ExistingId_UpdatesResultsAndReturnsNoContent()
    {
        var subtask1 = new Subtask { Id        = 1, Description = "Part 1", Points = 20 };
        var subtask2 = new Subtask { Id        = 2, Description = "Part 2", Points = 30 };
        var ss1      = new StudentSubtask { Id = 1, SubtaskId   = 1, Subtask       = subtask1, Result = 0 };
        var ss2      = new StudentSubtask { Id = 2, SubtaskId   = 2, Subtask       = subtask2, Result = 0 };
        var entity = new StudentExam
        {
            Id               = 1,
            ExamId           = 1,
            StudentId        = 1,
            Student          = new Student { Id = 1, FirstName = "Alice", LastName = "Smith" },
            LoginName        = "alice",
            RegistrationCode = "ABC12",
            StudentSubtasks  = new List<StudentSubtask> { ss1, ss2 }
        };
        _studentExamRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var updates = new List<StudentSubtaskUpdateDto>
        {
            new(1, 75, null, null),
            new(2, 50, null, null)
        };
        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1", updates);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        ss1.Result.Should().Be(0.75m);
        ss2.Result.Should().Be(0.50m);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task PutStudentExamResults_NonExistingId_Returns404()
    {
        _studentExamRepo.GetByIdAsync(99).ReturnsForAnyArgs((StudentExam?)null);

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/99",
            new List<StudentSubtaskUpdateDto>());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutStudentExamResults_WrongExamId_Returns404()
    {
        var entity = new StudentExam
        {
            Id               = 1, ExamId = 2, StudentId = 1,
            Student          = new Student { Id = 1, FirstName = "Alice", LastName = "Smith" },
            LoginName        = "alice",
            RegistrationCode = "ABC12",
            StudentSubtasks  = new List<StudentSubtask>()
        };
        _studentExamRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1",
            new List<StudentSubtaskUpdateDto>());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutStudentExamResults_UnknownSubtaskId_Returns400()
    {
        var entity = new StudentExam
        {
            Id               = 1,
            ExamId           = 1,
            StudentId        = 1,
            Student          = new Student { Id = 1, FirstName = "Alice", LastName = "Smith" },
            LoginName        = "alice",
            RegistrationCode = "ABC12",
            StudentSubtasks = new List<StudentSubtask>
            {
                new() { Id = 1, SubtaskId = 1, Subtask = new Subtask { Id = 1, Description = "Part 1", Points = 20 }, Result = 0 }
            }
        };
        _studentExamRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1",
            new List<StudentSubtaskUpdateDto> { new(99, 50, null, null) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await _uow.DidNotReceive().SaveChangesAsync();
    }

    // ── DELETE ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteStudentExam_Existing_ReturnsNoContent()
    {
        var entity = new StudentExam
        {
            Id               = 1, ExamId = 1, StudentId = 1,
            LoginName        = "alice",
            RegistrationCode = "ABC12"
        };
        _studentExamRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var response = await _client.DeleteAsync("/api/exam/1/students/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _studentExamRepo.Received(1).Remove(entity);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteStudentExam_NotFound_ReturnsBadRequest()
    {
        _studentExamRepo.GetByIdAsync(99).ReturnsForAnyArgs((StudentExam?)null);

        var response = await _client.DeleteAsync("/api/exam/1/students/99");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteStudentExam_WrongExamId_ReturnsBadRequest()
    {
        var entity = new StudentExam
        {
            Id               = 1, ExamId = 2, StudentId = 1,
            LoginName        = "alice",
            RegistrationCode = "ABC12"
        };
        _studentExamRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var response = await _client.DeleteAsync("/api/exam/1/students/1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _studentExamRepo.DidNotReceive().Remove(Arg.Any<StudentExam>());
    }
}