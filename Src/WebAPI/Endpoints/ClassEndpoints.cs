namespace WebAPI.Endpoints;

using Persistence;
using Persistence.Model;

using WebAPI.Filters;

public record ClassDto(
    int    Id,
    string Description,
    int    Year,
    int?   TeacherId
);

public static class ClassEndpoints
{
    #region Dto-Entity Mapping

    private static Class ToEntity(ClassDto dto)
    {
        return new Class()
        {
            Id          = dto.Id,
            Description = dto.Description,
            Year        = dto.Year,
            TeacherId   = dto.TeacherId
        };
    }

    private static ClassDto? ToDto(Class? entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new ClassDto(
            entity.Id,
            entity.Description,
            entity.Year,
            entity.TeacherId
        );
    }

    private static IList<ClassDto>? ToDto(IList<Class>? list)
    {
        if (list is null)
        {
            return null;
        }

        return list.Select(x => ToDto(x)!).ToList();
    }

    #endregion


    public static void MapClassEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Class")
            .RequireAuthorization(Settings.AdminOrUserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        var routeAdmin = app
            .MapGroup(baseRoute)
            .WithTags("Class")
            .RequireAuthorization(Settings.AdminPolicyName);

        route.MapGet("", async (IUnitOfWork uow) =>
            {
                var dtos = ToDto(await uow.Classes.GetNoTrackingAsync());
                return Results.Ok(dtos);
            })
            .WithName("GetClasses")
            .Produces<List<ClassDto>>(StatusCodes.Status200OK);


        route.MapGet("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var dto = ToDto(await uow.Classes.GetByIdAsync(id));

                if (dto is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Class not found",
                        detail: $"No Class found with ID {id}");
                }

                return Results.Ok(dto);
            })
            .WithName("GetClass")
            .Produces<ClassDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPut("/{id:int}", async (int id, ClassDto dto, IUnitOfWork uow) =>
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
                    var entity = await uow.Classes.GetByIdAsync(id);

                    if (entity is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Class not found",
                            detail: $"No Class found with ID {id}");
                    }

                    entity.Description = dto.Description;
                    entity.Year        = dto.Year;
                    entity.TeacherId   = dto.TeacherId;

                    await trans.CommitTransactionAsync();
                }

                return Results.NoContent();
            })
            .WithValidation<ClassDto>()
            .WithName("UpdateClass")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);


        routeAdmin.MapPost("", async (ClassDto dto, IUnitOfWork uow) =>
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

                    await uow.Classes.AddAsync(entity);

                    await trans.CommitTransactionAsync();

                    int id = entity.Id;

                    return Results.Created($"{baseRoute}/{id}", ToDto(await uow.Classes.GetByIdAsync(id)));
                }
            })
            .WithValidation<ClassDto>()
            .WithName("AddClass")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var entity = await uow.Classes.GetByIdAsync(id);

                if (entity is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Class not found",
                        detail: $"No Class found with ID {id}");
                }

                uow.Classes.Remove(entity);

                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteClass")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}