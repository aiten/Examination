namespace WebAPI.Endpoints;

using Persistence;
using Persistence.QueryResult;

using Service;

using Shared.Exceptions;

using WebAPI.Filters;

public record StudentExamResultQueryDto(string FirstName, string LastName, string Pin, string RegistrationCode);

public record StudentCourseResultQueryDto(string FirstName, string LastName, string Pin, string RegistrationCode);

public static class ResultEndpoints
{
    public static void MapResultEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Results")
            .RequireRateLimiting("public-lookup");
        // Anonymous — no RequireAuthorization

        route.MapPost("exam", async (StudentExamResultQueryDto dto, IStudentExamService studentExamService, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger(nameof(ResultEndpoints));
                try
                {
                    var result = await studentExamService.GetStudentExamResultAsync(
                        dto.FirstName, dto.LastName, dto.Pin, dto.RegistrationCode);

                    logger.LogInformation("QueryResults success: '{LastName}, {FirstName}' Pin={Pin} Exam={RegistrationCode}",
                        dto.LastName, dto.FirstName, dto.Pin, dto.RegistrationCode);

                    return Results.Ok(result);
                }
                catch (NotFoundException ex)
                {
                    logger.LogWarning("QueryResults failed: '{LastName}, {FirstName}' Pin={Pin} Exam={RegistrationCode} Error={Error}",
                        dto.LastName, dto.FirstName, dto.Pin, dto.RegistrationCode, ex.Message);

                    throw;
                }
            })
            .WithName("GetExamResult")
            .Produces<StudentExamResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        route.MapPost("course", async (StudentCourseResultQueryDto dto, IStudentExamService studentExamService, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger(nameof(ResultEndpoints));
                try
                {
                    var result = await studentExamService.GetStudentCourseResultAsync(
                        dto.FirstName, dto.LastName, dto.Pin, dto.RegistrationCode);

                    logger.LogInformation("QueryResults success: '{LastName}, {FirstName}' Pin={Pin} Exam={RegistrationCode}",
                        dto.LastName, dto.FirstName, dto.Pin, dto.RegistrationCode);

                    return Results.Ok(result);
                }
                catch (NotFoundException ex)
                {
                    logger.LogWarning("QueryResults failed: '{LastName}, {FirstName}' Pin={Pin} Exam={RegistrationCode} Error={Error}",
                        dto.LastName, dto.FirstName, dto.Pin, dto.RegistrationCode, ex.Message);

                    throw;
                }
            })
            .WithName("GetCourseResult")
            .Produces<StudentCourseResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

    }
}