using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvcUnit.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registrations
// ---------------------------------------------------------------------------

// Migrated from FilterConfig.RegisterGlobalFilters() — adds HandleErrorAttribute
// equivalent via the built-in exception-handling middleware configured below.
// Migrated from AreaRegistration.RegisterAllAreas() — areas are discovered
// automatically when controllers are registered with AddControllersWithViews().
builder.Services.AddControllersWithViews(options =>
{
    // Equivalent of FilterConfig.RegisterGlobalFilters: HandleErrorAttribute
    // is replaced by the built-in exception-handler middleware in .NET 8.
    // Global error handling is configured via app.UseExceptionHandler() below.
});

// Register EF Core DbContext with SQL Server provider.
builder.Services.AddDbContext<ProductDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MvcUnitContext")));

// Migrated from UnityConfig.RegisterComponents():
//   container.RegisterType<IProductRepository, ProductRepository>();
// Unity DI container replaced by the built-in .NET DI container.
builder.Services.AddTransient<IProductRepository, ProductRepository>();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// Migrated from Startup.Configure() and RouteConfig.RegisterRoutes()
// ---------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Replaces FilterConfig HandleErrorAttribute global filter for production.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Migrated from web.config HTTPS enforcement
app.UseHttpsRedirection();

// Serves static files from wwwroot (replaces BundleConfig — bundling is
// handled differently in .NET 8; static assets are served directly).
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Migrated from RouteConfig.RegisterRoutes():
//   routes.MapRoute(name: "Default", url: "{controller}/{action}/{id}",
//       defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional })
// routes.IgnoreRoute("{resource}.axd/{*pathInfo}") is not needed in .NET 8.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
