namespace WebAPI.Endpoints;

using Base.Persistence.Contracts;

using Service;

using Shared.Exceptions;

using WebAPI.Filters;

public record ConfigDto(
    int currentSchoolYear
);

public static class ConfigEndpoints
{
    public static void MapConfigEndpoints(this IEndpointRouteBuilder app, string baseRoute)
    {
        var route = app
            .MapGroup(baseRoute)
            .WithTags("Config");
        // NO Auth required

        var routeAdmin = app
            .MapGroup(baseRoute)
            .WithTags("Config")
            .RequireAuthorization(Settings.AdminOrUserPolicyName)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        route.MapGet("", async (IConfigService configService, ITransactionProvider transactionProvider, ILoggerFactory loggerFactory) =>
            {
                // Create a config Dto
                return Results.Ok(new ConfigDto(await configService.GetCurrentSchoolYear()));
            })
            .WithName("GetConfig")
            .Produces<ConfigDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}