﻿using Customers.Api.Contracts.Responses;
using Customers.Api.Tests.Integration.Common;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using FluentAssertions;

namespace Customers.Api.Tests.Integration.CustomerController
{
    [Collection(nameof(SharedDatabaseCollectionFixture))]
    public class GetAllCustomerControllerTests : IAsyncLifetime
    {
        private readonly CustomerApiFactory _apiFactory;
        private readonly HttpClient _httpClient;

        public GetAllCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
            _httpClient = apiFactory.HttpClient;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public async Task GetAll_ReturnsAllCustomers_WhenThereAreAny(int customersCount)
        {
            // Arrange
            var customerCreateRequests = CreateCustomerUtils.CustomerGenerator.Generate(customersCount);
            var expectedGetAllResponse = new List<CustomerResponse>();
            for (int i = 0; i < customersCount; i++)
            {
                var customer = customerCreateRequests[i];
                var createCustomerResponseMessage = await _httpClient.PostAsJsonAsync("customers", customer);
                createCustomerResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
                var createCustomerResponse = await createCustomerResponseMessage.Content.ReadFromJsonAsync<CustomerResponse>();
                createCustomerResponse.Should().NotBe(null);
                createCustomerResponse!.Id.Should().NotBeEmpty();

                expectedGetAllResponse.Add(new CustomerResponse
                {
                    Id = createCustomerResponse.Id,
                    DateOfBirth = customer.DateOfBirth,
                    Email = customer.Email,
                    FullName = customer.FullName,
                    GitHubUsername = customer.GitHubUsername,
                });
            }

            // Act
            var response = await _httpClient.GetAsync("customers");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var getAllCustomerResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
            getAllCustomerResponse.Should().BeEquivalentTo(new GetAllCustomersResponse { Customers = expectedGetAllResponse });
        }

        [Fact]
        public async Task GetAll_ReturnsEmpty_WhenThereAreNoCustomers()
        {
            // Act
            var response = await _httpClient.GetAsync("customers");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var getAllCustomerResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
            var expectedResponse = new GetAllCustomersResponse { Customers = Enumerable.Empty<CustomerResponse>() };
            getAllCustomerResponse.Should().BeEquivalentTo(expectedResponse);
        }

        public async Task DisposeAsync()
        {
            await _apiFactory.ResetDatabase();
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }
}
