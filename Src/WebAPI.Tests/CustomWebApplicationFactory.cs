using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace WebAPI.Tests;

using Base.Persistence.Contracts;

using Persistence;

using Service;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public IUnitOfWork            UnitOfWork            { get; } = Substitute.For<IUnitOfWork>();
    public IExamService           ExamService           { get; } = Substitute.For<IExamService>();
    public ITeacherService        TeacherService        { get; } = Substitute.For<ITeacherService>();
    public IClassService          ClassService          { get; } = Substitute.For<IClassService>();
    public IStudentService        StudentService        { get; } = Substitute.For<IStudentService>();
    public ISubtaskService        SubtaskService        { get; } = Substitute.For<ISubtaskService>();
    public IStudentExamService    StudentExamService    { get; } = Substitute.For<IStudentExamService>();
    public IStudentSubtaskService StudentSubtaskService { get; } = Substitute.For<IStudentSubtaskService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext, UnitOfWork, repositories, and all service implementations
            var persistenceAssembly = typeof(Persistence.ApplicationDbContext).Assembly;
            var descriptors = services
                .Where(d => d.ServiceType == typeof(IUnitOfWork) ||
                            d.ServiceType == typeof(ITransactionProvider) ||
                            d.ServiceType == typeof(IExamService) ||
                            d.ServiceType == typeof(ITeacherService) ||
                            d.ServiceType == typeof(IClassService) ||
                            d.ServiceType == typeof(IStudentService) ||
                            d.ServiceType == typeof(ISubtaskService) ||
                            d.ServiceType == typeof(IStudentExamService) ||
                            d.ServiceType == typeof(IStudentSubtaskService) ||
                            (d.ServiceType.FullName != null && d.ServiceType.FullName.Contains("DbContext")) ||
                            (d.ImplementationType != null && d.ImplementationType.Assembly == persistenceAssembly))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Register mock services
            services.AddScoped<IUnitOfWork>(_ => UnitOfWork);
            services.AddScoped<ITransactionProvider>(_ => UnitOfWork);
            services.AddScoped<IExamService>(_ => ExamService);
            services.AddScoped<ITeacherService>(_ => TeacherService);
            services.AddScoped<IClassService>(_ => ClassService);
            services.AddScoped<IStudentService>(_ => StudentService);
            services.AddScoped<ISubtaskService>(_ => SubtaskService);
            services.AddScoped<IStudentExamService>(_ => StudentExamService);
            services.AddScoped<IStudentSubtaskService>(_ => StudentSubtaskService);

            // Replace Keycloak authentication with a test handler that always succeeds
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme    = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            // Replace Keycloak-based authorization policies with pass-through versions
            services.PostConfigure<AuthorizationOptions>(options =>
            {
                var bypass = new AuthorizationPolicyBuilder("Test")
                    .RequireAssertion(_ => true)
                    .Build();
                options.AddPolicy(Settings.AdminPolicyName,       bypass);
                options.AddPolicy(Settings.UserPolicyName,        bypass);
                options.AddPolicy(Settings.AdminOrUserPolicyName, bypass);
            });
        });

        builder.UseEnvironment("Development");
    }
}
