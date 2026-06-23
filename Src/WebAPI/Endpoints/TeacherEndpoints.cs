namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

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

        route.MapGet("", async (ITeacherService teacherService, ITransactionProvider transactionProvider) =>
            {
                var dtos = ToDto(await teacherService.GetTeachersAsync());
                return Results.Ok(dtos);
            })
            .WithName("GetTeachers")
            .Produces<List<TeacherDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, ITeacherService teacherService, ITransactionProvider transactionProvider) =>
            {
                var dto = ToDto(await teacherService.SingleTeacherAsync(id));
                return Results.Ok(dto);
            })
            .WithName("GetTeacher")
            .Produces<TeacherDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPut("/{id:int}", async (int id, TeacherDto dto, ITeacherService teacherService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                using var trans  = await transactionProvider.BeginTransactionAsync();

                var entity = ToEntity(dto);

                await teacherService.UpdateTeacherAsync(id, entity);
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithValidation<TeacherDto>()
            .WithName("UpdateTeacher")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);


        routeAdmin.MapPost("", async (TeacherDto dto, ITeacherService teacherService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckIdMustBe0(dto.Id);

                using var trans  = await transactionProvider.BeginTransactionAsync();
                var       entity = ToEntity(dto);

                await teacherService.AddTeacherAsync(entity);

                await trans.CommitTransactionAsync();

                int id = entity.Id;

                return Results.Created($"{baseRoute}/{id}", ToDto(await teacherService.GetTeacherByIdAsync(id)));
            })
            .WithValidation<TeacherDto>()
            .WithName("AddTeacher")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, ITeacherService teacherService, ITransactionProvider transactionProvider) =>
            {
                using var trans  = await transactionProvider.BeginTransactionAsync();

                await teacherService.DeleteTeacherAsync(id);

                await trans.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteTeacher")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}