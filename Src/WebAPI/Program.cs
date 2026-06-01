using Base.Core;

using Core.Contracts;

using FluentValidation;

using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

using Persistence;

using WebAPI;
using WebAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
builder.Services.AddCors();

builder.Services
    .AddAuthorization()
    .AddKeycloakAuthorization(builder.Configuration)
    .AddAuthorizationBuilder()
    .AddPolicy(Settings.AdminPolicyName, policyBuilder =>
    {
        var keycloakOptions = builder.Configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>()!;

        policyBuilder
            // .RequireRealmRoles(Settings.KeycloakAdminRoleName)                                         // Realm role is fetched from token
            // .RequireResourceRolesForClient(keycloakOptions.Resource, [Settings.KeycloakAdminRoleName]) // Require Resource Roles (for this Client)
            .RequireResourceRoles(Settings.KeycloakAdminRoleName); // Resource/Client role is fetched from token (any client)
    })
    .AddPolicy(Settings.UserPolicyName, policyBuilder =>
    {
        var keycloakOptions = builder.Configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>()!;
        policyBuilder
            // .RequireRealmRoles(Settings.KeycloakUserRoleName)                                         // Realm role is fetched from token
            // .RequireResourceRolesForClient(keycloakOptions.Resource, [Settings.KeycloakUserRoleName]) // Require Resource Roles (for this Client)
            .RequireResourceRoles(Settings.KeycloakUserRoleName); // Resource/Client role is fetched from token (any client)
    })
    .AddPolicy(Settings.AdminOrUserPolicyName, policyBuilder =>
    {
        policyBuilder
            .RequireResourceRoles(Settings.KeycloakUserRoleName, Settings.KeycloakAdminRoleName);
    });

    builder.Services.AddHttpContextAccessor();
var kcOptions       = builder.Configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>()!;
var keycloakBaseUrl = $"{kcOptions.AuthServerUrl!.TrimEnd('/')}/realms/{kcOptions.Realm}/protocol/openid-connect";

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new OpenApiInfo { Title = "Examination", Version = "v1" };

        document.Components                 ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["oidc"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{keycloakBaseUrl}/auth"),
                    TokenUrl         = new Uri($"{keycloakBaseUrl}/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID" },
                        { "profile", "Profile" },
                    }
                }
            }
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference("oidc", document), [] }
        });

        var httpContext = context.ApplicationServices.GetRequiredService<IHttpContextAccessor>().HttpContext;
        if (httpContext?.Request.Host.Value?.Contains(".cloud.htl-leonding.ac.at") == true)
        {
            var fullHostname = System.Net.Dns.GetHostEntry("").HostName;
            var hostname     = fullHostname.Split('-')[0];
            document.Servers =
            [
                new OpenApiServer { Url = $"{httpContext.Request.Scheme}s://{httpContext.Request.Host.Value}/{hostname}" }
            ];
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddProblemDetails();

builder.Services.AddCors(options => { options.AddDefaultPolicy(policy => { policy.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod(); }); });


builder.Services
    .AddScoped<IUnitOfWork, UnitOfWork>()
    .AddAssemblyIncludingInternals(name => name.EndsWith("Repository"), ServiceLifetime.Transient, typeof(ApplicationDbContext).Assembly);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddValidatorsFromAssemblyContaining<WebAPI.Program>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (true) // app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.AddAuthorizationCodeFlow("oidc", flow => flow
            .WithClientId(kcOptions.Resource)
        );
    });
}

// Add CORS to support Single Page Apps (SPAs)
app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapGet("/api/ping", () => "pong")
    .WithName("Ping")
    .WithTags("Health");
//.RequireAuthorization(Settings.AdminPolicyName);

app.MapTeacherEndpoints("/api/teacher");
app.MapClassEndpoints("/api/class");
app.MapSubjectEndpoints("/api/subject");
app.MapCourseEndpoints("/api/course");
app.MapExamEndpoints("/api/exam");
app.MapSubtaskEndpoints("/api/exam");
app.MapStudentExamEndpoints("/api/exam");
app.MapStudentSubtaskEndpoints("/api/exam");
app.MapStudentEndpoints("/api/student");
app.MapRegistrationEndpoints("/api/registration");

app.MapFallbackToFile("index.html");

app.Run();

namespace WebAPI
{
    public partial class Program
    {
    }
}