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

public class SubtaskEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient       _client;
    private readonly IUnitOfWork      _uow;
    private readonly ISubtaskService  _subtaskService;

    public SubtaskEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client         = factory.CreateClient();
        _uow            = factory.UnitOfWork;
        _subtaskService = factory.SubtaskService;
        _uow.ClearReceivedCalls();
        _subtaskService.ClearSubstitute();
    }

    [Fact]
    public async Task GetSubtasks_ReturnsOkWithList()
    {
        var subtasks = new List<Subtask>
        {
            new() { Id = 1, ExamId = 1, Description = "Task A", Points = 10 },
            new() { Id = 2, ExamId = 1, Description = "Task B", Points = 20 }
        };
        _subtaskService.GetSubtasksForExamAsync(default).ReturnsForAnyArgs(subtasks);

        var response = await _client.GetAsync("/api/exam/1/subtask");
        var result   = await response.Content.ReadFromJsonAsync<List<SubtaskDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(2);
        result![0].Description.Should().Be("Task A");
    }

    [Fact]
    public async Task GetSubtask_ExistingId_ReturnsOk()
    {
        var subtask = new Subtask { Id = 1, ExamId = 1, Description = "Task A", Points = 10 };
        _subtaskService.SingleSubtaskAsync(default, null!).ReturnsForAnyArgs(subtask);

        var response = await _client.GetAsync("/api/exam/1/subtask/1");
        var result   = await response.Content.ReadFromJsonAsync<SubtaskDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.Description.Should().Be("Task A");
        result.Points.Should().Be(10);
    }

    [Fact]
    public async Task GetSubtask_NonExistingId_Returns404()
    {
        _subtaskService.SingleSubtaskAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<Subtask>(new NotFoundException("Subtask 99 not found")));

        var response = await _client.GetAsync("/api/exam/1/subtask/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSubtask_WrongExamId_Returns400()
    {
        var subtask = new Subtask { Id = 1, ExamId = 2, Description = "Task A", Points = 10 };
        _subtaskService.SingleSubtaskAsync(default, null!).ReturnsForAnyArgs(subtask);

        var response = await _client.GetAsync("/api/exam/1/subtask/1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostSubtask_ValidDto_ReturnsCreated()
    {
        var dto     = new SubtaskDto(0, 1, "Task A", 10, false);
        var created = new Subtask { Id = 1, ExamId = 1, Description = "Task A", Points = 10 };
        _subtaskService.AddSubtaskAsync(Arg.Any<Subtask>()).Returns(created);
        _subtaskService.GetSubtaskByIdAsync(default, null!).ReturnsForAnyArgs(created);

        var response = await _client.PostAsJsonAsync("/api/exam/1/subtask", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostSubtask_NonZeroId_ReturnsBadRequest()
    {
        var dto = new SubtaskDto(5, 1, "Task A", 10, false);

        var response = await _client.PostAsJsonAsync("/api/exam/1/subtask", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostSubtask_EmptyDescription_ReturnsBadRequest()
    {
        var dto = new SubtaskDto(0, 3, "", 10, false);

        var response = await _client.PostAsJsonAsync("/api/exam/1/subtask", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostSubtask_ZeroPoints_ReturnsBadRequest()
    {
        var dto = new SubtaskDto(0, 5, "Task A", 0, false);

        var response = await _client.PostAsJsonAsync("/api/exam/1/subtask", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutSubtask_ValidUpdate_ReturnsNoContent()
    {
        var dto   = new SubtaskDto(1, 3, "Updated", 20, false);
        var trans = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/exam/1/subtask/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _subtaskService.Received(1).UpdateSubtaskAsync(1, Arg.Any<Subtask>());
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutSubtask_IdMismatch_ReturnsBadRequest()
    {
        var dto = new SubtaskDto(2, 9, "Task A", 10, false);

        var response = await _client.PutAsJsonAsync("/api/exam/1/subtask/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutSubtask_NotFound_ReturnsNotFound()
    {
        var dto = new SubtaskDto(99, 12, "Task A", 10, false);
        _subtaskService.When(s => s.UpdateSubtaskAsync(99, Arg.Any<Subtask>()))
            .Throw(new NotFoundException("Subtask 99 not found"));

        var response = await _client.PutAsJsonAsync("/api/exam/1/subtask/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutSubtask_WrongExam_ReturnsConflict()
    {
        var dto = new SubtaskDto(1, 9, "Updated", 20, false);
        _subtaskService.When(s => s.UpdateSubtaskAsync(1, Arg.Any<Subtask>()))
            .Throw(new ConflictException("ExamId mismatch"));

        var response = await _client.PutAsJsonAsync("/api/exam/1/subtask/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteSubtask_Existing_ReturnsNoContent()
    {
        var subtask = new Subtask { Id = 1, ExamId = 1, Description = "Task A", Points = 10 };
        var trans   = Substitute.For<ITransaction>();
        _subtaskService.SingleSubtaskAsync(default, null!).ReturnsForAnyArgs(subtask);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.DeleteAsync("/api/exam/1/subtask/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _subtaskService.Received(1).DeleteSubtaskAsync(1);
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task DeleteSubtask_NotFound_ReturnsNotFound()
    {
        _subtaskService.SingleSubtaskAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<Subtask>(new NotFoundException("Subtask 99 not found")));

        var response = await _client.DeleteAsync("/api/exam/1/subtask/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSubtask_WrongExam_ReturnsBadRequest()
    {
        var subtask = new Subtask { Id = 1, ExamId = 2, Description = "Task A", Points = 10 };
        _subtaskService.SingleSubtaskAsync(default, null!).ReturnsForAnyArgs(subtask);

        var response = await _client.DeleteAsync("/api/exam/1/subtask/1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await _subtaskService.DidNotReceive().DeleteSubtaskAsync(Arg.Any<int>());
    }
}
