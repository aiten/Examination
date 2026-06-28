namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

public record StudentSubtaskDto(int Id, int SubtaskId, string Description, int Points, bool Bonus, int SeqNo, decimal? Result, string? Comment, string? CommentPrivate, DateOnly? Date);

public record StudentSubtaskCreateDto(int SubtaskId, decimal? Result, string? Comment, string? CommentPrivate, DateOnly? Date);

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
            entity.CommentPrivate,
            entity.Date);

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


        route.MapGet("", async (int examId, int studentExamId, IStudentSubtaskService studentSubtaskService, ITransactionProvider transactionProvider) =>
            {
                var list = await studentSubtaskService.GetAllForStudentAsync(examId, studentExamId);
                return Results.Ok(ToDto(list));
            })
            .WithName("GetStudentSubtasks")
            .Produces<List<StudentSubtaskDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapGet("/{id:int}", async (int examId, int studentExamId, int id, IStudentSubtaskService studentSubtaskService, ITransactionProvider transactionProvider) =>
            {
                await studentSubtaskService.CheckValidStudentExam(id, examId, studentExamId);
                var entity = await studentSubtaskService.SingleStudentSubtaskAsync(id, nameof(StudentSubtask.Subtask));

                return Results.Ok(ToDto(entity));
            })
            .WithName("GetStudentSubtask")
            .Produces<StudentSubtaskDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPost("", async (int examId, int studentExamId, StudentSubtaskCreateDto dto, IStudentSubtaskService studentSubtaskService, ITransactionProvider transactionProvider) =>
            {
                using var trans = await transactionProvider.BeginTransactionAsync();

                var entity = new StudentSubtask
                {
                    SubtaskId      = dto.SubtaskId,
                    StudentExamId  = studentExamId,
                    Result         = dto.Result.HasValue ? dto.Result / 100M : null,
                    Comment        = dto.Comment,
                    CommentPrivate = dto.CommentPrivate,
                    Date           = dto.Date
                };

                await studentSubtaskService.AddStudentSubtaskAsync(examId, entity);

                await trans.CommitTransactionAsync();

                var created = await studentSubtaskService.GetStudentSubtaskByIdAsync(entity.Id, "StudentExam", "StudentExam.Student");
                return Results.Created($"{examId}/students/{studentExamId}/subtasks/{entity.Id}", ToDto(created!));
            })
            .WithName("AddStudentSubtask")
            .Produces<StudentSubtaskDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        route.MapPut("/{id:int}", async (int examId, int studentExamId, int id, StudentSubtaskDto dto, IStudentSubtaskService studentSubtaskService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();

                var entity = new StudentSubtask()
                {
                    SubtaskId      = dto.SubtaskId,
                    StudentExamId  = studentExamId,
                    Result         = dto.Result.HasValue ? dto.Result / 100M : null,
                    Comment        = dto.Comment,
                    CommentPrivate = dto.CommentPrivate,
                    Date           = dto.Date
                };

                await studentSubtaskService.UpdateStudentSubtaskAsync(id, examId, entity);

                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("UpdateStudentSubtask")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        /*
                route.MapPut("/bulk/{id:int}", async (int examId, int id, IList<StudentSubtaskUpdateDto> updates, IStudentSubtaskService studentSubtaskService, ITransactionProvider transactionProvider) =>
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
        */
    }
}