using InnoClinic.Appointments.Infrastructure.Data;
using InnoClinic.Appointments.Api.Controllers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace InnoClinic.Appointments.IntegrationTests.Shared
{
    public class ApiWebApplicationFactory : WebApplicationFactory<AppointmentsController>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:latest").Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString()
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.Remove(services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)));
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));

                services.AddMassTransitTestHarness();
            });
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
        }
    }
}