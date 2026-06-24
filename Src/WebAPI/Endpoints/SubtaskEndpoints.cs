namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

using WebAPI.Filters;

public record SubtaskDto(int Id, int SeqNo, string Description, int Points, bool Bonus);

public static class SubtaskEndpoints
{
    #region Dto-Entity Mapping

    private static SubtaskDto ToDto(Subtask entity) =>
        new(entity.Id, entity.SeqNo, entity.Description, entity.Points, entity.Bonus);

    private static IList<SubtaskDto> ToDto(IList<Subtask> list) =>
        list.Select(ToDto).ToList();

    #endregion

    public static void MapSubtaskEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup($"{baseRoute}/{{examId:int}}/subtask")
            .WithTags("Subtask")
            .RequireAuthorization(Settings.UserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


        route.MapGet("", async (int examId, ISubtaskService subtaskService, ITransactionProvider transactionProvider) =>
            {
                var subtasks = await subtaskService.GetSubtasksForExamAsync(examId);
                return Results.Ok(ToDto(subtasks));
            })
            .WithName("GetSubtasks")
            .Produces<List<SubtaskDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int examId, int id, ISubtaskService subtaskService, ITransactionProvider transactionProvider) =>
            {
                var entity = await subtaskService.SingleSubtaskAsync(id);
                EndpointTools.CheckExamId(examId, entity.ExamId);
                return Results.Ok(ToDto(entity));
            })
            .WithName("GetSubtask")
            .Produces<SubtaskDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPost("", async (int examId, SubtaskDto dto, ISubtaskService subtaskService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckIdMustBe0(dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();
                var entity = new Subtask
                {
                    ExamId      = examId,
                    SeqNo       = dto.SeqNo,
                    Description = dto.Description,
                    Points      = dto.Points,
                    Bonus       = dto.Bonus
                };

                await subtaskService.AddSubtaskAsync(entity);
                await trans.CommitTransactionAsync();

                int id = entity.Id;

                return Results.Created($"{baseRoute}/{examId}/subtask/{id}",
                    ToDto((await subtaskService.GetSubtaskByIdAsync(id))!));
            })
            .WithValidation<SubtaskDto>()
            .WithName("AddSubtask")
            .Produces<SubtaskDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        route.MapPut("/{id:int}", async (int examId, int id, SubtaskDto dto, ISubtaskService subtaskService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();
                
                var entity = new Subtask()
                {
                    Description = dto.Description,
                    Points      = dto.Points, SeqNo = dto.SeqNo,
                    Bonus       = dto.Bonus,
                    ExamId      = examId
                };

                await subtaskService.UpdateSubtaskAsync(id, entity);

                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithValidation<SubtaskDto>()
            .WithName("UpdateSubtask")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        route.MapDelete("/{id:int}", async (int examId, int id, ISubtaskService subtaskService, ITransactionProvider transactionProvider) =>
            {
                using var trans  = await transactionProvider.BeginTransactionAsync();

                var       entity = await subtaskService.SingleSubtaskAsync(id);

                EndpointTools.CheckExamId(examId,entity.ExamId);

                await subtaskService.DeleteSubtaskAsync(id);

                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("DeleteSubtask")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}