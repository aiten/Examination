namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

using WebAPI.Filters;

public record CourseDto(
    int        Id,
    string     Name,
    int        Year,
    int        SubjectId,
    IList<int> ClassIds,
    IList<int> TeacherIds
);

public static class CourseEndpoints
{
    #region Dto-Entity Mapping

    private static CourseDto? ToDto(Course? entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new CourseDto(
            entity.Id,
            entity.Name,
            entity.Year,
            entity.SubjectId,
            entity.Classes.Select(c => c.Id).ToList(),
            entity.Teachers.Select(t => t.Id).ToList()
        );
    }

    private static IList<CourseDto>? ToDto(IList<Course>? list)
    {
        if (list is null)
        {
            return null;
        }

        return list.Select(x => ToDto(x)!).ToList();
    }

    #endregion

    public static void MapCourseEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Course")
            .RequireAuthorization(Settings.AdminOrUserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        var routeAdmin = app
            .MapGroup(baseRoute)
            .WithTags("Course")
            .RequireAuthorization(Settings.AdminPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        route.MapGet("", async (ICourseService courseService, ITransactionProvider transactionProvider) =>
            {
                var entities = await courseService.GetCoursesAsync(nameof(Course.Classes), nameof(Course.Teachers));
                var dtos = ToDto(entities);
                return Results.Ok(dtos);
            })
            .WithName("GetCourses")
            .Produces<List<CourseDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, ICourseService courseService, ITransactionProvider transactionProvider) =>
            {
                var dto = ToDto(await courseService.SingleCourseAsync(id, nameof(Course.Classes), nameof(Course.Teachers)));
                return Results.Ok(dto);
            })
            .WithName("GetCourse")
            .Produces<CourseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPut("/{id:int}", async (int id, CourseDto dto, ICourseService courseService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id,dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();

                await courseService.UpdateCourseAsync(id, dto.Name, dto.Year, dto.SubjectId, dto.ClassIds, dto.TeacherIds);

                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("UpdateCourse")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPost("", async (CourseDto dto, ICourseService courseService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckIdMustBe0(dto.Id);

                using var trans    = await transactionProvider.BeginTransactionAsync();

                var entity = await courseService.AddCourseAsync(dto.Name, dto.Year, dto.SubjectId, dto.ClassIds, dto.TeacherIds);

                await trans.CommitTransactionAsync();

                return Results.Created($"{baseRoute}/{entity.Id}", ToDto(entity));
            })
            .WithName("AddCourse")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, ICourseService courseService, ITransactionProvider transactionProvider) =>
            {
                using var trans  = await transactionProvider.BeginTransactionAsync();

                await courseService.DeleteCourseAsync(id);
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("DeleteCourse")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}