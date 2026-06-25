namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Service;

using Shared.Exceptions;

using WebAPI.Filters;

public record RegistrationExamDto(string FirstName, string LastName, string? LoginName, string Pin);
public record RegistrationCourseDto(string FirstName, string LastName, string Pin);

public record RegistrationExamResultDto(int Id, string LastName, string FirstName, string? Pin, string ExamDescription, DateOnly ExamDate, string RegistrationCode);
public record RegistrationCourseResultDto(int Id, string LastName, string FirstName, string? Pin, string CourseDescription, string RegistrationCode);

public static class RegistrationEndpoints
{
    public static void MapRegistrationEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Registration");
        // NO Auth required .RequireAuthorization(Settings.AdminPolicyName);

        route.MapPost("exam", async (RegistrationExamDto dto, IExamService examService, ITransactionProvider transactionProvider, ILoggerFactory loggerFactory) =>
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
                        new RegistrationExamResultDto(
                            registration.Id,
                            registration.Student.LastName,
                            registration.Student.FirstName,
                            registration.Exam.Pin,
                            registration.Exam.Description,
                            registration.Exam.Date,
                            registration.RegistrationCode));
                }
                catch (IllegalValuesException ex)
                {
                    logger.LogWarning("Exam registration failed: '{LastName}, {FirstName}' Pin={Pin} Error={Error}",
                        dto.LastName, dto.FirstName, dto.Pin, ex.Message);

                    throw;
                }
            })
            .WithValidation<RegistrationExamDto>()
            .WithName("RegisterForExam")
            .Produces<RegistrationExamResultDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        route.MapPost("course", async (RegistrationExamDto dto, ICourseService courseService, ITransactionProvider transactionProvider, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(RegistrationEndpoints));
            try
            {
                using var trans = await transactionProvider.BeginTransactionAsync();
                throw new NotImplementedException();
/*
                var registration = await courseService.RegisterStudentAsync(dto.FirstName, dto.LastName, dto.Pin);

                await trans.CommitTransactionAsync();

                logger.LogInformation("Registration success: '{LastName}, {FirstName}' Exam={ExamDescription}",
                    registration.Student.LastName,
                    registration.Student.FirstName,
                    registration.Exam.Description);

                return Results.Created($"/api/course/{registration.CourseId}",
                    new RegistrationExamResultDto(
                        registration.Id,
                        registration.Student.LastName,
                        registration.Student.FirstName,
                        registration.Exam.Pin,
                        registration.Exam.Description,
                        registration.Exam.Date,
                        registration.RegistrationCode));
*/
            }
            catch (IllegalValuesException ex)
            {
                logger.LogWarning("Course registration failed: '{LastName}, {FirstName}' Pin={Pin} Error={Error}",
                    dto.LastName, dto.FirstName, dto.Pin, ex.Message);

                throw;
            }

        })
        .WithValidation<RegistrationExamDto>()
        .WithName("RegisterForCourse")
        .Produces<RegistrationExamResultDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}