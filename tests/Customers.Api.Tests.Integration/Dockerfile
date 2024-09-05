FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /tests

COPY . .
RUN dotnet build IntegrationTesting.sln

ENTRYPOINT [ "dotnet", "test", "./tests/Customers.Api.Tests.Integration/", "--no-build" ]