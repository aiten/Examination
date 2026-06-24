namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

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

        route.MapGet("", async (IStudentService studentService, ITransactionProvider transactionProvider) =>
            {
                var students = await studentService.GetStudentsAsync(nameof(Student.Classes));
                return Results.Ok(ToDto(students));
            })
            .WithName("GetStudents")
            .Produces<List<StudentDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, IStudentService studentService, ITransactionProvider transactionProvider) =>
            {
                var dto = ToDto(await studentService.SingleStudentAsync(id, nameof(Student.Classes)));
                return Results.Ok(dto);
            })
            .WithName("GetStudent")
            .Produces<StudentDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPost("", async (StudentDto dto, IStudentService studentService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckIdMustBe0(dto.Id);

                using var trans   = await transactionProvider.BeginTransactionAsync();

                var entity = await studentService.AddStudentAsync(dto.FirstName, dto.LastName, dto.ClassIds);
                await trans.CommitTransactionAsync();

                int id = entity.Id;

                return Results.Created($"{baseRoute}/{id}", ToDto(entity));
            })
            .WithValidation<StudentDto>()
            .WithName("AddStudent")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapPut("/{id:int}", async (int id, StudentDto dto, IStudentService studentService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                using var trans  = await transactionProvider.BeginTransactionAsync();

                await studentService.UpdateStudentAsync(id, dto.FirstName, dto.LastName, dto.ClassIds);
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithValidation<StudentDto>()
            .WithName("UpdateStudent")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPost("/import", async (ImportStudentsDto dto, IStudentService studentService, ITransactionProvider transactionProvider) =>
            {
                using var trans = await transactionProvider.BeginTransactionAsync();
                await studentService.ImportStudentsAsync(dto.Students);
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("ImportStudents")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapDelete("/{id:int}", async (int id, IStudentService studentService, ITransactionProvider transactionProvider) =>
            {
                using var trans = await transactionProvider.BeginTransactionAsync();

                await studentService.DeleteStudentAsync(id);
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("DeleteStudent")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}