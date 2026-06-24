// Program.cs - Migrated from Global.asax / Startup.cs to .NET 8.0 minimal hosting model
//
// Migration notes:
//   - AreaRegistration.RegisterAllAreas()     => Areas are auto-discovered in ASP.NET Core MVC
//   - FilterConfig.RegisterGlobalFilters()    => HandleErrorAttribute replaced by UseExceptionHandler middleware
//   - RouteConfig.RegisterRoutes()            => Mapped via MapControllerRoute (Default + Hello routes)
//   - BundleConfig.RegisterBundles()          => System.Web.Optimization not available in .NET 8;
//                                               static assets (JS/CSS) are served from wwwroot via UseStaticFiles()
//   - ProductDBCintext / DrugDBContext        => Registered via AddDbContext with connection strings from appsettings.json
//   - Web.config connection strings           => Already migrated to appsettings.json
//   - Web.config appSettings                 => Already migrated to appsettings.json

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication2.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registrations
// ---------------------------------------------------------------------------

// Migrated from FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters):
// HandleErrorAttribute is replaced by the built-in exception-handling middleware below.
// AddControllersWithViews covers MVC controller + Razor view support.
builder.Services.AddControllersWithViews(options =>
{
    // HandleErrorAttribute equivalent: errors are handled by UseExceptionHandler middleware.
    // No additional filter registration is required here.
});

// Migrated from ProductDBCintext (Models/Product.cs) - EF DbContext registration.
// Connection string 'ProductDBContext' sourced from appsettings.json (migrated from Web.config).
builder.Services.AddDbContext<ProductDBCintext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDBContext")));

// Migrated from DrugDBContext (Models/Drug.cs) - EF DbContext registration.
// Connection string 'DrugDBContext' sourced from appsettings.json (migrated from Web.config).
builder.Services.AddDbContext<DrugDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DrugDBContext")));

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------

// Migrated from FilterConfig: HandleErrorAttribute => exception-handling middleware.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Equivalent of HandleErrorAttribute for production environments.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Replaces BundleConfig / System.Web.Optimization:
// Static files (JS, CSS, images) are served directly from the wwwroot folder.
// Scripts and Content directories have already been placed under wwwroot/.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ---------------------------------------------------------------------------
// Route configuration - migrated from RouteConfig.RegisterRoutes()
// ---------------------------------------------------------------------------

// Migrated from:
//   routes.MapRoute(name: "Default",
//       url: "{controller}/{action}/{id}",
//       defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Migrated from:
//   routes.MapRoute(name: "Hello",
//       url: "{controller}/{action}/{name}/{id}");
// HelloWorld controller uses a separate route with an extra {name} segment.
app.MapControllerRoute(
    name: "Hello",
    pattern: "{controller}/{action}/{name}/{id}");

// Areas are auto-discovered in ASP.NET Core - no explicit AreaRegistration.RegisterAllAreas() call needed.
app.MapAreaControllerRoute(
    name: "areas",
    areaName: "{area:exists}",
    pattern: "{area}/{controller=Home}/{action=Index}/{id?}");

app.Run();
