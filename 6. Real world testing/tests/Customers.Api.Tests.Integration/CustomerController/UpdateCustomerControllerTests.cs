using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using Customers.Api.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController
{
    [Collection(nameof(SharedDatabaseCollectionFixture))]
    public class UpdateCustomerControllerTests : IAsyncLifetime
    {
        private readonly CustomerApiFactory _apiFactory;
        private readonly HttpClient _httpClient;

        public UpdateCustomerControllerTests(
            CustomerApiFactory apiFactory
            ) // IMPORTANT: REMEMBER THIS CONTRUCTOR IS ALWAYS BEING EXECUTED ONCE BEFORE TEST, REGARDLESS OF CLASS AND COLLECTION FIXTURES!
        {
            _apiFactory = apiFactory;
            _httpClient = apiFactory.HttpClient;
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var idThatHasAnAstronomicalChanceOfExisting = Guid.NewGuid();
            var updateCustomerRequestBody = CreateCustomerUtils.CustomerGenerator.Generate();

            // Act
            var response = await _httpClient.PutAsJsonAsync($"customers/{idThatHasAnAstronomicalChanceOfExisting}", updateCustomerRequestBody);


            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //[Theory] // TODOCont: Generate invalid requests for each property alone, then each combination of them + verify that API returns validation errors per property
        public async Task Update_ReturnsBadRequest_WhenRequestIsNotValid()
        {
            //// Arrange
            //// 1. Create a valid customer and ensure it's a success
            //var customer = CreateCustomerUtils.CustomerGenerator.Generate();
            //var createCustomerResponseMessage = await _httpClient.PostAsJsonAsync("customers", customer);
            //createCustomerResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
            //var createCustomerResponse = await createCustomerResponseMessage.Content.ReadFromJsonAsync<CustomerResponse>();
            //createCustomerResponse.Should().NotBe(null);
            //createCustomerResponse!.Id.Should().NotBeEmpty();

            //// 2. Prepare an update request
            //var updateCustomerRequestBody = new CustomerRequest
            //{
            //    GitHubUsername = CustomerApiFactory.InvalidGithubUser,
            //    FullName = customer.FullName,
            //    DateOfBirth = customer.DateOfBirth,
            //    Email = customer.Email,
            //};

            //// Act
            //var response = await _httpClient.PutAsJsonAsync($"customers/{createCustomerResponse.Id}", updateCustomerRequestBody);

            //// Assert
            //response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            //var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            //error!.Status.Should().Be(400);
            //error.Title.Should().Be("One or more validation errors occurred.");
            //error.Errors[nameof(CustomerRequest.GitHubUsername)][0]
            //    .Should().Be($"There is no GitHub user with username {CustomerApiFactory.InvalidGithubUser}");
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenGitHubUsernameIsNotValid()
        {
            // Arrange
            // 1. Create a valid customer and ensure it's a success
            var customer = CreateCustomerUtils.CustomerGenerator.Generate();
            var createCustomerResponseMessage = await _httpClient.PostAsJsonAsync("customers", customer);
            createCustomerResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
            var createCustomerResponse = await createCustomerResponseMessage.Content.ReadFromJsonAsync<CustomerResponse>();
            createCustomerResponse.Should().NotBe(null);
            createCustomerResponse!.Id.Should().NotBeEmpty();

            // 2. Prepare an update request
            var updateCustomerRequestBody = new CustomerRequest
            {
                GitHubUsername = CustomerApiFactory.InvalidGithubUser,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth,
                Email = customer.Email,
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync($"customers/{createCustomerResponse.Id}", updateCustomerRequestBody);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            error!.Status.Should().Be(400);
            error.Title.Should().Be("One or more validation errors occurred.");
            error.Errors[nameof(CustomerRequest.GitHubUsername)][0]
                .Should().Be($"There is no GitHub user with username {CustomerApiFactory.InvalidGithubUser}");
        }

        [Fact]
        public async Task Update_ReturnsOkWithUpdatedCustomerObject_WhenRequestIsValid()
        {
            // Arrange
            // 1. Create a valid customer and ensure it's a success
            var customer = CreateCustomerUtils.CustomerGenerator.Generate();
            var createCustomerResponseMessage = await _httpClient.PostAsJsonAsync("customers", customer);
            createCustomerResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
            var createCustomerResponse = await createCustomerResponseMessage.Content.ReadFromJsonAsync<CustomerResponse>();
            createCustomerResponse.Should().NotBe(null);
            createCustomerResponse!.Id.Should().NotBeEmpty();

            // 2. Prepare a valid update request by regenerating data with Bogus again so we end up with different values
            var updateCustomerRequestBody = CreateCustomerUtils.CustomerGenerator.Generate();

            // Act
            var response = await _httpClient.PutAsJsonAsync($"customers/{createCustomerResponse.Id}", updateCustomerRequestBody);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updateCustomerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            updateCustomerResponse.Should().BeEquivalentTo(updateCustomerRequestBody);
            updateCustomerResponse!.Id.Should().Be(createCustomerResponse.Id);

        }
        [Fact]
        public async Task Update_ReturnsOkWithUpdatedCustomerObject_WhenRequestIsValid_V2_WithDoubleCheckingThroughGetEndpoint()
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

            // 2. Prepare a valid update request by regenerating data with Bogus again so we end up with different values
            var updateCustomerRequestBody = CreateCustomerUtils.CustomerGenerator.Generate();

            // Act
            var response = await _httpClient.PutAsJsonAsync($"customers/{createCustomerResponse.Id}", updateCustomerRequestBody);

            // Assert
            // 1. the Update response
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updateCustomerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            updateCustomerResponse.Should().BeEquivalentTo(updateCustomerRequestBody);
            updateCustomerResponse!.Id.Should().Be(createCustomerResponse.Id);
            // 2. the Get response
            var getByIdResponseAgain = await _httpClient.GetAsync($"customers/{createCustomerResponse.Id}");
            getByIdResponseAgain.StatusCode.Should().Be(HttpStatusCode.OK);
            var getByIdCustomerResponseAgain = await getByIdResponseAgain.Content.ReadFromJsonAsync<CustomerResponse>();
            getByIdCustomerResponseAgain.Should().BeEquivalentTo(updateCustomerRequestBody);
            getByIdCustomerResponseAgain!.Id.Should().Be(createCustomerResponse.Id);

        }
        [Fact]
        public async Task Update_Returns500_WhenGithubApiReturnsForbidden() // WHUUT?!
        {
            // Arrange
            // 1. Create a valid customer and ensure it's a success
            var customer = CreateCustomerUtils.CustomerGenerator.Generate();
            var createCustomerResponseMessage = await _httpClient.PostAsJsonAsync("customers", customer);
            createCustomerResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
            var createCustomerResponse = await createCustomerResponseMessage.Content.ReadFromJsonAsync<CustomerResponse>();
            createCustomerResponse.Should().NotBe(null);
            createCustomerResponse!.Id.Should().NotBeEmpty();

            // 2. Prepare an update request
            var updateCustomerRequestBody = new CustomerRequest
            {
                GitHubUsername = CustomerApiFactory.GithubUserCallerHasNoAccessTo,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth,
                Email = customer.Email,
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync($"customers/{createCustomerResponse.Id}", updateCustomerRequestBody);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        public async Task DisposeAsync()
        {
            await _apiFactory.ResetDatabase();
        }

        public Task InitializeAsync() => Task.CompletedTask;
    }
}
