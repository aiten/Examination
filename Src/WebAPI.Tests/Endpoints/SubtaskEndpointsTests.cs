using System.Net;
using System.Net.Http.Json;

using Base.Core.Contracts;

using Core.Contracts;
using Core.Entities;

using FluentAssertions;

using Microsoft.EntityFrameworkCore.ChangeTracking;

using NSubstitute;

using WebAPI.Endpoints;

namespace WebAPI.Tests.Endpoints;

public class SubtaskEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient          _client;
    private readonly IUnitOfWork         _uow;
    private readonly ISubtaskRepository  _subtaskRepo;

    public SubtaskEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client      = factory.CreateClient();
        _uow         = factory.UnitOfWork;
        _uow.ClearReceivedCalls();
        _subtaskRepo = Substitute.For<ISubtaskRepository>();
        _uow.Subtasks.Returns(_subtaskRepo);
    }

    [Fact]
    public async Task GetSubtasks_ReturnsOkWithList()
    {
        var subtasks = new List<Subtask>
        {
            new() { Id = 1, ExamId = 1, Description = "Task A", Points = 10 },
            new() { Id = 2, ExamId = 1, Description = "Task B", Points = 20 }
        };
        _subtaskRepo.GetNoTrackingAsync().ReturnsForAnyArgs(subtasks);

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
        _subtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(subtask);

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
        _subtaskRepo.GetByIdAsync(99).ReturnsForAnyArgs((Subtask?)null);

        var response = await _client.GetAsync("/api/exam/1/subtask/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSubtask_WrongExamId_Returns404()
    {
        var subtask = new Subtask { Id = 1, ExamId = 2, Description = "Task A", Points = 10 };
        _subtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(subtask);

        var response = await _client.GetAsync("/api/exam/1/subtask/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostSubtask_ValidDto_ReturnsCreated()
    {
        var dto     = new SubtaskDto(0, 1, "Task A", 10, false);
        var created = new Subtask { Id = 1, ExamId = 1, Description = "Task A", Points = 10 };

        _subtaskRepo.AddAsync(Arg.Any<Subtask>()).Returns(Task.FromResult<EntityEntry<Subtask>>(null!));
        _subtaskRepo.GetByIdAsync(Arg.Any<int>()).ReturnsForAnyArgs(created);

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
        var existing = new Subtask { Id = 1, ExamId = 1, Description = "Old", Points = 5 };
        var dto      = new SubtaskDto(1, 3, "Updated", 20, false);
        var trans    = Substitute.For<ITransaction>();

        _subtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/exam/1/subtask/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        existing.Description.Should().Be("Updated");
        existing.Points.Should().Be(20);
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
    public async Task PutSubtask_NotFound_ReturnsBadRequest()
    {
        var dto = new SubtaskDto(99, 12,"Task A", 10, false);
        _subtaskRepo.GetByIdAsync(99).ReturnsForAnyArgs((Subtask?)null);

        var response = await _client.PutAsJsonAsync("/api/exam/1/subtask/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutSubtask_WrongExam_ReturnsBadRequest()
    {
        var existing = new Subtask { Id = 1, ExamId = 2, Description = "Task A", Points = 10, Bonus = false };
        var dto      = new SubtaskDto(1, 9, "Updated", 20, false);

        _subtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);

        var response = await _client.PutAsJsonAsync("/api/exam/1/subtask/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSubtask_Existing_ReturnsNoContent()
    {
        var existing = new Subtask { Id = 1, ExamId = 1, Description = "Task A", Points = 10, Bonus = false };
        _subtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);

        var response = await _client.DeleteAsync("/api/exam/1/subtask/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _subtaskRepo.Received(1).Remove(existing);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteSubtask_NotFound_ReturnsBadRequest()
    {
        _subtaskRepo.GetByIdAsync(99).ReturnsForAnyArgs((Subtask?)null);

        var response = await _client.DeleteAsync("/api/exam/1/subtask/99");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSubtask_WrongExam_ReturnsBadRequest()
    {
        var existing = new Subtask { Id = 1, ExamId = 2, Description = "Task A", Points = 10, Bonus = false };
        _subtaskRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);

        var response = await _client.DeleteAsync("/api/exam/1/subtask/1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _subtaskRepo.DidNotReceive().Remove(Arg.Any<Subtask>());
    }
}
