namespace WebAPI.Endpoints;

using Core.Contracts;
using Core.Entities;
using Core.QueryResult;

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
                      ss.Result.HasValue ? ss.Result * 100: null,
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


        route.MapGet("", async (int examId, IUnitOfWork uow) =>
            {
                var overviews = await uow.StudentExams.GetStudentExamOverviewsAsync(examId);
                return Results.Ok(overviews);
            })
            .WithName("GetStudentExams")
            .Produces<List<StudentExamOverview>>(StatusCodes.Status200OK);

        route.MapGet("summary", async (int examId, IUnitOfWork uow) =>
            {
                var summary = await uow.StudentExams.GetStudentExamSummaryAsync(examId);
                return Results.Ok(summary);
            })
            .WithName("GetStudentExamsSummary")
            .Produces<List<StudentExamSummary>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int examId, int id, IUnitOfWork uow) =>
            {
                var entity = await uow.StudentExams.GetByIdAsync(id,
                    "Student",
                    "StudentSubtasks",
                    "StudentSubtasks.Subtask");

                if (entity is null || entity.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "StudentExam not found",
                        detail: $"No StudentExam found with ID {id} for exam {examId}");
                }

                return Results.Ok(ToDto(entity));
            })
            .WithName("GetStudentExam")
            .Produces<StudentExamDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPut("/{id:int}", async (int examId, int id, StudentExamDto dto, IUnitOfWork uow) =>
            {
                if (id != dto.Id)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request",
                        detail: "The ID in the URL does not match the ID in the request body");
                }

                var entity = await uow.StudentExams.GetByIdAsync(id);

                if (entity is null || entity.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "StudentExam not found",
                        detail: $"No StudentExam found with ID {id} for exam {examId}");
                }

                entity.LoginName        = dto.LoginName;
                entity.RegistrationCode = dto.RegistrationCode;
                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("UpdateStudentExam")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapDelete("/{id:int}", async (int examId, int id, IUnitOfWork uow) =>
            {
                var entity = await uow.StudentExams.GetByIdAsync(id);

                if (entity is null || entity.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "StudentExam not found",
                        detail: $"No StudentExam found with ID {id} for exam {examId}");
                }

                uow.StudentExams.Remove(entity);
                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteStudentExam")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}
