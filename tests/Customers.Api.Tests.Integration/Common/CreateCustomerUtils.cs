using Bogus;
using Customers.Api.Contracts.Requests;

namespace Customers.Api.Tests.Integration.Common
{
    public static class CreateCustomerUtils
    {
        public static readonly Faker<CustomerRequest> CustomerGenerator = new Faker<CustomerRequest>()
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
            .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);
    }
}
