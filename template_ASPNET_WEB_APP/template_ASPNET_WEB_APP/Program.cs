using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registrations
// ---------------------------------------------------------------------------

// Replaces AreaRegistration.RegisterAllAreas() + FilterConfig.RegisterGlobalFilters()
// Areas are discovered automatically; HandleErrorAttribute equivalent is the
// built-in exception-handling middleware configured below.
builder.Services.AddControllersWithViews(options =>
{
    // Equivalent of FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
    // which registered HandleErrorAttribute.  In ASP.NET Core the exception
    // handling middleware (app.UseExceptionHandler) replaces that filter.
    // Any additional global action filters should be added here, e.g.:
    // options.Filters.Add<MyCustomActionFilter>();
});

// ---------------------------------------------------------------------------
// Build the application
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    // Equivalent of HandleErrorAttribute (FilterConfig) for production:
    // catches unhandled exceptions and renders a friendly error page.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Serves files from wwwroot (place Content/ and Scripts/ there, or configure
// additional static-file providers as needed).
// Replaces BundleConfig.RegisterBundles() — System.Web.Optimization is not
// available in .NET 8.0.  Reference CSS/JS files directly in your Razor views.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ---------------------------------------------------------------------------
// Route configuration
// Replaces RouteConfig.RegisterRoutes(RouteTable.Routes) which defined:
//   url: "{controller}/{action}/{id}"
//   defaults: controller=Home, action=Index, id=Optional
// The {resource}.axd IgnoreRoute is not needed in ASP.NET Core.
// ---------------------------------------------------------------------------

// Area route — replaces AreaRegistration.RegisterAllAreas()
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default conventional route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
