using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace WebAPI.Tests;

using Persistence;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext, UnitOfWork, and all repository registrations
            // (repositories still need ApplicationDbContext which is removed)
            var persistenceAssembly = typeof(Persistence.ApplicationDbContext).Assembly;
            var descriptors = services
                .Where(d => d.ServiceType == typeof(IUnitOfWork) ||
                            (d.ServiceType.FullName != null && d.ServiceType.FullName.Contains("DbContext")) ||
                            (d.ImplementationType != null && d.ImplementationType.Assembly == persistenceAssembly))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Register the mock UnitOfWork
            services.AddScoped<IUnitOfWork>(_ => UnitOfWork);
        });

        builder.UseEnvironment("Development");
    }
}