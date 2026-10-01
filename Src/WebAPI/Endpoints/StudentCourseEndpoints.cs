namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;

using Service;

using Shared.Exceptions;

using WebAPI.Filters;

public record StudentCourseDto(
    int     Id,
    int     CourseId,
    int     StudentId,
    string  FirstName,
    string  LastName,
    string? RegistrationCode
);

public static class StudentCourseEndpoints
{
    #region Dto-Entity Mapping

    private static StudentCourseDto ToDto(StudentCourse entity) =>
        new(
            entity.Id,
            entity.CourseId,
            entity.StudentId,
            entity.Student.FirstName,
            entity.Student.LastName,
            entity.RegistrationCode);

    private static IList<StudentCourseDto> ToDto(IList<StudentCourse> list) =>
        list.Select(ToDto).ToList();

    #endregion

    private static void CheckBelongsToCourse(StudentCourse entity, int courseId)
    {
        if (entity.CourseId != courseId)
        {
            throw new NotFoundException($"StudentCourse {entity.Id} not found in course {courseId}");
        }
    }

    public static void MapStudentCourseEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app.MapGroup($"{baseRoute}/{{courseId:int}}/students")
            .WithTags("StudentCourse")
            .RequireAuthorization(Settings.AdminOrUserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        var routeAdmin = app.MapGroup($"{baseRoute}/{{courseId:int}}/students")
            .WithTags("StudentCourse")
            .RequireAuthorization(Settings.AdminPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        route.MapGet("", async (int courseId, IStudentCourseService studentCourseService, ITransactionProvider transactionProvider) =>
            {
                var list = await studentCourseService.GetByCourseAsync(courseId, nameof(StudentCourse.Student));
                return Results.Ok(ToDto(list));
            })
            .WithName("GetCourseStudents")
            .Produces<List<StudentCourseDto>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int courseId, int id, IStudentCourseService studentCourseService, ITransactionProvider transactionProvider) =>
            {
                var entity = await studentCourseService.SingleStudentCourseAsync(id, nameof(StudentCourse.Student));
                CheckBelongsToCourse(entity, courseId);

                return Results.Ok(ToDto(entity));
            })
            .WithName("GetCourseStudent")
            .Produces<StudentCourseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapPost("", async (int courseId, StudentCourseDto dto, IStudentCourseService studentCourseService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckIdMustBe0(dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();

                var entity = await studentCourseService.AddStudentCourseAsync(dto.StudentId, courseId, dto.RegistrationCode);

                await trans.CommitTransactionAsync();

                var created = await studentCourseService.GetStudentCourseByIdAsync(entity.Id, nameof(StudentCourse.Student));

                return Results.Created($"{baseRoute}/{courseId}/students/{entity.Id}", ToDto(created!));
            })
            .WithValidation<StudentCourseDto>()
            .WithName("AddCourseStudent")
            .Produces<StudentCourseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeAdmin.MapPut("/{id:int}", async (int courseId, int id, StudentCourseDto dto, IStudentCourseService studentCourseService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                var existing = await studentCourseService.SingleStudentCourseAsync(id);
                CheckBelongsToCourse(existing, courseId);

                using var trans = await transactionProvider.BeginTransactionAsync();

                await studentCourseService.UpdateStudentCourseAsync(id, dto.RegistrationCode);

                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithValidation<StudentCourseDto>()
            .WithName("UpdateCourseStudent")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeAdmin.MapDelete("/{id:int}", async (int courseId, int id, IStudentCourseService studentCourseService, ITransactionProvider transactionProvider) =>
            {
                var existing = await studentCourseService.SingleStudentCourseAsync(id);
                CheckBelongsToCourse(existing, courseId);

                using var trans = await transactionProvider.BeginTransactionAsync();

                await studentCourseService.DeleteStudentCourseAsync(id);

                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("DeleteCourseStudent")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status204NoContent);
    }
}
