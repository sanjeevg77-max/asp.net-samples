// ============================================================
// Program.cs  –  .NET 8 Minimal Hosting Model
// Migrated from:
//   • Global.asax.cs  (MvcApplication.Application_Start)
//   • App_Start/FilterConfig.cs   (HandleErrorAttribute global filter)
//   • App_Start/RouteConfig.cs    (Default + 3 named custom routes)
//   • App_Start/BundleConfig.cs   (see note below)
// ============================================================

// ── NOTE: BundleConfig (System.Web.Optimization) is not available in .NET 8.
//    Static assets (CSS, JS) are served directly from the wwwroot/ folder via
//    app.UseStaticFiles().  Reference them in Views with direct paths, e.g.
//      <link rel="stylesheet" href="~/Content/bootstrap.min.css" />
//      <script src="~/Scripts/jquery-3.4.1.min.js"></script>
//    All bundle assets have already been placed under wwwroot/Content/ and
//    wwwroot/Scripts/ during the migration.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────────────────────────
// appsettings.json is loaded automatically by WebApplication.CreateBuilder.
// Connection strings migrated from Web.config are already in appsettings.json.

// ── Service Registrations ────────────────────────────────────────────────────

// Replaces: AreaRegistration.RegisterAllAreas()  +  FilterConfig.RegisterGlobalFilters()
// FilterConfig registered a single global filter: HandleErrorAttribute.
// In ASP.NET Core the equivalent is the built-in exception-handling middleware
// (UseDeveloperExceptionPage / UseExceptionHandler) combined with the
// [HandleError]-equivalent behaviour provided by AddControllersWithViews itself.
// A global ExceptionFilter equivalent is wired via the options lambda below.
builder.Services.AddControllersWithViews(options =>
{
    // Equivalent to FilterConfig.RegisterGlobalFilters: adds a global error
    // handler filter so unhandled exceptions surface the shared Error view.
    // HandleErrorAttribute redirected to the Error view in MVC 5; the ASP.NET
    // Core equivalent is UseExceptionHandler("/Home/Error") in the pipeline,
    // but we also add AutoValidateAntiforgeryToken globally as good practice.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// ── Max request body size  ────────────────────────────────────────────────────
// Web.config: <httpRuntime maxRequestLength="1048576" /> (value is in KB)
// 1 048 576 KB = 1 073 741 824 000 bytes ≈ 1 GB
// Kestrel and IIS both respect this limit.
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1_073_741_824_000; // 1 048 576 KB from Web.config
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 1_073_741_824_000; // 1 048 576 KB from Web.config
});

// ── Build the application ────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware Pipeline ──────────────────────────────────────────────────────
// Order matters: exception handling must be first so it wraps everything else.

if (app.Environment.IsDevelopment())
{
    // Shows detailed error pages during development.
    app.UseDeveloperExceptionPage();
}
else
{
    // Replaces the HandleErrorAttribute global filter in non-development environments:
    // unhandled exceptions are caught here and forwarded to Home/Error.
    app.UseExceptionHandler("/Home/Error");
    // HTTP Strict Transport Security – recommended for production.
    app.UseHsts();
}

// Redirect HTTP → HTTPS (replaces IIS <httpRedirect> / SSL offloading config).
app.UseHttpsRedirection();

// Serve static files from wwwroot/ (replaces BundleConfig + ScriptManager).
// wwwroot/Content/  →  ~/Content/...
// wwwroot/Scripts/  →  ~/Scripts/...
app.UseStaticFiles();

// Enable routing so MapControllerRoute / MapControllers can match requests.
app.UseRouting();

// Enable authorisation middleware (must come after UseRouting, before endpoints).
app.UseAuthorization();

// ── Route Registrations ──────────────────────────────────────────────────────
// Migrated 1-for-1 from App_Start/RouteConfig.cs (RouteConfig.RegisterRoutes).
// Named routes are listed most-specific first so the conventional default
// does not swallow the explicit file-upload paths.

// Replaces: routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
// ASP.NET Core does not use .axd handlers; no explicit ignore rule is needed.

// Custom route: File/UploadForm  →  FileController.UploadForm()
// Replaces: routes.MapRoute("UploadForm", "File/UploadForm", ...)
app.MapControllerRoute(
    name: "UploadForm",
    pattern: "File/UploadForm",
    defaults: new { controller = "File", action = "UploadForm" });

// Custom route: File/UploadFile  →  FileController.Upload()
// Replaces: routes.MapRoute("Upload", "File/UploadFile", ...)
app.MapControllerRoute(
    name: "Upload",
    pattern: "File/UploadFile",
    defaults: new { controller = "File", action = "Upload" });

// Custom route: File/ViewOldUploads/{id?}  →  FileController.ViewOldUpload(id)
// Replaces: routes.MapRoute("ViewOldUploads", "File/ViewOldUploads/{id}", ...)
app.MapControllerRoute(
    name: "ViewOldUploads",
    pattern: "File/ViewOldUploads/{id?}",
    defaults: new { controller = "File", action = "ViewOldUpload" });

// Default conventional MVC route  →  {controller=Home}/{action=Index}/{id?}
// Replaces: routes.MapRoute("Default", "{controller}/{action}/{id}",
//               new { controller = "Home", action = "Index", id = UrlParameter.Optional })
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Run ──────────────────────────────────────────────────────────────────────
app.Run();
