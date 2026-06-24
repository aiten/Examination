namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

using Shared.Exceptions;

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


        route.MapGet("", async (int examId, int subtaskId, IStudentSubtaskService studentSubtaskService, ITransactionProvider transactionProvider) =>
            {
                var list = await studentSubtaskService.GetAllForSubtaskAsync(examId, subtaskId);
                return Results.Ok(ToDto(list));
            })
            .WithName("GetSubtaskStudents")
            .Produces<List<SubtaskStudentDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapGet("/{id:int}", async (int examId, int subtaskId, int id, IStudentSubtaskService studentSubtaskService, ITransactionProvider transactionProvider) =>
            {
                await studentSubtaskService.CheckValid(id, examId, subtaskId);
                var entity = await studentSubtaskService.SingleStudentSubtaskAsync(id, nameof(StudentSubtask.StudentExam));

                return Results.Ok(ToDto(entity));
            })
            .WithName("GetSubtaskStudent")
            .Produces<SubtaskStudentDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPost("", async (int examId, int subtaskId, SubtaskStudentCreateDto dto, IStudentSubtaskService studentSubtaskService, ITransactionProvider transactionProvider) =>
            {
                using var trans = await transactionProvider.BeginTransactionAsync();

                var entity = new StudentSubtask
                {
                    SubtaskId      = subtaskId,
                    StudentExamId  = dto.StudentExamId,
                    Result         = dto.Result.HasValue ? dto.Result / 100M : null,
                    Comment        = dto.Comment,
                    CommentPrivate = dto.CommentPrivate
                };

                await studentSubtaskService.AddStudentSubtaskAsync(examId, entity);

                await trans.CommitTransactionAsync();

                var created = await studentSubtaskService.GetStudentSubtaskByIdAsync(entity.Id, "StudentExam", "StudentExam.Student");
                return Results.Created($"{examId}/subtasks/{subtaskId}/students/{entity.Id}", ToDto(created!));
            })
            .WithName("AddSubtaskStudent")
            .Produces<SubtaskStudentDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        route.MapPut("/{id:int}", async (int examId, int subtaskId, int id, SubtaskStudentDto dto, IStudentSubtaskService studentSubtaskService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();

                var entity = new StudentSubtask()
                {
                    SubtaskId      = subtaskId,
                    StudentExamId  = dto.StudentExamId,
                    Result         = dto.Result.HasValue ? dto.Result / 100M : null,
                    Comment        = dto.Comment,
                    CommentPrivate = dto.CommentPrivate
                };

                await studentSubtaskService.UpdateStudentSubtaskAsync(id, examId, entity);

                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("UpdateSubtaskStudent")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}