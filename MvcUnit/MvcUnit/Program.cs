// Program.cs — .NET 8.0 entry point
// Migrated from Global.asax / Global.asax.cs (MvcApplication.Application_Start)
//
// Original startup sequence (Global.asax.cs):
//   1. AreaRegistration.RegisterAllAreas()        → app.MapAreaControllerRoute()
//   2. UnityConfig.RegisterComponents()           → UnityConfig.RegisterComponents(builder.Services)
//                                                    + builder.Host.UseUnityServiceProvider()
//   3. FilterConfig.RegisterGlobalFilters(...)    → builder.Services.AddControllersWithViews() + exception handler
//   4. RouteConfig.RegisterRoutes(...)            → app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}")
//   5. BundleConfig.RegisterBundles(...)          → REMOVED — Microsoft.AspNet.Web.Optimization unavailable in .NET 8;
//                                                    static files (CSS/JS) served directly via app.UseStaticFiles()
//
// Unity.Mvc5 upgrade path:
//   Unity.Mvc5 used System.Web.Mvc.DependencyResolver.SetResolver(new UnityDependencyResolver(container))
//   which is not available in .NET 8.0.  The replacement is Unity.Microsoft.DependencyInjection
//   (5.11.11), which integrates Unity with ASP.NET Core's IServiceCollection / IServiceProvider
//   by replacing the default host service provider via UseUnityServiceProvider().

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MvcUnit;
using MvcUnit.Models;


var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. Service registrations
// ---------------------------------------------------------------------------

// Replaces System.Web.Mvc controller/view engine registration.
// Also replaces FilterConfig.RegisterGlobalFilters — HandleErrorAttribute is
// covered by the built-in exception-handling middleware configured below.
builder.Services.AddControllersWithViews(options =>
{
    // Global error handling: equivalent to filters.Add(new HandleErrorAttribute())
    // Unhandled exceptions are caught by UseExceptionHandler middleware below;
    // no additional filter registration is required in .NET 8.
});

// ---------------------------------------------------------------------------
// 2. Dependency-injection registrations via Unity
//    Replaces: DependencyResolver.SetResolver(new UnityDependencyResolver(container))
//    from the original Unity.Mvc5-based UnityConfig.
//
//    UnityConfig.RegisterComponents() populates the Unity container with the
//    registrations above (IProductRepository → ProductRepository, Transient)
//    AND with all ASP.NET Core framework services already added to
//    builder.Services (MVC infrastructure, logging, etc.).
//
//    UseUnityServiceProvider() then replaces the default Microsoft DI
//    service provider with Unity so every controller and service resolved
//    by ASP.NET Core goes through the Unity container.
// ---------------------------------------------------------------------------
UnityConfig.RegisterComponents(builder.Services);

// ---------------------------------------------------------------------------
// 3. Build the application
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// 4. Middleware pipeline
//    Order mirrors the recommended ASP.NET Core pipeline order and the
//    original Application_Start sequence.
// ---------------------------------------------------------------------------

// Replaces FilterConfig's HandleErrorAttribute for non-development environments.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Equivalent to HandleErrorAttribute — redirects to /Home/Error on unhandled exceptions.
    app.UseExceptionHandler("/Home/Error");
    // HTTP Strict Transport Security (recommended for production).
    app.UseHsts();
}

// Enforce HTTPS (replaces IIS rewrite rules that were implicit in classic MVC).
app.UseHttpsRedirection();

// Replaces BundleConfig.RegisterBundles / BundleTable.Bundles.
// Microsoft.AspNet.Web.Optimization is not available in .NET 8.
// CSS and JavaScript files under wwwroot/ are served directly from disk.
app.UseStaticFiles();

// Enables routing middleware — required before MapControllerRoute / MapAreaControllerRoute.
app.UseRouting();

// Enables authorisation middleware (place after UseRouting, before Map*).
app.UseAuthorization();

// ---------------------------------------------------------------------------
// 5. Route registration
//    Replaces AreaRegistration.RegisterAllAreas() + RouteConfig.RegisterRoutes()
// ---------------------------------------------------------------------------

// Area route — replaces AreaRegistration.RegisterAllAreas().
// Maps area-prefixed URLs: {area}/{controller}/{action}/{id?}
app.MapAreaControllerRoute(
    name: "areas",
    areaName: "{area}",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// Default route — replaces RouteConfig.RegisterRoutes / routes.MapRoute("Default", ...).
// Equivalent to the original: url = "{controller}/{action}/{id}",
//   defaults: controller="Home", action="Index", id=UrlParameter.Optional
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// ---------------------------------------------------------------------------
// 6. Run the application
// ---------------------------------------------------------------------------
app.Run();
