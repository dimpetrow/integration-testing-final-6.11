using Customers.Api.Contracts.Responses;
using Customers.Api.Tests.Integration.Common;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using FluentAssertions;

namespace Customers.Api.Tests.Integration.CustomerController
{
    [Collection(nameof(SharedDatabaseCollectionFixture))]
    public class DeleteCustomerControllerTests : IAsyncLifetime
    {
        private readonly CustomerApiFactory _apiFactory;
        private readonly HttpClient _httpClient;

        public DeleteCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
            _httpClient = apiFactory.HttpClient;
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenCustomerExists()
        {
            // Arrange
            var customer = CreateCustomerUtils.CustomerGenerator.Generate();
            var createCustomerResponseMessage = await _httpClient.PostAsJsonAsync("customers", customer);
            createCustomerResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
            var createCustomerResponse = await createCustomerResponseMessage.Content.ReadFromJsonAsync<CustomerResponse>();
            createCustomerResponse.Should().NotBe(null);
            createCustomerResponse!.Id.Should().NotBeEmpty();

            // Act
            var response = await _httpClient.DeleteAsync($"customers/{createCustomerResponse.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenCustomerExists_V2_WithDoubleCheckingThroughGetEndpoint()
        {
            // Arrange
            // 1. Create a valid customer and ensure it's a success
            var customer = CreateCustomerUtils.CustomerGenerator.Generate();
            var createCustomerResponseMessage = await _httpClient.PostAsJsonAsync("customers", customer);
            createCustomerResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
            var createCustomerResponse = await createCustomerResponseMessage.Content.ReadFromJsonAsync<CustomerResponse>();
            createCustomerResponse.Should().NotBe(null);
            createCustomerResponse!.Id.Should().NotBeEmpty();
            // 1.1. Double-ensure the success by using the Get endpoint. Is it an overkill for integration tests ??
            var getByIdResponse = await _httpClient.GetAsync($"customers/{createCustomerResponse.Id}");
            getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var getByIdCustomerResponse = await getByIdResponse.Content.ReadFromJsonAsync<CustomerResponse>();
            getByIdCustomerResponse.Should().BeEquivalentTo(customer);
            getByIdCustomerResponse!.Id.Should().Be(createCustomerResponse.Id);

            // Act
            var response = await _httpClient.DeleteAsync($"customers/{createCustomerResponse.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var getByIdResponseAgain = await _httpClient.GetAsync($"customers/{createCustomerResponse.Id}");
            getByIdResponseAgain.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var idThatHasAnAstronomicalChanceOfExisting = Guid.NewGuid();

            // Act
            var response = await _httpClient.DeleteAsync($"customers/{idThatHasAnAstronomicalChanceOfExisting}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        public async Task DisposeAsync()
        {
            await _apiFactory.ResetDatabase();
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }
}
