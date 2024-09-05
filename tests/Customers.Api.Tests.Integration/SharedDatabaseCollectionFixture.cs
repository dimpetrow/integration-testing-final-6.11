using Xunit;

namespace Customers.Api.Tests.Integration
{
    [CollectionDefinition(nameof(SharedDatabaseCollectionFixture))]
    public class SharedDatabaseCollectionFixture : ICollectionFixture<CustomerApiFactory>
    { }
}
