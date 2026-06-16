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
    public IUnitOfWork  UnitOfWork  { get; } = Substitute.For<IUnitOfWork>();
    public IExamService ExamService { get; } = Substitute.For<IExamService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext, UnitOfWork, and all repository registrations
            // (repositories still need ApplicationDbContext which is removed)
            var persistenceAssembly = typeof(Persistence.ApplicationDbContext).Assembly;
            var descriptors = services
                .Where(d => d.ServiceType == typeof(IUnitOfWork) ||
                            d.ServiceType == typeof(ITransactionProvider) ||
                            d.ServiceType == typeof(IExamService) ||
                            (d.ServiceType.FullName != null && d.ServiceType.FullName.Contains("DbContext")) ||
                            (d.ImplementationType != null && d.ImplementationType.Assembly == persistenceAssembly))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Register the mock UnitOfWork, TransactionProvider and ExamService
            services.AddScoped<IUnitOfWork>(_ => UnitOfWork);
            services.AddScoped<ITransactionProvider>(_ => UnitOfWork);
            services.AddScoped<IExamService>(_ => ExamService);
        });

        builder.UseEnvironment("Development");
    }
}