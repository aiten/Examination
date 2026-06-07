namespace WebAPI.Endpoints;

using Core.Contracts;

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

        route.MapPost("", async (ExamRegistrationDto dto, IUnitOfWork uow) =>
            {
                try
                {
                    using var trans = await uow.BeginTransactionAsync();
                    var registration = await uow.Exams.RegisterStudentAsync(dto.FirstName, dto.LastName, dto.LoginName, dto.Pin);
                   
                    await trans.CommitTransactionAsync();
                    
                    return Results.Created($"/api/exam/{registration.ExamId}",
                        new ExamRegistrationResultDto(
                            registration.Id,
                            registration.Student.LastName, 
                            registration.Student.FirstName,
                            registration.Exam.Pin??0,
                            registration.Exam.Description,
                            registration.Exam.Date,
                            registration.RegistrationCode));
                }
                catch (InvalidOperationException ex)
                {
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
