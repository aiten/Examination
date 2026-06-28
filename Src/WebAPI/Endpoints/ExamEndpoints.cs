namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Persistence;
using Persistence.Model;
using Persistence.QueryResult;

using Service;

using Shared.Exceptions;

using WebAPI.Filters;

public record ExamDto(
    int       Id,
    string    Description,
    int       ExamType,
    int       TeacherId,
    int       CourseId,
    DateOnly? Date,
    TimeOnly? From,
    TimeOnly? To,
    string?   Pin,
    bool      CanRegister,
    bool      CanShowResults
);

public static class ExamEndpoints
{
    #region Dto-Entity Mapping

    private static Exam ToEntity(ExamDto dto)
    {
        return new Exam()
        {
            Id             = dto.Id,
            ExamType       = (ExamType)dto.ExamType,
            Description    = dto.Description,
            CourseId       = dto.CourseId,
            TeacherId      = dto.TeacherId,
            Pin            = dto.Pin,
            Date           = dto.Date,
            From           = dto.From,
            To             = dto.To,
            CanRegister    = dto.CanRegister,
            CanShowResults = dto.CanShowResults
        };
    }

    private static ExamDto? ToDto(Exam? entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new ExamDto(
            entity.Id,
            entity.Description,
            (int)entity.ExamType,
            entity.TeacherId,
            entity.CourseId,
            entity.Date,
            entity.From,
            entity.To,
            entity.Pin,
            entity.CanRegister,
            entity.CanShowResults
        );
    }

    private static IList<ExamDto>? ToDto(IList<Exam>? list)
    {
        if (list is null)
        {
            return null;
        }

        return list.Select(x => ToDto(x)!).ToList();
    }

    #endregion

    public static void MapExamEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Exam")
            .RequireAuthorization(Settings.UserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);


        route.MapGet("", async (IExamService examService) =>
            {
                var dtos = ToDto(await examService.GetExamsAsync());
                return Results.Ok(dtos);
            })
            .WithName("GetExams")
            .Produces<List<ExamDto>>(StatusCodes.Status200OK);


        route.MapGet("overview", async (int? teacherId, int? courseId, int? courseYear, IExamService examService) =>
            {
                var dtos = await examService.GetExamOverviewsAsync(teacherId, courseId, courseYear);
                return Results.Ok(dtos);
            })
            .WithName("GetExamOverview")
            .Produces<List<ExamOverview>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, IExamService examService) =>
            {
                var dto = ToDto(await examService.SingleExamAsync(id, nameof(Exam.Teacher), nameof(Exam.Course)));
                return Results.Ok(dto);
            })
            .WithName("GetExam")
            .Produces<ExamDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPut("/{id:int}", async (int id, ExamDto dto, IExamService examService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckId(id, dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();

                await examService.UpdateExamAsync(id, ToEntity(dto));
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithValidation<ExamDto>()
            .WithName("UpdateExam")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);


        route.MapPost("", async (ExamDto dto, IExamService examService, ITransactionProvider transactionProvider) =>
            {
                EndpointTools.CheckIdMustBe0(dto.Id);

                using var trans = await transactionProvider.BeginTransactionAsync();

                var entity  = ToEntity(dto);
                var created = await examService.AddExamAsync(entity);

                await trans.CommitTransactionAsync();

                return Results.Created($"{baseRoute}/{created.Id}", ToDto(created));
            })
            .WithValidation<ExamDto>()
            .WithName("AddExam")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        route.MapDelete("/{id:int}", async (int id, IExamService examService, ITransactionProvider transactionProvider) =>
            {
                using var trans = await transactionProvider.BeginTransactionAsync();

                await examService.DeleteExamAsync(id);
                await trans.CommitTransactionAsync();

                return Results.NoContent();
            })
            .WithName("DeleteExam")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}