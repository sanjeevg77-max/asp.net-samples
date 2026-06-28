// Program.cs — .NET 8.0 minimal hosting model
// Migrated from Global.asax / Global.asax.cs (ASP.NET MVC 5 / .NET Framework 4.7.2)
//
// Legacy Application_Start() registrations and their .NET 8 equivalents:
//   AreaRegistration.RegisterAllAreas()           → builder.Services.AddControllersWithViews() + app.MapControllerRoute() with area support
//   FilterConfig.RegisterGlobalFilters(...)       → built-in exception-handling middleware (HandleErrorAttribute is superseded)
//   RouteConfig.RegisterRoutes(RouteTable.Routes) → app.MapControllerRoute() (default + custom File routes below)
//   BundleConfig.RegisterBundles(BundleTable.Bundles) → removed; Microsoft.AspNet.Web.Optimization is not available in .NET 8;
//                                                         static assets (CSS/JS) are served via app.UseStaticFiles()
//
// Web.config → appsettings.json migrations:
//   <connectionStrings>              → appsettings.json "ConnectionStrings" section
//   <httpRuntime maxRequestLength>   → builder.Services.Configure<FormOptions> + builder.WebHost.ConfigureKestrel

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. MVC services — replaces AreaRegistration.RegisterAllAreas() and
//    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters).
//    HandleErrorAttribute is superseded by the built-in exception-handling
//    middleware; no explicit global filter registration is required.
// ---------------------------------------------------------------------------
builder.Services.AddControllersWithViews();

// ---------------------------------------------------------------------------
// 2. File-upload size limits
//    Web.config: <httpRuntime maxRequestLength="1048576" /> (value is in KB)
//    1 048 576 KB = 1 073 741 824 000 bytes ≈ 1 TB — kept as-is to preserve
//    the original intent.  Adjust the constants below for your deployment.
// ---------------------------------------------------------------------------
const long MaxRequestBodyBytes = 1048576L * 1024L; // 1 048 576 KB converted to bytes

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit    = MaxRequestBodyBytes;
    options.ValueLengthLimit            = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.Configure<KestrelServerOptions>(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = MaxRequestBodyBytes;
});

// ---------------------------------------------------------------------------
// 3. Build the application
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// 4. Exception handling / HSTS
//    Replaces FilterConfig → HandleErrorAttribute global filter.
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // Default HSTS value is 30 days. Adjust for production scenarios:
    // https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}

// ---------------------------------------------------------------------------
// 5. Static files — replaces BundleConfig.RegisterBundles(BundleTable.Bundles)
//    CSS/JS assets placed under wwwroot/ are served directly without bundling.
//    For bundling/minification in .NET 8 consider WebOptimizer or LibMan.
// ---------------------------------------------------------------------------
app.UseHttpsRedirection();
app.UseStaticFiles();

// ---------------------------------------------------------------------------
// 6. Routing & authorisation middleware
// ---------------------------------------------------------------------------
app.UseRouting();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// 7. Route mapping — replaces RouteConfig.RegisterRoutes(RouteTable.Routes)
//
//    Original RouteConfig routes:
//      Default         {controller}/{action}/{id?}        → Home/Index
//      UploadForm      File/UploadForm
//      Upload          File/UploadFile
//      ViewOldUploads  File/ViewOldUploads/{id?}
//
//    Custom File routes are kept explicit here to mirror the original
//    RouteConfig exactly; they are also matched by the default pattern.
//
//    Area support (replaces AreaRegistration.RegisterAllAreas()) is enabled
//    automatically when AddControllersWithViews() scans assemblies; the area
//    route is registered first so it takes precedence over the default route.
// ---------------------------------------------------------------------------

// Area route — replaces AreaRegistration.RegisterAllAreas()
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Custom route: File upload form
app.MapControllerRoute(
    name: "UploadForm",
    pattern: "File/UploadForm",
    defaults: new { controller = "File", action = "UploadForm" });

// Custom route: File upload action
app.MapControllerRoute(
    name: "Upload",
    pattern: "File/UploadFile",
    defaults: new { controller = "File", action = "Upload" });

// Custom route: View old uploads (with optional id)
app.MapControllerRoute(
    name: "ViewOldUploads",
    pattern: "File/ViewOldUploads/{id?}",
    defaults: new { controller = "File", action = "ViewOldUpload" });

// Default conventional MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------------------------------------------------------------------------
// 8. Run
// ---------------------------------------------------------------------------
app.Run();
