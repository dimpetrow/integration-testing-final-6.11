using Customers.Api.Database;
using Customers.Api.Tests.Integration.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Npgsql;
using Respawn;
using System.Data.Common;
using System.Net;
using Testcontainers.PostgreSql;
using Xunit;

namespace Customers.Api.Tests.Integration;

public class CustomerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    public const string ValidGithubUser = "validuser";
    public const string InvalidGithubUser = "invaliduser";
    public const string RateLimitedGithubUser = "ratelimiteduser";
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("db")
        .WithUsername("course")
        .WithPassword("whatever")
        .Build();

    private readonly GitHubApiServer _gitHubApiServer = new();
    private HttpClient _httpClient = default!;
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    public HttpClient HttpClient => _httpClient;

    public async Task InitializeAsync()
    {
        _gitHubApiServer.Start();
        _gitHubApiServer.SetupUser(ValidGithubUser, HttpStatusCode.OK);
        _gitHubApiServer.SetupUser(InvalidGithubUser, HttpStatusCode.NotFound);
        _gitHubApiServer.SetupUser(RateLimitedGithubUser, HttpStatusCode.Forbidden);

        // Must be before CreateClient() as CreateClient() will intialize an API instance in memory, therefore call its ConfigureServices method, which requires DB to already be up & running
        await _dbContainer.StartAsync();

        // Must be before Respawner as Respawner will execute DB Initialization, which will create tables, therefore Respawner won't throw error that there are no tables in DB
        _httpClient = CreateClient();

        var postgreDbConnectionString = _dbContainer.GetConnectionString();
        _dbConnection = new NpgsqlConnection(postgreDbConnectionString);
        // Must be opened prior to giving it to Respawner
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            SchemasToInclude = new string[] { "public" },
            DbAdapter = DbAdapter.Postgres,
        });
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        _gitHubApiServer.Dispose();
    }
    public async Task ResetDatabase()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

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
}
