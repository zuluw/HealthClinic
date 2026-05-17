using InnoClinic.Appointments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;

namespace InnoClinic.Appointments.IntegrationTests.Shared
{
    public abstract class IntegrationTestBase : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
    {
        protected readonly HttpClient Client;
        protected readonly AppDbContext DbContext;
        private readonly IServiceScope _scope;
        private readonly ApiWebApplicationFactory _factory;
        private Respawner? _respawner;

        protected IntegrationTestBase(ApiWebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Client = factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            var conn = DbContext.Database.GetDbConnection();
            await conn.OpenAsync();

            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = ["__EFMigrationsHistory"]
            });

            await _respawner.ResetAsync(conn);
        }

        public Task DisposeAsync()
        {
            _scope.Dispose();
            return Task.CompletedTask;
        }
    }
}