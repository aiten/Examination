namespace WebAPI.Endpoints;

using Core.Contracts;
using Core.QueryResult;

using WebAPI.Filters;

public record StudentExamResultQueryDto(string FirstName, string LastName, int Pin, string RegistrationCode);

public static class ResultEndpoints
{
    public static void MapResultEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Results");
            // Anonymous — no RequireAuthorization

        route.MapPost("", async (StudentExamResultQueryDto dto, IUnitOfWork uow) =>
            {
                try
                {
                    var result = await uow.StudentExams.GetStudentResultAsync(
                        dto.FirstName, dto.LastName, dto.Pin, dto.RegistrationCode);

                    return Results.Ok(result);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Query failed",
                        detail: ex.Message);
                }
            })
            .WithValidation<StudentExamResultQueryDto>()
            .WithName("GetExamResult")
            .Produces<StudentExamResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
