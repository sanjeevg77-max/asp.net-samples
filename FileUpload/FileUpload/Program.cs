using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ============================================================
// Program.cs — FileUpload (.NET 8.0, C# 12)
// Migrated from Global.asax / Global.asax.cs
//
// Migration summary:
//   • AreaRegistration.RegisterAllAreas()           → builder.Services.AddControllersWithViews()
//   • FilterConfig.RegisterGlobalFilters(...)       → app.UseExceptionHandler("/Home/Error")
//   • RouteConfig.RegisterRoutes(RouteTable.Routes) → app.MapControllerRoute("default", ...)
//   • BundleConfig.RegisterBundles(...)             → app.UseStaticFiles()
//     (System.Web.Optimization is not supported in .NET 8;
//      static assets in Content/ and Scripts/ are served via UseStaticFiles())
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registrations
// ---------------------------------------------------------------------------

// Replaces AreaRegistration.RegisterAllAreas() and enables MVC with Razor views.
// Area support is built-in when controllers carry [Area] attributes.
builder.Services.AddControllersWithViews();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // Replaces FilterConfig → HandleErrorAttribute for production error handling.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Replaces BundleConfig.RegisterBundles() — serves CSS/JS from Content/ and Scripts/
// directories as plain static files without bundling or minification.
app.UseStaticFiles();

// Enables routing so that endpoint middleware can match requests.
app.UseRouting();

// Enables authorisation middleware (required between UseRouting and MapControllerRoute).
app.UseAuthorization();

// ---------------------------------------------------------------------------
// Endpoint routing
// ---------------------------------------------------------------------------

// Replaces RouteConfig → routes.MapRoute("Default", "{controller}/{action}/{id}", ...)
// routes.IgnoreRoute("{resource}.axd/{*pathInfo}") is not needed in .NET 8
// because .axd handlers do not exist in the ASP.NET Core pipeline.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
