namespace WebAPI.Endpoints;

using Core.Contracts;
using Core.Entities;

using WebAPI.Filters;

public record TeacherDto(
    int     Id,
    string? FirstName,
    string  LastName,
    string? NickName,
    string? Abbreviation
);

public static class TeacherEndpoints
{
    #region Dto-Entity Mapping

    private static Teacher ToEntity(TeacherDto dto)
    {
        return new Teacher()
        {
            Id           = dto.Id,
            FirstName    = dto.FirstName,
            LastName     = dto.LastName,
            NickName     = dto.NickName,
            Abbreviation = dto.Abbreviation
        };
    }

    private static TeacherDto? ToDto(Teacher? entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new TeacherDto(
            entity.Id,
            entity.FirstName,
            entity.LastName,
            entity.NickName,
            entity.Abbreviation
        );
    }

    private static IList<TeacherDto>? ToDto(IList<Teacher>? list)
    {
        if (list is null)
        {
            return null;
        }

        return list.Select(x => ToDto(x)!).ToList();
    }

    #endregion

    public static void MapTeacherEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Teacher")
            .RequireAuthorization(Settings.AdminOrUserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        var routeAdmin = app
            .MapGroup(baseRoute)
            .WithTags("Teacher")
            .RequireAuthorization(Settings.AdminPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        route.MapGet("", async (IUnitOfWork uow) =>
            {
                var dtos = ToDto(await uow.Teachers.GetNoTrackingAsync());
                return Results.Ok(dtos);
            })
            .WithName("GetTeachers")
            .Produces<List<TeacherDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var dto = ToDto(await uow.Teachers.GetByIdAsync(id));

                if (dto is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Teacher not found",
                        detail: $"No Teacher found with ID {id}");
                }

                return Results.Ok(dto);
            })
            .WithName("GetTeacher")
            .Produces<TeacherDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPut("/{id:int}", async (int id, TeacherDto dto, IUnitOfWork uow) =>
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
                    var entity = await uow.Teachers.GetByIdAsync(id);

                    if (entity is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Teacher not found",
                            detail: $"No Teacher found with ID {id}");
                    }

                    entity.FirstName    = dto.FirstName;
                    entity.LastName     = dto.LastName;
                    entity.NickName     = dto.NickName;
                    entity.Abbreviation = dto.Abbreviation;

                    await trans.CommitTransactionAsync();
                }

                return Results.NoContent();
            })
            .WithValidation<TeacherDto>()
            .WithName("UpdateTeacher")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);


        routeAdmin.MapPost("", async (TeacherDto dto, IUnitOfWork uow) =>
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

                    await uow.Teachers.AddAsync(entity);

                    await trans.CommitTransactionAsync();

                    int id = entity.Id;

                    return Results.Created($"{baseRoute}/{id}", ToDto(await uow.Teachers.GetByIdAsync(id)));
                }
            })
            .WithValidation<TeacherDto>()
            .WithName("AddTeacher")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var entity = await uow.Teachers.GetByIdAsync(id);

                if (entity is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Teacher not found",
                        detail: $"No Teacher found with ID {id}");
                }

                uow.Teachers.Remove(entity);

                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteTeacher")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}