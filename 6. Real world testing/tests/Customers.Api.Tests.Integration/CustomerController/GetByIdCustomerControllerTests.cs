using Customers.Api.Contracts.Responses;
using Customers.Api.Tests.Integration.Common;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using FluentAssertions;

namespace Customers.Api.Tests.Integration.CustomerController
{
    [Collection(nameof(SharedDatabaseCollectionFixture))]
    public class GetByIdCustomerControllerTests : IAsyncLifetime
    {
        private readonly CustomerApiFactory _apiFactory;
        private readonly HttpClient _httpClient;

        public GetByIdCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
            _httpClient = apiFactory.HttpClient;
        }

        [Fact]
        public async Task GetById_ReturnsCustomer_WhenExists()
        {
            // Arrange
            var customer = CreateCustomerUtils.CustomerGenerator.Generate();
            var createCustomerResponseMessage = await _httpClient.PostAsJsonAsync("customers", customer);
            createCustomerResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
            var createCustomerResponse = await createCustomerResponseMessage.Content.ReadFromJsonAsync<CustomerResponse>();
            createCustomerResponse.Should().NotBe(null);
            createCustomerResponse!.Id.Should().NotBeEmpty();

            // Act
            var response = await _httpClient.GetAsync($"customers/{createCustomerResponse.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var getByIdCustomerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            getByIdCustomerResponse.Should().BeEquivalentTo(customer);
            getByIdCustomerResponse!.Id.Should().Be(createCustomerResponse.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var idThatHasAnAstronomicalChanceOfExisting = Guid.NewGuid();

            // Act
            var response = await _httpClient.GetAsync($"customers/{idThatHasAnAstronomicalChanceOfExisting}");

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
