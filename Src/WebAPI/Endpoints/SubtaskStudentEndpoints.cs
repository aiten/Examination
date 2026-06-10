namespace WebAPI.Endpoints;

using Core.Contracts;
using Core.Entities;

public record SubtaskStudentDto(int Id, int StudentExamId, string LastName, string FirstName, decimal? Result, string? Comment, string? CommentPrivate);

public record SubtaskStudentCreateDto(int StudentExamId, decimal? Result, string? Comment, string? CommentPrivate);

public static class SubtaskStudentEndpoints
{
    #region Dto-Entity Mapping

    private static SubtaskStudentDto ToDto(StudentSubtask entity) =>
        new(
            entity.Id,
            entity.StudentExamId,
            entity.StudentExam.Student.LastName,
            entity.StudentExam.Student.FirstName,
            entity.Result.HasValue ? entity.Result * 100M : null,
            entity.Comment,
            entity.CommentPrivate);

    private static IList<SubtaskStudentDto> ToDto(IList<StudentSubtask> list) =>
        list.Select(ToDto).ToList();

    #endregion


    public static void MapSubtaskStudentEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app.MapGroup($"{baseRoute}/{{examId:int}}/subtasks/{{subtaskId:int}}/students")
            .WithTags("SubtaskStudent")
            .RequireAuthorization(Settings.UserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


        route.MapGet("", async (int examId, int subtaskId, IUnitOfWork uow) =>
            {
                var subtask = await uow.Subtasks.GetByIdAsync(subtaskId);

                if (subtask is null || subtask.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Subtask not found",
                        detail: $"No Subtask found with ID {subtaskId} for exam {examId}");
                }

                var list = await uow.StudentSubtasks.GetAllForSubtaskAsync(examId, subtaskId);

                return Results.Ok(ToDto(list));
            })
            .WithName("GetSubtaskStudents")
            .Produces<List<SubtaskStudentDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapGet("/{id:int}", async (int examId, int subtaskId, int id, IUnitOfWork uow) =>
            {
                var entity = await uow.StudentSubtasks.GetByIdAsync(id, "StudentExam");

                if (entity is null || entity.SubtaskId != subtaskId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "StudentSubtask not found",
                        detail: $"No StudentSubtask found with ID {id} for subtask {subtaskId}");
                }

                return Results.Ok(ToDto(entity));
            })
            .WithName("GetSubtaskStudent")
            .Produces<SubtaskStudentDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPost("", async (int examId, int subtaskId, SubtaskStudentCreateDto dto, IUnitOfWork uow) =>
            {
                var subtask = await uow.Subtasks.GetByIdAsync(subtaskId);

                if (subtask is null || subtask.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Subtask not found",
                        detail: $"No Subtask found with ID {subtaskId} for exam {examId}");
                }

                var studentExam = await uow.StudentExams.GetByIdAsync(dto.StudentExamId);

                if (studentExam is null || studentExam.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "StudentExam not found",
                        detail: $"No StudentExam found with ID {dto.StudentExamId} for exam {examId}");
                }

                var existing = await uow.StudentSubtasks.GetNoTrackingAsync(ss => ss.SubtaskId == subtaskId && ss.StudentExamId == dto.StudentExamId);

                if (existing.Count > 0)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Duplicate entry",
                        detail: $"A result for student exam {dto.StudentExamId} already exists for subtask {subtaskId}");
                }

                var entity = new StudentSubtask
                {
                    SubtaskId      = subtaskId,
                    StudentExamId  = dto.StudentExamId,
                    Result         = dto.Result.HasValue ? dto.Result / 100M : null,
                    Comment        = dto.Comment,
                    CommentPrivate = dto.CommentPrivate
                };

                await uow.StudentSubtasks.AddAsync(entity);
                await uow.SaveChangesAsync();

                var created = await uow.StudentSubtasks.GetByIdAsync(entity.Id, "StudentExam", "StudentExam.Student");
                return Results.Created($"{examId}/subtasks/{subtaskId}/students/{entity.Id}", ToDto(created!));
            })
            .WithName("AddSubtaskStudent")
            .Produces<SubtaskStudentDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        route.MapPut("/{id:int}", async (int examId, int subtaskId, int id, SubtaskStudentDto dto, IUnitOfWork uow) =>
            {
                if (id != dto.Id)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request",
                        detail: "The ID in the URL does not match the ID in the request body");
                }

                var entity = await uow.StudentSubtasks.GetByIdAsync(id);

                if (entity is null || entity.SubtaskId != subtaskId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "StudentSubtask not found",
                        detail: $"No StudentSubtask found with ID {id} for subtask {subtaskId}");
                }

                entity.Result         = dto.Result.HasValue ? dto.Result / 100M : null;
                entity.Comment        = dto.Comment;
                entity.CommentPrivate = dto.CommentPrivate;
                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("UpdateSubtaskStudent")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
