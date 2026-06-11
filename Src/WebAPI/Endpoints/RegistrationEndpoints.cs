namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Service;

using WebAPI.Filters;

public record ExamRegistrationDto(string FirstName, string LastName, string LoginName, int Pin);

public record ExamRegistrationResultDto(int Id, string LastName, string FirstName, int Pin, string ExamDescription, DateOnly ExamDate, string RegistrationCode);

public static class RegistrationEndpoints
{
    public static void MapRegistrationEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Registration");
        // NO Auth required .RequireAuthorization(Settings.AdminPolicyName);

        route.MapPost("", async (ExamRegistrationDto dto, IExamService examService, ITransactionProvider transactionProvider, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger(nameof(RegistrationEndpoints));
                try
                {
                    using var trans = await transactionProvider.BeginTransactionAsync();

                    var registration = await examService.RegisterStudentAsync(dto.FirstName, dto.LastName, dto.LoginName, dto.Pin);

                    await trans.CommitTransactionAsync();

                    logger.LogInformation("Registration success: '{LastName}, {FirstName}' Exam={ExamDescription}",
                        registration.Student.LastName,
                        registration.Student.FirstName,
                        registration.Exam.Description);

                    return Results.Created($"/api/exam/{registration.ExamId}",
                        new ExamRegistrationResultDto(
                            registration.Id,
                            registration.Student.LastName,
                            registration.Student.FirstName,
                            registration.Exam.Pin ?? 0,
                            registration.Exam.Description,
                            registration.Exam.Date,
                            registration.RegistrationCode));
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogWarning("Registration failed: '{LastName}, {FirstName}' Pin={Pin} Error={Error}",
                        dto.LastName, dto.FirstName, dto.Pin, ex.Message);

                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Registration failed",
                        detail: ex.Message);
                }
            })
            .WithValidation<ExamRegistrationDto>()
            .WithName("RegisterForExam")
            .Produces<ExamRegistrationResultDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}