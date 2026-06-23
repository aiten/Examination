namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

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
            .RequireAuthorization(Settings.AdminPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);  

        route.MapGet("", async (IClassService classService, ITransactionProvider transactionProvider) =>
            {
                var dtos = ToDto(await classService.GetClassesAsync());
                return Results.Ok(dtos);
            })
            .WithName("GetClasses")
            .Produces<List<ClassDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, IClassService classService, ITransactionProvider transactionProvider) =>
            {
                var dto = ToDto(await classService.SingleClassAsync(id));
                return Results.Ok(dto);
            })
            .WithName("GetClass")
            .Produces<ClassDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPut("/{id:int}", async (int id, ClassDto dto, IClassService classService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                using var trans  = await transactionProvider.BeginTransactionAsync();
                var       entity = await classService.SingleClassAsync(id);

                await classService.UpdateClassAsync(id, ToEntity(dto));
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithValidation<ClassDto>()
            .WithName("UpdateClass")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);


        routeAdmin.MapPost("", async (ClassDto dto, IClassService classService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckIdMustBe0(dto.Id);

                using var trans  = await transactionProvider.BeginTransactionAsync();
                var       entity = ToEntity(dto);

                await classService.AddClassAsync(entity);
                await trans.CommitTransactionAsync();

                int id = entity.Id;

                return Results.Created($"{baseRoute}/{id}", ToDto(await classService.GetClassByIdAsync(id)));
            })
            .WithValidation<ClassDto>()
            .WithName("AddClass")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, IClassService classService, ITransactionProvider transactionProvider) =>
            {
                using var trans = await transactionProvider.BeginTransactionAsync();

                await classService.DeleteClassAsync(id);
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("DeleteClass")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}