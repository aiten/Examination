namespace WebAPI.Endpoints;

using Persistence;
using Persistence.Model;

using WebAPI.Filters;

public record SubjectDto(
    int    Id,
    string Name
);

public static class SubjectEndpoints
{
    #region Dto-Entity Mapping

    private static Subject ToEntity(SubjectDto dto)
    {
        return new Subject()
        {
            Id   = dto.Id,
            Name = dto.Name
        };
    }

    private static SubjectDto? ToDto(Subject? entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new SubjectDto(entity.Id, entity.Name);
    }

    private static IList<SubjectDto>? ToDto(IList<Subject>? list)
    {
        if (list is null)
        {
            return null;
        }

        return list.Select(x => ToDto(x)!).ToList();
    }

    #endregion

    public static void MapSubjectEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Subject")
            .RequireAuthorization(Settings.AdminOrUserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        var routeAdmin = app
            .MapGroup(baseRoute)
            .WithTags("Subject")
            .RequireAuthorization(Settings.AdminPolicyName);

        route.MapGet("", async (IUnitOfWork uow) =>
            {
                var dtos = ToDto(await uow.Subjects.GetNoTrackingAsync());
                return Results.Ok(dtos);
            })
            .WithName("GetSubjects")
            .Produces<List<SubjectDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var dto = ToDto(await uow.Subjects.GetByIdAsync(id));

                if (dto is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Subject not found",
                        detail: $"No Subject found with ID {id}");
                }

                return Results.Ok(dto);
            })
            .WithName("GetSubject")
            .Produces<SubjectDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPut("/{id:int}", async (int id, SubjectDto dto, IUnitOfWork uow) =>
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
                    var entity = await uow.Subjects.GetByIdAsync(id);

                    if (entity is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Subject not found",
                            detail: $"No Subject found with ID {id}");
                    }

                    entity.Name = dto.Name;

                    await trans.CommitTransactionAsync();
                }

                return Results.NoContent();
            })
            .WithName("UpdateSubject")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPost("", async (SubjectDto dto, IUnitOfWork uow) =>
            {
                if (dto.Id != 0)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request",
                        detail: "The ID in the request body must be 0");
                }

                using (var trans = await uow.BeginTransactionAsync())
                {
                    var entity = ToEntity(dto);

                    await uow.Subjects.AddAsync(entity);

                    await trans.CommitTransactionAsync();

                    int id = entity.Id;

                    return Results.Created($"{baseRoute}/{id}", ToDto(await uow.Subjects.GetByIdAsync(id)));
                }
            })
            .WithName("AddSubject")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var entity = await uow.Subjects.GetByIdAsync(id);

                if (entity is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Subject not found",
                        detail: $"No Subject found with ID {id}");
                }

                uow.Subjects.Remove(entity);

                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteSubject")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}