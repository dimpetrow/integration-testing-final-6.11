
using Microsoft.AspNetCore.Hosting.Server;
using System.Collections.Concurrent;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

var wiremockServer = WireMockServer.Start(55773);

Console.WriteLine($"Wiremock is now running on: {wiremockServer.Url}");

ConcurrentDictionary<string, Guid> usernameWireMockMappingIdPairs = new ConcurrentDictionary<string, Guid>();

SetupUser("nickchapsas", @"{
  ""login"": ""nickchapsas"",
  ""id"": 67104228,
  ""node_id"": ""MDQ6VXNlcjY3MTA0MjI4"",
  ""avatar_url"": ""https://avatars.githubusercontent.com/u/67104228?v=4"",
  ""gravatar_id"": """",
  ""url"": ""https://api.github.com/users/nickchapsas"",
  ""html_url"": ""https://github.com/nickchapsas"",
  ""followers_url"": ""https://api.github.com/users/nickchapsas/followers"",
  ""following_url"": ""https://api.github.com/users/nickchapsas/following{/other_user}"",
  ""gists_url"": ""https://api.github.com/users/nickchapsas/gists{/gist_id}"",
  ""starred_url"": ""https://api.github.com/users/nickchapsas/starred{/owner}{/repo}"",
  ""subscriptions_url"": ""https://api.github.com/users/nickchapsas/subscriptions"",
  ""organizations_url"": ""https://api.github.com/users/nickchapsas/orgs"",
  ""repos_url"": ""https://api.github.com/users/nickchapsas/repos"",
  ""events_url"": ""https://api.github.com/users/nickchapsas/events{/privacy}"",
  ""received_events_url"": ""https://api.github.com/users/nickchapsas/received_events"",
  ""type"": ""User"",
  ""site_admin"": false,
  ""name"": null,
  ""company"": null,
  ""blog"": """",
  ""location"": null,
  ""email"": null,
  ""hireable"": null,
  ""bio"": null,
  ""twitter_username"": null,
  ""public_repos"": 0,
  ""public_gists"": 0,
  ""followers"": 0,
  ""following"": 0,
  ""created_at"": ""2020-06-18T11:47:58Z"",
  ""updated_at"": ""2020-06-18T11:47:58Z""
}");
SetupUser("nickchapsas", @"{
""message"": ""Not Found"",
""documentation_url"": ""https://docs.github.com/rest"",
""status"": ""404""
}");

//var getExistingUserRequest = Request.Create()
//    .WithPath("/users/nickchapsas")
//    .UsingGet();
//var getExistingUserResponse = Response.Create()
//        .WithBody(@"{
//  ""login"": ""nickchapsas"",
//  ""id"": 67104228,
//  ""node_id"": ""MDQ6VXNlcjY3MTA0MjI4"",
//  ""avatar_url"": ""https://avatars.githubusercontent.com/u/67104228?v=4"",
//  ""gravatar_id"": """",
//  ""url"": ""https://api.github.com/users/nickchapsas"",
//  ""html_url"": ""https://github.com/nickchapsas"",
//  ""followers_url"": ""https://api.github.com/users/nickchapsas/followers"",
//  ""following_url"": ""https://api.github.com/users/nickchapsas/following{/other_user}"",
//  ""gists_url"": ""https://api.github.com/users/nickchapsas/gists{/gist_id}"",
//  ""starred_url"": ""https://api.github.com/users/nickchapsas/starred{/owner}{/repo}"",
//  ""subscriptions_url"": ""https://api.github.com/users/nickchapsas/subscriptions"",
//  ""organizations_url"": ""https://api.github.com/users/nickchapsas/orgs"",
//  ""repos_url"": ""https://api.github.com/users/nickchapsas/repos"",
//  ""events_url"": ""https://api.github.com/users/nickchapsas/events{/privacy}"",
//  ""received_events_url"": ""https://api.github.com/users/nickchapsas/received_events"",
//  ""type"": ""User"",
//  ""site_admin"": false,
//  ""name"": null,
//  ""company"": null,
//  ""blog"": """",
//  ""location"": null,
//  ""email"": null,
//  ""hireable"": null,
//  ""bio"": null,
//  ""twitter_username"": null,
//  ""public_repos"": 0,
//  ""public_gists"": 0,
//  ""followers"": 0,
//  ""following"": 0,
//  ""created_at"": ""2020-06-18T11:47:58Z"",
//  ""updated_at"": ""2020-06-18T11:47:58Z""
//}")
//        .WithHeader("content-type", "application/json; charset=utf-8")
//        .WithStatusCode(200);
//wiremockServer.Given(getExistingUserRequest).RespondWith(getExistingUserResponse);

//var getNonExistingUserRequest = Request.Create()
//    .WithPath("/users/nickchapsas")
//    .UsingGet();
//var getNonExistingUserResponse = Response.Create()
//        .WithBody(@"{
//""message"": ""Not Found"",
//""documentation_url"": ""https://docs.github.com/rest"",
//""status"": ""404""
//}")
//        .WithHeader("content-type", "application/json; charset=utf-8")
//        .WithStatusCode(404);
//wiremockServer.Given(getNonExistingUserRequest).RespondWith(getNonExistingUserResponse);

Console.ReadKey();
wiremockServer.Dispose();

void SetupUser(string username, string responsBody)
{
    if (usernameWireMockMappingIdPairs.TryGetValue(username, out var existingMappingId))
    {
        wiremockServer.DeleteMapping(existingMappingId);
    }

    var newMappingId = Guid.NewGuid();
    wiremockServer.Given(
            Request.Create()
                .WithPath($"/users/{username}")
                .UsingGet())
        .WithGuid(newMappingId)
        .RespondWith(
            Response.Create()
                .WithBody(responsBody)
                .WithHeader("content-type", "application/json; charset=utf-8")
                .WithStatusCode(200));

    usernameWireMockMappingIdPairs[username] = newMappingId;
}
