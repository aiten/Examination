namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;
using Persistence.QueryResult;

using Service;

public record StudentSubtaskResultDto(int SubtaskId, string Description, int Points, decimal? Result, string? Comment, string? CommentPrivate);

public record StudentExamDto(
    int                            Id,
    int                            ExamId,
    int                            StudentId,
    string                         FirstName,
    string                         LastName,
    string                         LoginName,
    string                         RegistrationCode,
    IList<StudentSubtaskResultDto> Subtasks
);

public record StudentSubtaskUpdateDto(int SubtaskId, decimal? Result, string? Comment, string? CommentPrivate);

public static class StudentExamEndpoints
{
    #region Dto-Entity Mapping

    private static StudentExamDto ToDto(StudentExam entity) =>
        new(
            entity.Id,
            entity.ExamId,
            entity.StudentId,
            entity.Student.FirstName,
            entity.Student.LastName,
            entity.LoginName,
            entity.RegistrationCode,
            entity.StudentSubtasks
                .Select(ss => new StudentSubtaskResultDto(
                    ss.SubtaskId,
                    ss.Subtask.Description,
                    ss.Subtask.Points,
                    ss.Result.HasValue ? ss.Result * 100 : null,
                    ss.Comment,
                    ss.CommentPrivate))
                .ToList()
        );

    #endregion

    public static void MapStudentExamEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup($"{baseRoute}/{{examId:int}}/students")
            .WithTags("StudentExam")
            .RequireAuthorization(Settings.UserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


        route.MapGet("", async (int examId, IStudentExamService studentExamService, ITransactionProvider transactionProvider) =>
            {
                var overviews = await studentExamService.GetStudentExamOverviewsAsync(examId);
                return Results.Ok(overviews);
            })
            .WithName("GetStudentExams")
            .Produces<List<StudentExamOverview>>(StatusCodes.Status200OK);

        route.MapGet("summary", async (int examId, IStudentExamService studentExamService, ITransactionProvider transactionProvider) =>
            {
                var summary = await studentExamService.GetStudentExamSummaryAsync(examId);
                return Results.Ok(summary);
            })
            .WithName("GetStudentExamsSummary")
            .Produces<List<StudentExamSummary>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int examId, int id, IStudentExamService studentExamService, ITransactionProvider transactionProvider) =>
            {
                var entity = await studentExamService.SingleStudentExamAsync(id,
                    nameof(StudentExam.Student),
                    nameof(StudentExam.StudentSubtasks),
                    $"{nameof(StudentExam.StudentSubtasks)}.{nameof(StudentSubtask.Subtask)}");

                EndpointTools.CheckExamId(examId, entity.ExamId);

                return Results.Ok(ToDto(entity));
            })
            .WithName("GetStudentExam")
            .Produces<StudentExamDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPut("/{id:int}", async (int examId, int id, StudentExamDto dto, IStudentExamService studentExamService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();

                var entity = new StudentExam()
                {
                    LoginName        = dto.LoginName,
                    RegistrationCode = dto.RegistrationCode,
                    ExamId           = examId
                };

                await studentExamService.UpdateStudentExamAsync(id, entity);
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("UpdateStudentExam")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapDelete("/{id:int}", async (int examId, int id, IStudentExamService studentExamService, ITransactionProvider transactionProvider) =>
            {
                using var trans = await transactionProvider.BeginTransactionAsync();

                var entity = await studentExamService.SingleStudentExamAsync(id);

                EndpointTools.CheckExamId(examId, entity.ExamId);

                await studentExamService.DeleteStudentExamAsync(id);

                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("DeleteStudentExam")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}