namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

using WebAPI.Filters;

public record SubjectDto(
    int     Id,
    string  Name,
    string? Comment
);

public static class SubjectEndpoints
{
    #region Dto-Entity Mapping

    private static Subject ToEntity(SubjectDto dto)
    {
        return new Subject()
        {
            Id      = dto.Id,
            Name    = dto.Name,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment
        };
    }

    private static SubjectDto? ToDto(Subject? entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new SubjectDto(entity.Id, entity.Name, entity.Comment);
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
            .RequireAuthorization(Settings.AdminPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        route.MapGet("", async (ISubjectService subjectService, ITransactionProvider transactionProvider) =>
            {
                var dtos = ToDto(await subjectService.GetSubjectsAsync());
                return Results.Ok(dtos);
            })
            .WithName("GetSubjects")
            .Produces<List<SubjectDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, ISubjectService subjectService, ITransactionProvider transactionProvider) =>
            {
                var dto = ToDto(await subjectService.SingleSubjectAsync(id));
                return Results.Ok(dto);
            })
            .WithName("GetSubject")
            .Produces<SubjectDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPut("/{id:int}", async (int id, SubjectDto dto, ISubjectService subjectService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();

                await subjectService.UpdateSubjectAsync(id, ToEntity(dto));
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("UpdateSubject")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPost("", async (SubjectDto dto, ISubjectService subjectService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckIdMustBe0(dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();

                var entity = ToEntity(dto);

                await subjectService.AddSubjectAsync(entity);
                await trans.CommitTransactionAsync();

                int id = entity.Id;

                return Results.Created($"{baseRoute}/{id}", ToDto(await subjectService.GetSubjectByIdAsync(id)));
            })
            .WithName("AddSubject")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, ISubjectService subjectService, ITransactionProvider transactionProvider) =>
            {
                using var trans  = await transactionProvider.BeginTransactionAsync();

                await subjectService.DeleteSubjectAsync(id);
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("DeleteSubject")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}