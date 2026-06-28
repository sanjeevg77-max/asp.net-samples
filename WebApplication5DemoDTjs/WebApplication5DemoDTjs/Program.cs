// Program.cs — migrated from Global.asax / Global.asax.cs (ASP.NET MVC 5, .NET Framework 4.7.2)
// Target: ASP.NET Core MVC, .NET 8.0, C# 12, minimal hosting model
//
// Migration summary
// -----------------
// Global.asax.cs  : MvcApplication.Application_Start()
//     AreaRegistration.RegisterAllAreas()          → app.MapControllerRoute() covers default area;
//                                                    explicit area routes added with MapAreaControllerRoute if needed.
//     FilterConfig.RegisterGlobalFilters()         → builder.Services.AddControllersWithViews(options => ...)
//                                                    HandleErrorAttribute → app.UseExceptionHandler / app.UseHsts
//     RouteConfig.RegisterRoutes()                 → app.MapControllerRoute() "default" pattern
//     BundleConfig.RegisterBundles()               → REMOVED; System.Web.Optimization is not supported in .NET 8.
//                                                    Static assets (CSS/JS) in wwwroot/ are served by UseStaticFiles()
//                                                    and referenced directly from Razor views.
// Web.config                                       → appsettings.json (already migrated; file kept for reference)
// Startup.cs (IHostBuilder pattern)                → replaced by this file; Startup.cs can be removed.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────────────────────────
// appsettings.json is loaded automatically by WebApplication.CreateBuilder.
// Connection strings from Web.config <connectionStrings> are already present in
// appsettings.json under "ConnectionStrings":
//   "DefaultConnection"              → SQL Server LocalDB (ASP.NET Identity)
//   "webApplication5DemoDTjsDBContext" → SQL Server LocalDB (Customers.mdf)
//   "test (webApplication5DemoDTjs)" → Oracle Managed Data Access
//
// App settings from Web.config <appSettings> are stored as top-level keys in
// appsettings.json:
//   "ClientValidationEnabled"        → "true"
//   "UnobtrusiveJavaScriptEnabled"   → "true"
//   "webpages:Enabled"               → "false"  (MVC Razor Pages flag — no longer relevant)

// ── Service registrations ────────────────────────────────────────────────────

// MVC with Views
// FilterConfig.RegisterGlobalFilters registered HandleErrorAttribute globally.
// In ASP.NET Core the equivalent is configuring the exception-handler middleware
// (app.UseExceptionHandler / app.UseDeveloperExceptionPage below) together with
// the built-in exception filter. HandleErrorAttribute itself is not available in
// ASP.NET Core; the options lambda below is the correct extension point for any
// custom global action filters.
builder.Services.AddControllersWithViews(options =>
{
    // Global filters (migrated from FilterConfig.RegisterGlobalFilters)
    // HandleErrorAttribute is not available in ASP.NET Core.
    // Error handling is provided by the exception-handler middleware configured
    // in the pipeline below (app.UseExceptionHandler / app.UseDeveloperExceptionPage).
    // Add any additional global ASP.NET Core action filters here, for example:
    //   options.Filters.Add<MyCustomGlobalFilter>();
});

// Entity Framework / Oracle Data Access
// Web.config registered EF 6 providers (SqlClient + Oracle.ManagedDataAccess.EntityFramework).
// For .NET 8 use EF Core. Register the appropriate DbContext(s) here, for example:
//
//   SQL Server:
//     builder.Services.AddDbContext<WebApplication5DemoDTjsDBContext>(options =>
//         options.UseSqlServer(
//             builder.Configuration.GetConnectionString("webApplication5DemoDTjsDBContext")));
//
//   Oracle (Oracle.EntityFrameworkCore):
//     builder.Services.AddDbContext<OracleDbContext>(options =>
//         options.UseOracle(
//             builder.Configuration.GetConnectionString("test (webApplication5DemoDTjs)")));
//
// Uncomment and adjust the registrations above once the EF Core packages are added to the project.

// ── Build the application ─────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
// Ordering follows ASP.NET Core conventions (error handling first, then
// security, static files, routing, auth, endpoints).

if (app.Environment.IsDevelopment())
{
    // Developer-friendly error page in Development (replaces HandleErrorAttribute
    // behaviour in non-production environments).
    app.UseDeveloperExceptionPage();
}
else
{
    // Production error handler — maps to the /Home/Error action.
    // This replaces the global HandleErrorAttribute registered in FilterConfig.
    app.UseExceptionHandler("/Home/Error");

    // HTTP Strict Transport Security (30-day default).
    app.UseHsts();
}

// Redirect HTTP → HTTPS.
app.UseHttpsRedirection();

// Serve files from wwwroot/ (replaces BundleConfig / ScriptBundle / StyleBundle).
// CSS and JS files that were previously bundled should now be referenced directly
// in _Layout.cshtml, e.g.:
//   <link rel="stylesheet" href="~/Content/bootstrap.min.css" />
//   <link rel="stylesheet" href="~/Content/Site.css" />
//   <script src="~/Scripts/jquery-3.4.1.min.js"></script>
//   <script src="~/Scripts/bootstrap.bundle.min.js"></script>
//   <script src="~/Scripts/jquery.validate.min.js"></script>
//   <script src="~/Scripts/jquery.validate.unobtrusive.min.js"></script>
app.UseStaticFiles();

// Routing middleware — must precede UseAuthorization and MapControllerRoute.
app.UseRouting();

// Authorization middleware.
app.UseAuthorization();

// ── Route registration (migrated from RouteConfig.RegisterRoutes) ─────────────
// Original System.Web.Routing route:
//   routes.IgnoreRoute("{resource}.axd/{*pathInfo}")  → not needed in ASP.NET Core
//   routes.MapRoute(
//       name: "Default",
//       url: "{controller}/{action}/{id}",
//       defaults: new { controller="Home", action="Index", id = UrlParameter.Optional })
//
// Equivalent minimal-hosting endpoint:
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start the application (replaces CreateHostBuilder(...).Build().Run()).
app.Run();
