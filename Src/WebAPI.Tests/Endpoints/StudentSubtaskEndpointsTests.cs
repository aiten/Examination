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

public class StudentSubtaskEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient               _client;
    private readonly IUnitOfWork             _uow;
    private readonly IStudentSubtaskService  _studentSubtaskService;

    public StudentSubtaskEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client                = factory.CreateClient();
        _uow                   = factory.UnitOfWork;
        _studentSubtaskService = factory.StudentSubtaskService;
        _uow.ClearReceivedCalls();
        _studentSubtaskService.ClearSubstitute();
    }

    // ── GET all ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStudentSubtasks_ReturnsOkWithList()
    {
        var subtask1 = new Subtask { Id = 1, Description = "Part 1", Points = 20 };
        var subtask2 = new Subtask { Id = 2, Description = "Part 2", Points = 30 };
        var list = new List<StudentSubtask>
        {
            new() { Id = 1, SubtaskId = 1, Subtask = subtask1, StudentExamId = 1, Result = 0.75m },
            new() { Id = 2, SubtaskId = 2, Subtask = subtask2, StudentExamId = 1, Result = 0.50m }
        };
        _studentSubtaskService.GetAllForStudentAsync(default, default).ReturnsForAnyArgs(list);

        var response = await _client.GetAsync("/api/exam/1/students/1/subtasks");
        var result   = await response.Content.ReadFromJsonAsync<List<StudentSubtaskDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(2);
        result![0].Description.Should().Be("Part 1");
        result![0].Result.Should().Be(75);
        result![1].Result.Should().Be(50);
    }

    [Fact]
    public async Task GetStudentSubtasks_StudentExamNotFound_Returns409()
    {
        _studentSubtaskService.GetAllForStudentAsync(default, default)
            .ReturnsForAnyArgs(Task.FromException<IList<StudentSubtask>>(
                new ConflictException("No StudentExam found with ID 99 for exam 1")));

        var response = await _client.GetAsync("/api/exam/1/students/99/subtasks");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetStudentSubtasks_WrongExamId_Returns409()
    {
        _studentSubtaskService.GetAllForStudentAsync(default, default)
            .ReturnsForAnyArgs(Task.FromException<IList<StudentSubtask>>(
                new ConflictException("No StudentExam found with ID 1 for exam 1")));

        var response = await _client.GetAsync("/api/exam/1/students/1/subtasks");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── GET by ID ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStudentSubtask_ExistingId_ReturnsOk()
    {
        var subtask = new Subtask { Id = 1, Description = "Part 1", Points = 20, SeqNo = 1 };
        var entity  = new StudentSubtask { Id = 1, SubtaskId = 1, Subtask = subtask, StudentExamId = 1, Result = 0.75m };
        _studentSubtaskService.SingleStudentSubtaskAsync(default, null!).ReturnsForAnyArgs(entity);

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
        _studentSubtaskService.When(s => s.CheckValidStudentExam(99, 1, 1))
            .Throw(new NotFoundException("No StudentSubtask found with ID 99"));

        var response = await _client.GetAsync("/api/exam/1/students/1/subtasks/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStudentSubtask_WrongStudentExamId_Returns409()
    {
        _studentSubtaskService.When(s => s.CheckValidStudentExam(1, 1, 2))
            .Throw(new ConflictException("StudentSubtask 1 does not belong to studentexam 2"));

        var response = await _client.GetAsync("/api/exam/1/students/2/subtasks/1");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── PUT ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PutStudentSubtask_ValidUpdate_ReturnsNoContentAndUpdatesResult()
    {
        var dto   = new StudentSubtaskDto(1, 1, "Part 1", 20, false, 1, 75m, null, null, null);
        var trans = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1/subtasks/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _studentSubtaskService.Received(1)
            .UpdateStudentSubtaskAsync(1, 1, Arg.Is<StudentSubtask>(e => e.Result == 0.75m));
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutStudentSubtask_IdMismatch_ReturnsBadRequest()
    {
        var dto = new StudentSubtaskDto(2, 1, "Part 1", 20, false, 1, 75m, null, null, null);

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1/subtasks/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await _studentSubtaskService.DidNotReceive().UpdateStudentSubtaskAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<StudentSubtask>());
    }

    [Fact]
    public async Task PutStudentSubtask_NonExistingId_Returns404()
    {
        var dto = new StudentSubtaskDto(99, 1, "Part 1", 20, false, 1, 75m, null, null, null);
        _studentSubtaskService.When(s => s.UpdateStudentSubtaskAsync(99, 1, Arg.Any<StudentSubtask>()))
            .Throw(new NotFoundException("StudentSubtask 99 not found"));

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1/subtasks/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutStudentSubtask_WrongStudentExamId_Returns409()
    {
        var dto = new StudentSubtaskDto(1, 1, "Part 1", 20, false, 1, 75m, null, null, null);
        _studentSubtaskService.When(s => s.UpdateStudentSubtaskAsync(1, 1, Arg.Any<StudentSubtask>()))
            .Throw(new ConflictException("StudentSubtask 1 does not belong to studentexam 1"));

        var response = await _client.PutAsJsonAsync("/api/exam/1/students/1/subtasks/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
