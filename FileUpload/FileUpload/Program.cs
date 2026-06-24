using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Register ASP.NET Core MVC services
// Replaces: FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
// HandleErrorAttribute is handled automatically by ASP.NET Core exception middleware
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    // Replaces: HandleErrorAttribute global filter for non-development environments
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Replaces: BundleConfig.RegisterBundles() — System.Web.Optimization is not available in .NET 8.0
// Static files (CSS, JS, images) are served directly from wwwroot
app.UseStaticFiles();

// Replaces: routes.IgnoreRoute("{resource}.axd/{*pathInfo}") — .axd handlers do not exist in ASP.NET Core
app.UseRouting();

app.UseAuthorization();

// Replaces: AreaRegistration.RegisterAllAreas()
// Replaces: RouteConfig.RegisterRoutes(RouteTable.Routes)
// Original route: {controller}/{action}/{id} with defaults Home/Index and optional id
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
