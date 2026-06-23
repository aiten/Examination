namespace WebAPI.Endpoints;

using Persistence;
using Persistence.Model;

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


        route.MapGet("", async (int examId, IUnitOfWork uow) =>
            {
                var subtasks = await uow.Subtasks.GetNoTrackingAsync(s => s.ExamId == examId);
                return Results.Ok(ToDto(subtasks));
            })
            .WithName("GetSubtasks")
            .Produces<List<SubtaskDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int examId, int id, IUnitOfWork uow) =>
            {
                var entity = await uow.Subtasks.GetByIdAsync(id);

                if (entity is null || entity.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Subtask not found",
                        detail: $"No Subtask found with ID {id} for exam {examId}");
                }

                return Results.Ok(ToDto(entity));
            })
            .WithName("GetSubtask")
            .Produces<SubtaskDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPost("", async (int examId, SubtaskDto dto, IUnitOfWork uow) =>
            {
                if (dto.Id != 0)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request",
                        detail: "The ID in the request body must be 0");
                }

                using var trans = await uow.BeginTransactionAsync();
                var entity = new Subtask
                {
                    ExamId      = examId,
                    SeqNo       = dto.SeqNo,
                    Description = dto.Description,
                    Points      = dto.Points,
                    Bonus       = dto.Bonus
                };

                await uow.Subtasks.AddAsync(entity);
                await trans.CommitTransactionAsync();

                int id = entity.Id;

                return Results.Created($"{baseRoute}/{examId}/subtask/{id}",
                    ToDto((await uow.Subtasks.GetByIdAsync(id))!));
            })
            .WithValidation<SubtaskDto>()
            .WithName("AddSubtask")
            .Produces<SubtaskDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        route.MapPut("/{id:int}", async (int examId, int id, SubtaskDto dto, IUnitOfWork uow) =>
            {
                if (id != dto.Id)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request",
                        detail: "The ID in the URL does not match the ID in the request body");
                }

                using (var trans = await uow.BeginTransactionAsync())
                {
                    var entity = await uow.Subtasks.GetByIdAsync(id);

                    if (entity is null || entity.ExamId != examId)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Subtask not found",
                            detail: $"No Subtask found with ID {id} for exam {examId}");
                    }

                    entity.Description = dto.Description;
                    entity.Points      = dto.Points;
                    entity.SeqNo       = dto.SeqNo;
                    entity.Bonus       = dto.Bonus;

                    await trans.CommitTransactionAsync();
                }

                return Results.NoContent();
            })
            .WithValidation<SubtaskDto>()
            .WithName("UpdateSubtask")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        route.MapDelete("/{id:int}", async (int examId, int id, IUnitOfWork uow) =>
            {
                var entity = await uow.Subtasks.GetByIdAsync(id);

                if (entity is null || entity.ExamId != examId)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Subtask not found",
                        detail: $"No Subtask found with ID {id} for exam {examId}");
                }

                uow.Subtasks.Remove(entity);
                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteSubtask")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}