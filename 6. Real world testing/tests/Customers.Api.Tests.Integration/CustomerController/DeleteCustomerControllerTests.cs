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

            // Act
            var response = await _httpClient.DeleteAsync($"customers/{createCustomerResponse!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Act
            var response = await _httpClient.DeleteAsync($"customers/{Guid.NewGuid()}");

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
