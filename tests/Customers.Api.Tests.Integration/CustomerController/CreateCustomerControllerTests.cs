using System.Net;
using System.Net.Http.Json;
using Customers.Api.Contracts.Responses;
using Customers.Api.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController;

[Collection(nameof(SharedDatabaseCollectionFixture))]
public class CreateCustomerControllerTests : IAsyncLifetime
{
    private readonly CustomerApiFactory _apiFactory;
    private readonly HttpClient _httpClient;

    public CreateCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _httpClient = apiFactory.HttpClient;
    }

    [Fact]
    public async Task Create_CreatesUser_WhenDataIsValid()
    {
        // Arrange
        var customer = CreateCustomerUtils.CustomerGenerator.Generate();

        // Act
        var response = await _httpClient.PostAsJsonAsync("customers", customer);

        // Assert
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(customer);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should()
            .Be($"http://localhost/customers/{customerResponse!.Id}");
    }

    [Fact]
    public async Task Create_ReturnsValidationError_WhenEmailIsInvalid()
    {
        // Arrange
        const string invalidEmail = "dasdja9d3j";
        var customer = CreateCustomerUtils.CustomerGenerator.Clone()
            .RuleFor(x => x.Email, invalidEmail).Generate();

        // Act
        var response = await _httpClient.PostAsJsonAsync("customers", customer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error.Title.Should().Be("One or more validation errors occurred.");
        error.Errors["Email"][0].Should().Be($"{invalidEmail} is not a valid email address");
    }
    
    [Fact]
    public async Task Create_ReturnsValidationError_WhenGitHubUserDoestNotExist()
    {
        // Arrange
        const string invalidGitHubUser = "dasdja9d3j";
        var customer = CreateCustomerUtils.CustomerGenerator.Clone()
            .RuleFor(x => x.GitHubUsername, invalidGitHubUser).Generate();

        // Act
        var response = await _httpClient.PostAsJsonAsync("customers", customer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error.Title.Should().Be("One or more validation errors occurred.");
        error.Errors["GitHubUsername"][0].Should().Be($"There is no GitHub user with username {invalidGitHubUser}");
    }

    public async Task DisposeAsync()
    {
        await _apiFactory.ResetDatabase();
    }

    public Task InitializeAsync() => Task.CompletedTask;
}
