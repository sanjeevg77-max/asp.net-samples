// More info can be found at : https://codewithmukesh.com/blog/identityserver4-in-aspnet-core/
// When build and run go to : https://localhost:7232/.well-known/openid-configuration

using IDServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityServer()
    .AddInMemoryClients(IdentityConfiguration.Clients)
    .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
    .AddInMemoryApiResources(IdentityConfiguration.ApiResources)
    .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
    .AddTestUsers(IdentityConfiguration.TestUsers)
    .AddDeveloperSigningCredential();

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.UseIdentityServer();

app.MapGet("/", () => "Hello World!");

app.Run();
