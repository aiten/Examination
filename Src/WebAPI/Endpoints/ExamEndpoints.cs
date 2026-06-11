namespace WebAPI.Endpoints;

using Persistence;
using Persistence.Model;
using Persistence.QueryResult;

using WebAPI.Filters;

public record ExamDto(
    int      Id,
    string   Description,
    int      ExamType,
    int      TeacherId,
    int      CourseId,
    DateOnly Date,
    TimeOnly From,
    TimeOnly To,
    int?     Pin,
    bool     CanRegister    = true,
    bool     CanShowResults = false
);

public static class ExamEndpoints
{
    #region Dto-Entity Mapping

    private static Exam ToEntity(ExamDto dto)
    {
        return new Exam()
        {
            Id          = dto.Id,
            ExamType    = (ExamType)dto.ExamType,
            Description = dto.Description
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


        route.MapGet("", async (IUnitOfWork uow) =>
            {
                var dtos = ToDto(await uow.Exams.GetNoTrackingAsync(null, null, nameof(Exam.Teacher), nameof(Exam.Course)));
                return Results.Ok(dtos);
            })
            .WithName("GetExams")
            .Produces<List<ExamDto>>(StatusCodes.Status200OK);


        route.MapGet("overview", async (int? teacherId, int? courseId, IUnitOfWork uow) =>
            {
                var dtos = await uow.Exams.GetExamOverviewsAsync(teacherId, courseId);
                return Results.Ok(dtos);
            })
            .WithName("GetExamOverview")
            .Produces<List<ExamOverview>>(StatusCodes.Status200OK);

        route.MapGet("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var dto = ToDto(await uow.Exams.GetByIdAsync(id, nameof(Exam.Teacher), nameof(Exam.Course)));

                if (dto is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Exam not found",
                        detail: $"No Exam found with ID {id}");
                }

                return Results.Ok(dto);
            })
            .WithName("GetExam")
            .Produces<ExamDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        route.MapPut("/{id:int}", async (int id, ExamDto dto, IUnitOfWork uow) =>
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
                    var entity = await uow.Exams.GetByIdAsync(id);

                    if (entity is null)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "Exam not found",
                            detail: $"No Exam found with ID {id}");
                    }

                    entity.Description    = dto.Description;
                    entity.ExamType       = (ExamType)dto.ExamType;
                    entity.CourseId       = dto.CourseId;
                    entity.TeacherId      = dto.TeacherId;
                    entity.Pin            = dto.Pin;
                    entity.Date           = dto.Date;
                    entity.From           = dto.From;
                    entity.To             = dto.To;
                    entity.CanRegister    = dto.CanRegister;
                    entity.CanShowResults = dto.CanShowResults;
                    entity.Modified       = DateTime.Now;

                    await trans.CommitTransactionAsync();
                }

                return Results.NoContent();
            })
            .WithValidation<ExamDto>()
            .WithName("UpdateExam")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);


        route.MapPost("", async (ExamDto dto, IUnitOfWork uow) =>
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
                    var entity = ToEntity(dto);

                    entity.Created        = DateTime.Now;
                    entity.Modified       = null;
                    entity.ExamType       = (ExamType)dto.ExamType;
                    entity.CourseId       = dto.CourseId;
                    entity.TeacherId      = dto.TeacherId;
                    entity.Pin            = dto.Pin;
                    entity.Date           = dto.Date;
                    entity.From           = dto.From;
                    entity.To             = dto.To;
                    entity.CanRegister    = dto.CanRegister;
                    entity.CanShowResults = dto.CanShowResults;

                    await uow.Exams.AddAsync(entity);

                    await trans.CommitTransactionAsync();

                    int id = entity.Id;

                    return Results.Created($"{baseRoute}/{id}", await uow.Exams.GetByIdAsync(id));
                }
            })
            .WithValidation<ExamDto>()
            .WithName("AddExam")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        route.MapDelete("/{id:int}", async (int id, IUnitOfWork uow) =>
            {
                var entity = await uow.Exams.GetByIdAsync(id);

                if (entity is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Exam not found",
                        detail: $"No Exam found with ID {id}");
                }

                uow.Exams.Remove(entity);

                await uow.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteExam")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);
    }
}