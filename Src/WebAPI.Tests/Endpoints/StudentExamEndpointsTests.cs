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
using Persistence.QueryResult;

using Service;

using Shared.Exceptions;

public class StudentExamEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient           _client;
    private readonly IUnitOfWork          _uow;
    private readonly IStudentExamService  _studentExamService;

    public StudentExamEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client             = factory.CreateClient();
        _uow                = factory.UnitOfWork;
        _studentExamService = factory.StudentExamService;
        _uow.ClearReceivedCalls();
        _studentExamService.ClearSubstitute();
    }

    // ── GET all ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStudentExams_ReturnsOkWithOverviewList()
    {
        var overviews = new List<StudentExamOverview>
        {
            new(1, 1, "Alice", "Smith", "alice", "ABC12", 1, 30, 2, 1),
            new(2, 2, "Bob",   "Jones", "bob",   "DEF34", 1, 25, 2, 2)
        };
        _studentExamService.GetStudentExamOverviewsAsync(default).ReturnsForAnyArgs(overviews);

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
        _studentExamService.GetStudentExamOverviewsAsync(default)
            .ReturnsForAnyArgs(new List<StudentExamOverview>());

        var response = await _client.GetAsync("/api/exam/99/students");
        var result   = await response.Content.ReadFromJsonAsync<List<StudentExamOverview>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeEmpty();
    }

    // ── GET by ID ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStudentExam_ExistingId_ReturnsOkWithSubtasks()
    {
        var student  = new Student { Id = 1, FirstName = "Alice", LastName = "Smith" };
        var subtask1 = new Subtask { Id = 1, Description = "Part 1", Points = 20 };
        var subtask2 = new Subtask { Id = 2, Description = "Part 2", Points = 30 };
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
        _studentExamService.SingleStudentExamAsync(default, null!).ReturnsForAnyArgs(entity);

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
        _studentExamService.SingleStudentExamAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<StudentExam>(new NotFoundException("StudentExam 99 not found")));

        var response = await _client.GetAsync("/api/exam/1/students/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStudentExam_WrongExamId_Returns400()
    {
        var entity = new StudentExam { Id = 1, ExamId = 2, StudentId = 1, LoginName = "alice", RegistrationCode = "ABC12" };
        _studentExamService.SingleStudentExamAsync(default, null!).ReturnsForAnyArgs(entity);

        var response = await _client.GetAsync("/api/exam/1/students/1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PUT ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PutStudentExam_ValidUpdate_ReturnsNoContent()
    {
        var dto   = new StudentExamDto(1, 1, 1, "Alice", "Smith", "alice", "ABC12", null, new List<StudentSubtaskResultDto>());
        var trans = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _studentExamService.Received(1).UpdateStudentExamAsync(1, Arg.Any<StudentExam>());
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutStudentExam_IdMismatch_ReturnsBadRequest()
    {
        var dto = new StudentExamDto(2, 1, 1, "Alice", "Smith", "alice", "ABC12", null, new List<StudentSubtaskResultDto>());

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutStudentExam_NotFound_ReturnsNotFound()
    {
        var dto = new StudentExamDto(99, 1, 1, "Alice", "Smith", "alice", "ABC12", null, new List<StudentSubtaskResultDto>());
        _studentExamService.When(s => s.UpdateStudentExamAsync(99, Arg.Any<StudentExam>()))
            .Throw(new NotFoundException("StudentExam 99 not found"));

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutStudentExam_WrongExamId_ReturnsConflict()
    {
        var dto = new StudentExamDto(1, 1, 1, "Alice", "Smith", "alice", "ABC12", null, new List<StudentSubtaskResultDto>());
        _studentExamService.When(s => s.UpdateStudentExamAsync(1, Arg.Any<StudentExam>()))
            .Throw(new ConflictException("ExamId mismatch"));

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── DELETE ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteStudentExam_Existing_ReturnsNoContent()
    {
        var entity = new StudentExam { Id = 1, ExamId = 1, StudentId = 1, LoginName = "alice", RegistrationCode = "ABC12" };
        var trans  = Substitute.For<ITransaction>();
        _studentExamService.SingleStudentExamAsync(default, null!).ReturnsForAnyArgs(entity);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.DeleteAsync("/api/exam/1/students/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _studentExamService.Received(1).DeleteStudentExamAsync(1);
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task DeleteStudentExam_NotFound_ReturnsNotFound()
    {
        _studentExamService.SingleStudentExamAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<StudentExam>(new NotFoundException("StudentExam 99 not found")));

        var response = await _client.DeleteAsync("/api/exam/1/students/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteStudentExam_WrongExamId_ReturnsBadRequest()
    {
        var entity = new StudentExam { Id = 1, ExamId = 2, StudentId = 1, LoginName = "alice", RegistrationCode = "ABC12" };
        _studentExamService.SingleStudentExamAsync(default, null!).ReturnsForAnyArgs(entity);

        var response = await _client.DeleteAsync("/api/exam/1/students/1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await _studentExamService.DidNotReceive().DeleteStudentExamAsync(Arg.Any<int>());
    }
}
