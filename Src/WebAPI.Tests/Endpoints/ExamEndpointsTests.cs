using System.Net;
using System.Net.Http.Json;

using Base.Core.Contracts;

using Core.Contracts;
using Core.Entities;
using Core.QueryResult;

using FluentAssertions;

using Microsoft.EntityFrameworkCore.ChangeTracking;

using NSubstitute;

using WebAPI.Endpoints;

namespace WebAPI.Tests.Endpoints;

public class ExamEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient              _client;
    private readonly IUnitOfWork             _uow;
    private readonly IExamRepository         _examRepo;
    private readonly IStudentRepository      _studentRepo;
    private readonly IStudentExamRepository  _studentExamRepo;

    public ExamEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _uow    = factory.UnitOfWork;
        _uow.ClearReceivedCalls();
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
        _examRepo.GetNoTrackingAsync().ReturnsForAnyArgs(exams);

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
            new(1, "Kinder, lernt!", 123, "Mustermann", "2AHIF", new DateOnly(2026, 5, 7), new TimeOnly(8, 0), new TimeOnly(10, 0), ["X", "Y"], ["S1", "S2"])
        };
        _examRepo.GetExamOverviewsAsync(null, null).ReturnsForAnyArgs(overviews);

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
        _examRepo.GetByIdAsync(1).ReturnsForAnyArgs(exam);

        var response = await _client.GetAsync("/api/exam/1");
        var result   = await response.Content.ReadFromJsonAsync<ExamDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Id.Should().Be(1);
        result.Description.Should().Be("Kinder, lernt!");
    }

    [Fact]
    public async Task GetExam_NonExistingId_Returns404()
    {
        _examRepo.GetByIdAsync(99).ReturnsForAnyArgs((Exam?)null);

        var response = await _client.GetAsync("/api/exam/99");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostExam_ValidDto_ReturnsCreated()
    {
        var dto     = new ExamDto(0, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null);
        var teacher = new Teacher { Id = 1, LastName    = "Mustermann" };
        var created = new Exam { Id    = 1, Description = "Kinder, lernt!", TeacherId = 1, Teacher = teacher, Created = DateTime.Today, ExamType = ExamType.Standard, From = new TimeOnly(8, 0), To = new TimeOnly(10, 0) };

        _examRepo.AddAsync(Arg.Any<Exam>())
            .Returns(Task.FromResult<EntityEntry<Exam>>(null!));
        _examRepo.GetByIdAsync(Arg.Any<int>()).ReturnsForAnyArgs(created);

        var response = await _client.PostAsJsonAsync("/api/exam", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostExam_NonZeroId_ReturnsBadRequest()
    {
        var dto = new ExamDto(5, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null);

        var response = await _client.PostAsJsonAsync("/api/exam", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutExam_ValidUpdate_ReturnsNoContent()
    {
        var teacher  = new Teacher { Id = 1, LastName    = "Mustermann" };
        var existing = new Exam { Id    = 1, Description = "Old", TeacherId = 1, Teacher = teacher, Created = DateTime.Today, ExamType = ExamType.Standard };
        var dto      = new ExamDto(1, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null);
        var trans    = Substitute.For<ITransaction>();
        _examRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PutAsJsonAsync("/api/exam/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task PutExam_IdMismatch_ReturnsBadRequest()
    {
        var dto = new ExamDto(2, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null);

        var response = await _client.PutAsJsonAsync("/api/exam/1", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutExam_NotFound_ReturnsBadRequest()
    {
        var dto = new ExamDto(99, "Kinder, lernt!", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), null);
        _examRepo.GetByIdAsync(99).ReturnsForAnyArgs((Exam?)null);

        var response = await _client.PutAsJsonAsync("/api/exam/99", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteExam_Existing_ReturnsNoContent()
    {
        var teacher  = new Teacher { Id = 1, LastName    = "Mustermann" };
        var existing = new Exam { Id    = 1, Description = "Kinder, lernt!", TeacherId = 1, Teacher = teacher, Created = DateTime.Today, ExamType = ExamType.Standard };
        _examRepo.GetByIdAsync(1).ReturnsForAnyArgs(existing);

        var response = await _client.DeleteAsync("/api/exam/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _examRepo.Received(1).Remove(existing);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteExam_NotFound_ReturnsBadRequest()
    {
        _examRepo.GetByIdAsync(99).ReturnsForAnyArgs((Exam?)null);

        var response = await _client.DeleteAsync("/api/exam/99");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExam_PinOutOfRange_ReturnsBadRequest()
    {
        var dto = new ExamDto(0, "Test", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), 99);

        var response = await _client.PostAsJsonAsync("/api/exam", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExam_ValidPin_ReturnsCreated()
    {
        var dto     = new ExamDto(0, "Test", (int)ExamType.Standard, 1, 1, new DateOnly(2026, 1, 1), new TimeOnly(8, 0), new TimeOnly(10, 0), 123);
        var teacher = new Teacher { Id = 1, LastName    = "Mustermann" };
        var created = new Exam { Id    = 1, Description = "Test", TeacherId = 1, Teacher = teacher, Created = DateTime.Today, ExamType = ExamType.Standard, Pin = 123, From = new TimeOnly(8, 0), To = new TimeOnly(10, 0) };

        _examRepo.AddAsync(Arg.Any<Exam>()).Returns(Task.FromResult<EntityEntry<Exam>>(null!));
        _examRepo.GetByIdAsync(Arg.Any<int>()).ReturnsForAnyArgs(created);

        var response = await _client.PostAsJsonAsync("/api/exam", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegisterForExam_ValidRegistration_ReturnsCreated()
    {
        var exam         = new Exam { Id = 1, Description = "Test", CourseId = 1, Pin = 123, Date = new DateOnly(2026, 1, 1), ExamType = ExamType.Standard };
        var student      = new Student { Id = 1, FirstName = "Alice", LastName = "Smith" };
        var registration = new StudentExam { Id = 1, StudentId = 1, ExamId = 1, LoginName = "alice", RegistrationCode = "ABC12", Student = student, Exam = exam };
        var trans        = Substitute.For<ITransaction>();
        var dto          = new ExamRegistrationDto("Alice", "Smith", "alice", 123);

        _examRepo.RegisterStudentAsync("Alice", "Smith", "alice", 123).Returns(registration);
        _uow.BeginTransactionAsync().Returns(trans);

        var response = await _client.PostAsJsonAsync("/api/exam/register", dto);
        var result   = await response.Content.ReadFromJsonAsync<ExamRegistrationResultDto>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result!.StudentName.Should().Be("Alice Smith");
        result.ExamDescription.Should().Be("Test");
        await trans.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task RegisterForExam_InvalidPin_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "alice", 123);
        _examRepo.RegisterStudentAsync(default!, default!, default!, default).ReturnsForAnyArgs<StudentExam>(_ => throw new InvalidOperationException("No exam found with PIN 123"));

        var response = await _client.PostAsJsonAsync("/api/exam/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_StudentNotFound_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Unknown", "User", "unknown", 123);
        _examRepo.RegisterStudentAsync(default!, default!, default!, default).ReturnsForAnyArgs<StudentExam>(_ => throw new InvalidOperationException("No student found with name 'Unknown User'"));

        var response = await _client.PostAsJsonAsync("/api/exam/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_StudentNotInClass_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "alice", 123);
        _examRepo.RegisterStudentAsync(default!, default!, default!, default).ReturnsForAnyArgs<StudentExam>(_ => throw new InvalidOperationException("Student 'Alice Smith' is not enrolled in the class of this exam"));

        var response = await _client.PostAsJsonAsync("/api/exam/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_AlreadyRegistered_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "alice", 123);
        _examRepo.RegisterStudentAsync(default!, default!, default!, default).ReturnsForAnyArgs<StudentExam>(_ => throw new InvalidOperationException("Student 'Alice Smith' is already registered for this exam"));

        var response = await _client.PostAsJsonAsync("/api/exam/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_InvalidPinFormat_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "alice", 99);

        var response = await _client.PostAsJsonAsync("/api/exam/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterForExam_EmptyLoginName_ReturnsBadRequest()
    {
        var dto = new ExamRegistrationDto("Alice", "Smith", "", 123);

        var response = await _client.PostAsJsonAsync("/api/exam/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}