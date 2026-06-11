namespace WebAPI.Endpoints;

using Persistence;
using Persistence.Model;

using WebAPI.Filters;

public record StudentDto(
    int    Id,
    string FirstName,
    string LastName,
    int[]  ClassIds
);

// Each entry: "Firstname;Lastname;ClassName(Year),ClassName2(Year2)" e.g. "Max;Mustermann;1ahif(2024),2ahif(2025)"
public record ImportStudentsDto(string[] Students);

public static class StudentEndpoints
{
    #region Dto-Entity Mapping

    private static StudentDto? ToDto(Student? entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new StudentDto(
            entity.Id,
            entity.FirstName,
            entity.LastName,
            entity.Classes.Select(c => c.Id).ToArray()
        );
    }

    private static IList<StudentDto>? ToDto(IList<Student>? list)
    {
        if (list is null)
        {
            return null;
        }

        return list.Select(x => ToDto(x)!).ToList();
    }

    #endregion

    public static void MapStudentEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Student")
            .RequireAuthorization(Settings.AdminOrUserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


        var routeAdmin = app
            .MapGroup(baseRoute)
            .WithTags("Student")
            .RequireAuthorization(Settings.AdminPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        route.MapGet("", async (IUnitOfWork uow) =>
            {
                var students = await uow.Students.GetNoTrackingAsync(includeProperties: "Classes");
                return Results.Ok(ToDto(students));
            })
            .WithName("GetStudents")
            .Produces<List<StudentDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var dto = ToDto(await uow.Students.GetByIdAsync(id, "Classes"));

                if (dto is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Student not found",
                        detail: $"No Student found with ID {id}");
                }

                return Results.Ok(dto);
            })
            .WithName("GetStudent")
            .Produces<StudentDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPost("", async (StudentDto dto, IUnitOfWork uow) =>
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
                    var classes = await uow.Classes.GetAsync(c => dto.ClassIds.Contains(c.Id));

                    var entity = new Student
                    {
                        FirstName = dto.FirstName,
                        LastName  = dto.LastName,
                        Classes   = classes
                    };

                    await uow.Students.AddAsync(entity);
                    await trans.CommitTransactionAsync();

                    int id = entity.Id;

                    return Results.Created($"{baseRoute}/{id}", ToDto(await uow.Students.GetByIdAsync(id, "Classes")));
                }
            })
            .WithValidation<StudentDto>()
            .WithName("AddStudent")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapPut("/{id:int}", async (int id, StudentDto dto, IUnitOfWork uow) =>
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
                    var entity = await uow.Students.GetByIdAsync(id, "Classes");

                    if (entity is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Student not found",
                            detail: $"No Student found with ID {id}");
                    }

                    entity.FirstName = dto.FirstName;
                    entity.LastName  = dto.LastName;

                    var newClasses = await uow.Classes.GetAsync(c => dto.ClassIds.Contains(c.Id));
                    entity.Classes.Clear();
                    foreach (var cls in newClasses)
                    {
                        entity.Classes.Add(cls);
                    }

                    await trans.CommitTransactionAsync();

                    return Results.NoContent();
                }
            })
            .WithValidation<StudentDto>()
            .WithName("UpdateStudent")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPost("/import", async (ImportStudentsDto dto, IUnitOfWork uow) =>
            {
                using (var trans = await uow.BeginTransactionAsync())
                {
                    await uow.Students.ImportStudentsAsync(dto.Students);
                    await trans.CommitTransactionAsync();
                }

                return Results.NoContent();
            })
            .WithName("ImportStudents")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var entity = await uow.Students.GetByIdAsync(id);

                if (entity is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Student not found",
                        detail: $"No Student found with ID {id}");
                }

                uow.Students.Remove(entity);
                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteStudent")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}