// Migrated from Global.asax / Global.asax.cs + App_Start/WebApiConfig.cs
// Legacy entry points removed:
//   - WebApiApplication : System.Web.HttpApplication  (Global.asax.cs)
//   - WebApiConfig.Register(HttpConfiguration)        (App_Start/WebApiConfig.cs)
//   - AreaRegistration.RegisterAllAreas()             (HelpPage area — no equivalent needed in ASP.NET Core)
// Routing migrated:
//   - config.MapHttpAttributeRoutes()                 -> app.MapControllers() (attribute routing is on by default)
//   - config.Routes.MapHttpRoute("DefaultApi",
//       "api/{controller}/{id}", new { id = Optional }) -> conventional route registered below

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registrations
// ---------------------------------------------------------------------------

// Replaces GlobalConfiguration.Configure(WebApiConfig.Register):
// AddControllers registers the Web API / MVC controller pipeline without View support.
builder.Services.AddControllers();

// ---------------------------------------------------------------------------
// Build the application
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Equivalent to the production error handling that was absent in the legacy app.
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Serves static files from wwwroot (replaces IIS static file handling from Web.config).
app.UseStaticFiles();

// Required before UseAuthorization and MapControllers.
app.UseRouting();

app.UseAuthorization();

// ---------------------------------------------------------------------------
// Endpoint / route registration
// ---------------------------------------------------------------------------

// Attribute routing: replaces config.MapHttpAttributeRoutes().
// Any [Route] attributes on controllers are honoured automatically by MapControllers().

// Conventional route: replaces config.Routes.MapHttpRoute(
//     name: "DefaultApi",
//     routeTemplate: "api/{controller}/{id}",
//     defaults: new { id = RouteParameter.Optional }
// )
// In ASP.NET Core the action must be disambiguated, so the conventional template
// uses {action} as well; callers that relied on the implicit GET mapping should
// migrate their controllers to use [HttpGet] / [HttpGet("{id}")] attribute routes.
app.MapControllerRoute(
    name: "DefaultApi",
    pattern: "api/{controller}/{id?}");

// Maps all remaining attribute-routed controllers (Web API + MVC).
app.MapControllers();

app.Run();
