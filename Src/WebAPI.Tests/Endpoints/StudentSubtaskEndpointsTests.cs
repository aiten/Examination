using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using NSubstitute;

using WebAPI.Endpoints;

namespace WebAPI.Tests.Endpoints;

using Persistence;
using Persistence.Model;
using Persistence.Repositories;

public class StudentSubtaskEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient                _client;
    private readonly IUnitOfWork               _uow;
    private readonly IStudentSubtaskRepository _studentSubtaskRepo;
    private readonly IStudentExamRepository    _studentExamRepo;

    public StudentSubtaskEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _uow    = factory.UnitOfWork;
        _uow.ClearReceivedCalls();
        _studentSubtaskRepo = Substitute.For<IStudentSubtaskRepository>();
        _studentExamRepo    = Substitute.For<IStudentExamRepository>();
        _uow.StudentSubtasks.Returns(_studentSubtaskRepo);
        _uow.StudentExams.Returns(_studentExamRepo);
    }

    // ── GET all ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStudentSubtasks_ReturnsOkWithList()
    {
        var studentExam = new StudentExam { Id = 1, ExamId      = 1, StudentId     = 1, LoginName = "alice", RegistrationCode = "ABC12" };
        var subtask1    = new Subtask { Id     = 1, Description = "Part 1", Points = 20 };
        var subtask2    = new Subtask { Id     = 2, Description = "Part 2", Points = 30 };
        var list = new List<StudentSubtask>
        {
            new() { Id = 1, SubtaskId = 1, Subtask = subtask1, StudentExamId = 1, Result = 0.75m },
            new() { Id = 2, SubtaskId = 2, Subtask = subtask2, StudentExamId = 1, Result = 0.50m }
        };
        _studentExamRepo.GetByIdAsync(1).ReturnsForAnyArgs(studentExam);
        _studentSubtaskRepo.GetNoTrackingAsync().ReturnsForAnyArgs(list);

        var response = await _client.GetAsync("/api/exam/1/students/1/subtasks");
        var result   = await response.Content.ReadFromJsonAsync<List<StudentSubtaskDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(2);
        result![0].Description.Should().Be("Part 1");
        result![0].Result.Should().Be(75);
        result![1].Result.Should().Be(50);
    }

    [Fact]
    public async Task GetStudentSubtasks_StudentExamNotFound_Returns404()
    {
        _studentExamRepo.GetByIdAsync(99).ReturnsForAnyArgs((StudentExam?)null);

        var response = await _client.GetAsync("/api/exam/1/students/99/subtasks");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStudentSubtasks_WrongExamId_Returns404()
    {
        var studentExam = new StudentExam { Id = 1, ExamId = 2, StudentId = 1, LoginName = "alice", RegistrationCode = "ABC12" };
        _studentExamRepo.GetByIdAsync(1).ReturnsForAnyArgs(studentExam);

        var response = await _client.GetAsync("/api/exam/1/students/1/subtasks");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET by ID ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStudentSubtask_ExistingId_ReturnsOk()
    {
        var subtask = new Subtask { Id        = 1, Description = "Part 1", Points = 20 };
        var entity  = new StudentSubtask { Id = 1, SubtaskId   = 1, Subtask       = subtask, StudentExamId = 1, Result = 0.75m };
        _studentSubtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var response = await _client.GetAsync("/api/exam/1/students/1/subtasks/1");
        var result   = await response.Content.ReadFromJsonAsync<StudentSubtaskDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.Description.Should().Be("Part 1");
        result.Points.Should().Be(20);
        result.Result.Should().Be(75);
    }

    [Fact]
    public async Task GetStudentSubtask_NonExistingId_Returns404()
    {
        _studentSubtaskRepo.GetByIdAsync(99).ReturnsForAnyArgs((StudentSubtask?)null);

        var response = await _client.GetAsync("/api/exam/1/students/1/subtasks/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStudentSubtask_WrongStudentExamId_Returns404()
    {
        var subtask = new Subtask { Id        = 1, Description = "Part 1", Points = 20 };
        var entity  = new StudentSubtask { Id = 1, SubtaskId   = 1, Subtask       = subtask, StudentExamId = 2, Result = 0.75m };
        _studentSubtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var response = await _client.GetAsync("/api/exam/1/students/1/subtasks/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PutStudentSubtask_ValidUpdate_ReturnsNoContentAndUpdatesResult()
    {
        var entity = new StudentSubtask { Id = 1, SubtaskId = 1, StudentExamId = 1, Result = 0 };
        _studentSubtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var dto      = new StudentSubtaskDto(1, 1, "Part 1", 20, false, 75, 1m, null, null);
        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1/subtasks/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        entity.Result.Should().Be(0.75m);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task PutStudentSubtask_IdMismatch_ReturnsBadRequest()
    {
        var dto      = new StudentSubtaskDto(2, 1, "Part 1", 20, false, 75, 1m, null, null);
        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1/subtasks/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await _uow.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task PutStudentSubtask_NonExistingId_Returns404()
    {
        _studentSubtaskRepo.GetByIdAsync(99).ReturnsForAnyArgs((StudentSubtask?)null);

        var dto      = new StudentSubtaskDto(99, 1, "Part 1", 20, false, 75, 1m, null, null);
        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1/subtasks/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await _uow.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task PutStudentSubtask_WrongStudentExamId_Returns404()
    {
        var entity = new StudentSubtask { Id = 1, SubtaskId = 1, StudentExamId = 2, Result = 0 };
        _studentSubtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(entity);

        var dto      = new StudentSubtaskDto(1, 1, "Part 1", 20, false, 75, 1m, null, null);
        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1/subtasks/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await _uow.DidNotReceive().SaveChangesAsync();
    }
}