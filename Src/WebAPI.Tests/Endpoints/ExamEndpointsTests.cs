using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.EntityFrameworkCore.ChangeTracking;

using NSubstitute;
using NSubstitute.ClearExtensions;

using WebAPI.Endpoints;

namespace WebAPI.Tests.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;
using Persistence.QueryResult;
using Persistence.Repositories;

using Service;

using Shared.Exceptions;

public class ExamEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient             _client;
    private readonly IUnitOfWork            _uow;
    private readonly IExamRepository        _examRepo;
    private readonly IStudentRepository     _studentRepo;
    private readonly IStudentExamRepository _studentExamRepo;
    private readonly IExamService           _examService;

    public ExamEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client      = factory.CreateClient();
        _uow         = factory.UnitOfWork;
        _examService = factory.ExamService;
        _uow.ClearReceivedCalls();
        _examService.ClearSubstitute();
        _examRepo        = Substitute.For<IExamRepository>();
        _studentRepo     = Substitute.For<IStudentRepository>();
        _studentExamRepo = Substitute.For<IStudentExamRepository>();
        _uow.Exams.Returns(_examRepo);
        _uow.Students.Returns(_studentRepo);
        _uow.StudentExams.Returns(_studentExamRepo);
    }

    [Fact]
    public async Task GetExams_ReturnsOkWithList()
    {
        var teacher = new Teacher { Id = 1, LastName = "Mustermann" };
        var exams = new List<Exam>
        {
            new() { Id = 1, Description = "Kinder, lernt!", TeacherId          = 1, Teacher = teacher, Created = DateTime.Today, ExamType = ExamType.Standard },
            new() { Id = 2, Description = "Habt ihr nichts zu tun?", TeacherId = 1, Teacher = teacher, Created = DateTime.Today, ExamType = ExamType.Standard }
        };
        _examService.GetExamsAsync().Returns(exams);

        var response = await _client.GetAsync("/api/exam");
        var result   = await response.Content.ReadFromJsonAsync<List<ExamDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(2);
        result![0].Description.Should().Be("Kinder, lernt!");
    }

    [Fact]
    public async Task GetExamOverview_ReturnsOk()
    {
        var overviews = new List<ExamOverview>
        {
            new(1, "Kinder, lernt!", 12345, "Mustermann", "2AHIF", new DateOnly(2026, 5, 7), new TimeOnly(8, 0), new TimeOnly(10, 0), ["X", "Y"], ["S1", "S2"])
        };
        _examService.GetExamOverviewsAsync(null, null).ReturnsForAnyArgs(overviews);

        var response = await _client.GetAsync("/api/exam/overview");
        var result   = await response.Content.ReadFromJsonAsync<List<ExamOverview>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().HaveCount(1);
        result![0].Teacher.Should().Be("Mustermann");
    }

    [Fact]
    public async Task GetExam_ExistingId_ReturnsOk()
    {
        var teacher = new Teacher { Id = 1, LastName    = "Mustermann" };
        var exam    = new Exam { Id    = 1, Description = "Kinder, lernt!", TeacherId = 1, Teacher = teacher, Created = DateTime.Today, ExamType = ExamType.Standard };
        _examService.SingleExamAsync(default, null!).ReturnsForAnyArgs(exam);

        var response = await _client.GetAsync("/api/exam/1");
        var result   = await response.Content.ReadFromJsonAsync<ExamDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.Description.Should().Be("Kinder, lernt!");
    }

    [Fact]
    public async Task GetExam_NonExistingId_Returns404()
    {
        _examService.SingleExamAsync(default, null!)
            .ReturnsForAnyArgs(Task.FromException<Exam>(new NotFoundException("Exam 99 not found")));

        var response = await _client.GetAsync("/api/exam/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostExam_ValidDto_ReturnsCreated()
    {
        var dto     = new ExamDto(0, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null, false, false);
        var teacher = new Teacher { Id = 1, LastName    = "Mustermann" };
        var created = new Exam { Id    = 1, Description = "Kinder, lernt!", TeacherId = 1, Teacher = teacher, Created = DateTime.Today, ExamType = ExamType.Standard, From = new TimeOnly(8, 0), To = new TimeOnly(10, 0) };

        _examService.AddExamAsync(Arg.Any<Exam>()).Returns(created);

        var response = await _client.PostAsJsonAsync("/api/exam", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostExam_NonZeroId_ReturnsBadRequest()
    {
        var dto = new ExamDto(5, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null, false, false);

        var response = await _client.PostAsJsonAsync("/api/exam", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutExam_ValidUpdate_ReturnsNoContent()
    {
        var dto   = new ExamDto(1, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null, false, false);
        var trans = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/exam/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutExam_IdMismatch_ReturnsBadRequest()
    {
        var dto = new ExamDto(2, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null, false, false);

        var response = await _client.PutAsJsonAsync("/api/exam/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutExam_NotFound_ReturnsNotFound()
    {
        var dto   = new ExamDto(99, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null, false, false);
        var trans = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);
        _examService.When(s => s.UpdateExamAsync(99, Arg.Any<Exam>()))
            .Throw(new NotFoundException("Exam 99 not found"));

        var response = await _client.PutAsJsonAsync("/api/exam/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteExam_Existing_ReturnsNoContent()
    {
        var trans = Substitute.For<ITransaction>();
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.DeleteAsync("/api/exam/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _examService.Received(1).DeleteExamAsync(1);
    }

    [Fact]
    public async Task DeleteExam_NotFound_ReturnsNotFound()
    {
        _examService.When(s => s.DeleteExamAsync(99))
            .Throw(new NotFoundException("Exam 99 not found"));

        var response = await _client.DeleteAsync("/api/exam/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostExam_PinOutOfRange_ReturnsBadRequest()
    {
        var dto = new ExamDto(0, "Test", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), 99, false, false);

        var response = await _client.PostAsJsonAsync("/api/exam", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExam_ValidPin_ReturnsCreated()
    {
        var dto     = new ExamDto(0, "Test", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), 12345, false, false);
        var teacher = new Teacher { Id = 1, LastName    = "Mustermann" };
        var created = new Exam { Id    = 1, Description = "Test", TeacherId = 1, Teacher = teacher, Created = DateTime.Today, ExamType = ExamType.Standard, Pin = 12345, From = new TimeOnly(8, 0), To = new TimeOnly(10, 0) };

        _examService.AddExamAsync(Arg.Any<Exam>()).Returns(created);

        var response = await _client.PostAsJsonAsync("/api/exam", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegisterForExam_ValidRegistration_ReturnsCreated()
    {
        var exam         = new Exam { Id        = 1, Description = "Test", CourseId  = 1, Pin = 12345, Date = new DateOnly(2026, 1, 1), ExamType = ExamType.Standard };
        var student      = new Student { Id     = 1, FirstName   = "Alice", LastName = "Smith" };
        var registration = new StudentExam { Id = 1, StudentId   = 1, ExamId         = 1, LoginName = "alice", RegistrationCode = "ABC12", Student = student, Exam = exam };
        var trans        = Substitute.For<ITransaction>();
        var dto          = new ExamRegistrationDto("Alice", "Smith", "alice", 12345);

        _examService.RegisterStudentAsync("Alice", "Smith", "alice", 12345).Returns(registration);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PostAsJsonAsync("/api/registration", dto);
        var result   = await response.Content.ReadFromJsonAsync<ExamRegistrationResultDto>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result!.LastName.Should().Be("Smith");
        result!.FirstName.Should().Be("Alice");
        result.ExamDescription.Should().Be("Test");
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task RegisterForExam_InvalidPin_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "alice", 12345);
        _examService.RegisterStudentAsync(default!, default!, default!, default).ReturnsForAnyArgs<StudentExam>(_ => throw new IllegalValuesException("No exam found with PIN 12345"));

        var response = await _client.PostAsJsonAsync("/api/registration", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_StudentNotFound_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Unknown", "User", "unknown", 12345);
        _examService.RegisterStudentAsync(default!, default!, default!, default).ReturnsForAnyArgs<StudentExam>(_ => throw new IllegalValuesException("No student found with name 'Unknown User'"));

        var response = await _client.PostAsJsonAsync("/api/registration", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_StudentNotInClass_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "alice", 12345);
        _examService.RegisterStudentAsync(default!, default!, default!, default).ReturnsForAnyArgs<StudentExam>(_ => throw new IllegalValuesException("Student 'Alice Smith' is not enrolled in the class of this exam"));

        var response = await _client.PostAsJsonAsync("/api/registration", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_AlreadyRegistered_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "alice", 12345);
        _examService.RegisterStudentAsync(default!, default!, default!, default).ReturnsForAnyArgs<StudentExam>(_ => throw new IllegalValuesException("Student 'Alice Smith' is already registered for this exam"));

        var response = await _client.PostAsJsonAsync("/api/registration", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_InvalidPinFormat_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "alice", 99);

        var response = await _client.PostAsJsonAsync("/api/registration", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_EmptyLoginName_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "", 12345);

        var response = await _client.PostAsJsonAsync("/api/registration", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}