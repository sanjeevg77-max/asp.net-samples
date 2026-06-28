// Program.cs — .NET 8.0 minimal hosting model
// Migrated from Global.asax.cs (MvcApplication.Application_Start),
// App_Start/RouteConfig.cs, App_Start/FilterConfig.cs, and App_Start/BundleConfig.cs.
//
// BundleConfig note: System.Web.Optimization bundles are not supported in ASP.NET Core.
// jQuery, Bootstrap, Modernizr and jquery.validate assets are served directly from
// wwwroot/Scripts and wwwroot/Content via app.UseStaticFiles().

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------
// Service registrations
// ----------------------------------------------------------------

// Migrated from: FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
//   — GlobalFilters contained HandleErrorAttribute, which maps to the
//     built-in exception-handler middleware and the MVC filter below.
// Migrated from: AreaRegistration.RegisterAllAreas()
//   — Areas are discovered automatically when AddControllersWithViews() is called.
// Register EF Core DbContext instances (migrated from EntityFramework 6.x)
builder.Services.AddDbContext<DrugDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DrugDBContext")));
builder.Services.AddDbContext<ProductDBCintext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDBContext")));

builder.Services.AddControllersWithViews(options =>
{
    // Equivalent of filters.Add(new HandleErrorAttribute()) from FilterConfig.
    // HandleError behaviour is provided by UseExceptionHandler / UseDeveloperExceptionPage
    // in the pipeline below; the ResponseCache filter is the closest MVC-level equivalent
    // for ensuring error responses are not cached.
    options.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
});

// ----------------------------------------------------------------
// Build the application
// ----------------------------------------------------------------
var app = builder.Build();

// Expose IConfiguration via the static shim so any existing code that
// references ConfigurationManager.Configuration continues to compile.
ConfigurationManager.Configuration = app.Configuration;

// ----------------------------------------------------------------
// Middleware pipeline
// Migrated from: Startup.Configure() and Global.asax Application_Start
// ----------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    // Developer-friendly error page replaces HandleErrorAttribute in development.
    app.UseDeveloperExceptionPage();
}
else
{
    // Migrated from FilterConfig HandleErrorAttribute — production error handling.
    app.UseExceptionHandler("/Home/Error");
    // HSTS — recommended for production; default is 30 days.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Migrated from BundleConfig: static assets (jQuery, Bootstrap, Modernizr,
// jquery.validate, site.css) are served directly from wwwroot/Scripts and
// wwwroot/Content instead of via System.Web.Optimization bundles.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ----------------------------------------------------------------
// Route configuration
// Migrated from: RouteConfig.RegisterRoutes(RouteTable.Routes)
// Original routes:
//   routes.IgnoreRoute("{resource}.axd/{*pathInfo}")  — not needed in ASP.NET Core
//   Default:  {controller}/{action}/{id}  defaults: Home/Index, id optional
//   Hello:    {controller}/{action}/{name}/{id}
// ----------------------------------------------------------------

// HelloWorld route — matches {controller}/{action}/{name}/{id}
// Registered first (more-specific pattern before the catch-all default).
app.MapControllerRoute(
    name: "Hello",
    pattern: "{controller}/{action}/{name}/{id}");

// Default route — equivalent of the original Default MapRoute.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Area controller routes — supports AreaRegistration.RegisterAllAreas().
app.MapAreaControllerRoute(
    name: "areas",
    areaName: "{area:exists}",
    pattern: "{area}/{controller=Home}/{action=Index}/{id?}");

app.Run();

// ---------------------------------------------------------------------------
// ConfigurationManager shim
// Preserves compatibility with any code that previously read IConfiguration
// through the static helper that was defined in the old Startup.cs.
// ---------------------------------------------------------------------------
public class ConfigurationManager
{
    public static IConfiguration? Configuration { get; set; }
}
