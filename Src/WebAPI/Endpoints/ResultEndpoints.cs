namespace WebAPI.Endpoints;

using Persistence;
using Persistence.QueryResult;

using Service;

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

        route.MapPost("", async (StudentExamResultQueryDto dto, IStudentExamService studentExamService, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger(nameof(ResultEndpoints));
                try
                {
                    var result = await studentExamService.GetStudentResultAsync(
                        dto.FirstName, dto.LastName, dto.Pin, dto.RegistrationCode);

                    logger.LogInformation("QueryResults success: '{LastName}, {FirstName}' Pin={Pin} Exam={RegistrationCode}",
                        dto.LastName, dto.FirstName, dto.Pin, dto.RegistrationCode);

                    return Results.Ok(result);
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogWarning("QueryResults failed: '{LastName}, {FirstName}' Pin={Pin} Exam={RegistrationCode} Error={Error}",
                        dto.LastName, dto.FirstName, dto.Pin, dto.RegistrationCode, ex.Message);

                    throw;
                }
            })
            .WithValidation<StudentExamResultQueryDto>()
            .WithName("GetExamResult")
            .Produces<StudentExamResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}