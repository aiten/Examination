namespace WebAPI.Endpoints;

using Core.Contracts;
using Core.Entities;

public record StudentSubtaskDto(int Id, int SubtaskId, string Description, int Points, bool Bonus, int SeqNo, decimal? Result, string? Comment, string? CommentPrivate);

public record StudentSubtaskCreateDto(int SubtaskId, decimal? Result, string? Comment, string? CommentPrivate);

public static class StudentSubtaskEndpoints
{
    #region Dto-Entity Mapping

    private static StudentSubtaskDto ToDto(StudentSubtask entity) =>
        new(
            entity.Id,
            entity.SubtaskId,
            entity.Subtask.Description,
            entity.Subtask.Points,
            entity.Subtask.Bonus,
            entity.Subtask.SeqNo,
            entity.Result.HasValue ? (entity.Result) * 100M : null,
            entity.Comment,
            entity.CommentPrivate);

    private static IList<StudentSubtaskDto> ToDto(IList<StudentSubtask> list) =>
        list.Select(ToDto).ToList();

    #endregion


    public static void MapStudentSubtaskEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app.MapGroup($"{baseRoute}/{{examId:int}}/students/{{studentExamId:int}}/subtasks")
            .WithTags("StudentSubtask")
            .RequireAuthorization(Settings.UserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


        route.MapGet("", async (int examId, int studentExamId, IUnitOfWork uow) =>
            {
                var studentExam = await uow.StudentExams.GetByIdAsync(studentExamId);

                if (studentExam is null || studentExam.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "StudentExam not found",
                        detail: $"No StudentExam found with ID {studentExamId} for exam {examId}");
                }

                var list = await uow.StudentSubtasks.GetAllPossibleAsync(examId, studentExamId);

                return Results.Ok(ToDto(list));
            })
            .WithName("GetStudentSubtasks")
            .Produces<List<StudentSubtaskDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapGet("/{id:int}", async (int examId, int studentExamId, int id, IUnitOfWork uow) =>
            {
                var entity = await uow.StudentSubtasks.GetByIdAsync(id, "Subtask");

                if (entity is null || entity.StudentExamId != studentExamId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "StudentSubtask not found",
                        detail: $"No StudentSubtask found with ID {id} for student exam {studentExamId}");
                }

                return Results.Ok(ToDto(entity));
            })
            .WithName("GetStudentSubtask")
            .Produces<StudentSubtaskDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPost("", async (int examId, int studentExamId, StudentSubtaskCreateDto dto, IUnitOfWork uow) =>
            {
                var studentExam = await uow.StudentExams.GetByIdAsync(studentExamId);

                if (studentExam is null || studentExam.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "StudentExam not found",
                        detail: $"No StudentExam found with ID {studentExamId} for exam {examId}");
                }

                var subtask = await uow.Subtasks.GetByIdAsync(dto.SubtaskId);

                if (subtask is null || subtask.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Subtask not found",
                        detail: $"No Subtask found with ID {dto.SubtaskId} for exam {examId}");
                }

                var existing = await uow.StudentSubtasks.GetNoTrackingAsync(ss => ss.StudentExamId == studentExamId && ss.SubtaskId == dto.SubtaskId);

                if (existing.Count > 0)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Duplicate entry",
                        detail: $"A result for subtask {dto.SubtaskId} already exists for student exam {studentExamId}");
                }

                var entity = new StudentSubtask
                {
                    StudentExamId  = studentExamId,
                    SubtaskId      = dto.SubtaskId,
                    Result         = dto.Result.HasValue ? dto.Result / 100M : null,
                    Comment        = dto.Comment,
                    CommentPrivate = dto.CommentPrivate
                };

                await uow.StudentSubtasks.AddAsync(entity);
                await uow.SaveChangesAsync();

                var created = await uow.StudentSubtasks.GetByIdAsync(entity.Id, "Subtask");
                return Results.Created($"{examId}/students/{studentExamId}/subtasks/{entity.Id}", ToDto(created!));
            })
            .WithName("AddStudentSubtask")
            .Produces<StudentSubtaskDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        route.MapPut("/{id:int}", async (int examId, int studentExamId, int id, StudentSubtaskDto dto, IUnitOfWork uow) =>
            {
                if (id != dto.Id)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request",
                        detail: "The ID in the URL does not match the ID in the request body");
                }

                var entity = await uow.StudentSubtasks.GetByIdAsync(id);

                if (entity is null || entity.StudentExamId != studentExamId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "StudentSubtask not found",
                        detail: $"No StudentSubtask found with ID {id} for student exam {studentExamId}");
                }

                entity.Result         = dto.Result.HasValue ? dto.Result / 100M : null;
                entity.Comment        = dto.Comment;
                entity.CommentPrivate = dto.CommentPrivate;
                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("UpdateStudentSubtask")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPut("/bulk/{id:int}", async (int examId, int id, IList<StudentSubtaskUpdateDto> updates, IUnitOfWork uow) =>
            {
                var entity = await uow.StudentExams.GetByIdAsync(id, "StudentSubtasks");

                if (entity is null || entity.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "StudentExam not found",
                        detail: $"No StudentExam found with ID {id} for exam {examId}");
                }

                foreach (var update in updates)
                {
                    var ss = entity.StudentSubtasks.FirstOrDefault(s => s.SubtaskId == update.SubtaskId);
                    if (ss is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Subtask not found",
                            detail: $"Subtask {update.SubtaskId} is not part of this student exam");
                    }
                    ss.Result         = update.Result.HasValue ? update.Result / 100 : null;
                    ss.Comment        = update.Comment;
                    ss.CommentPrivate = update.CommentPrivate;
                }

                await uow.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdateStudentExamResults")
            .Accepts<IList<StudentSubtaskUpdateDto>>("application/json")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);

    }
}