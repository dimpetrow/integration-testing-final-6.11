using Customers.Api.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Testcontainers.PostgreSql;
using Xunit;

namespace Customers.Api.Tests.Integration;

public class CustomerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    public const string ValidGithubUser = "validuser";
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("db")
        .WithUsername("course")
        .WithPassword("whatever")
        .Build();
        
    private readonly GitHubApiServer _gitHubApiServer = new ();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IDbConnectionFactory));
            var postgreDbConnectionString = _dbContainer.GetConnectionString();
            services.AddSingleton<IDbConnectionFactory>(_ =>
                new NpgsqlConnectionFactory(postgreDbConnectionString));
            
            services.AddHttpClient("GitHub", httpClient =>
            {
                httpClient.BaseAddress = new Uri(_gitHubApiServer.Url);
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Accept, "application/vnd.github.v3+json");
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.UserAgent, $"Course-{Environment.MachineName}");
            });
        });
    }

    public async Task InitializeAsync()
    {
        _gitHubApiServer.Start();
        _gitHubApiServer.SetupUser(ValidGithubUser);
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        _gitHubApiServer.Dispose();
    }
}

// private readonly TestcontainersContainer _dbContainer =
//     new TestcontainersBuilder<TestcontainersContainer>()
//         .WithImage("postgres:latest")
//         .WithEnvironment("POSTGRES_USER", "course")
//         .WithEnvironment("POSTGRES_PASSWORD", "changeme")
//         .WithEnvironment("POSTGRES_DB", "mydb")
//         .WithPortBinding(5555, 5432)
//         .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
//         .Build();
