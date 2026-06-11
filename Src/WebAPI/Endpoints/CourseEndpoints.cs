namespace WebAPI.Endpoints;

using Persistence;
using Persistence.Model;

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
            .RequireAuthorization(Settings.AdminPolicyName);

        route.MapGet("", async (IUnitOfWork uow) =>
            {
                var entities = await uow.Courses.GetNoTrackingAsync(
                    null, null, nameof(Course.Classes), nameof(Course.Teachers));
                var dtos = ToDto(entities);
                return Results.Ok(dtos);
            })
            .WithName("GetCourses")
            .Produces<List<CourseDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var dto = ToDto(await uow.Courses.GetByIdAsync(
                    id, nameof(Course.Classes), nameof(Course.Teachers)));

                if (dto is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Course not found",
                        detail: $"No Course found with ID {id}");
                }

                return Results.Ok(dto);
            })
            .WithName("GetCourse")
            .Produces<CourseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPut("/{id:int}", async (int id, CourseDto dto, IUnitOfWork uow) =>
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
                    var entity = await uow.Courses.GetByIdAsync(
                        id, nameof(Course.Classes), nameof(Course.Teachers));

                    if (entity is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Course not found",
                            detail: $"No Course found with ID {id}");
                    }

                    entity.Name      = dto.Name;
                    entity.Year      = dto.Year;
                    entity.SubjectId = dto.SubjectId;

                    var classes  = await uow.Classes.GetAsync(c => dto.ClassIds.Contains(c.Id));
                    var teachers = await uow.Teachers.GetAsync(t => dto.TeacherIds.Contains(t.Id));

                    entity.Classes  = classes.ToList();
                    entity.Teachers = teachers.ToList();

                    await trans.CommitTransactionAsync();
                }

                return Results.NoContent();
            })
            .WithName("UpdateCourse")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPost("", async (CourseDto dto, IUnitOfWork uow) =>
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
                    var classes  = await uow.Classes.GetAsync(c => dto.ClassIds.Contains(c.Id));
                    var teachers = await uow.Teachers.GetAsync(t => dto.TeacherIds.Contains(t.Id));

                    var entity = new Course
                    {
                        Name      = dto.Name,
                        Year      = dto.Year,
                        SubjectId = dto.SubjectId,
                        Classes   = classes.ToList(),
                        Teachers  = teachers.ToList()
                    };

                    await uow.Courses.AddAsync(entity);

                    await trans.CommitTransactionAsync();

                    int newId = entity.Id;

                    return Results.Created($"{baseRoute}/{newId}",
                        ToDto(await uow.Courses.GetByIdAsync(newId, nameof(Course.Classes), nameof(Course.Teachers))));
                }
            })
            .WithName("AddCourse")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var entity = await uow.Courses.GetByIdAsync(id);

                if (entity is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Course not found",
                        detail: $"No Course found with ID {id}");
                }

                uow.Courses.Remove(entity);

                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteCourse")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}