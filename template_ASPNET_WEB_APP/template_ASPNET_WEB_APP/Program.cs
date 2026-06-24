// -------------------------------------------------------------------
// Program.cs — .NET 8.0 Minimal Hosting Entry Point
// Migrated from Global.asax / Global.asax.cs (ASP.NET MVC 5 / .NET 4.7.2)
//
// Migration summary:
//   AreaRegistration.RegisterAllAreas()            → builder.Services.AddControllersWithViews()
//                                                     + app.MapAreaControllerRoute() (see area route below)
//   FilterConfig.RegisterGlobalFilters()           → AddControllersWithViews(options => options.Filters.Add(...))
//   RouteConfig.RegisterRoutes()                   → app.MapControllerRoute() default + ignore .axd handled
//                                                     automatically (not present in ASP.NET Core pipeline)
//   BundleConfig.RegisterBundles()                 → NOT applicable in .NET 8.0;
//                                                     System.Web.Optimization is removed;
//                                                     static assets (Bootstrap, jQuery, etc.) are served
//                                                     directly via app.UseStaticFiles().
//                                                     Consider LibMan / npm / WebOptimizer for bundling.
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. CONFIGURATION
//    appsettings.json is loaded automatically by WebApplication.CreateBuilder.
//    Web.config is no longer the primary configuration mechanism in .NET 8.0.
// ---------------------------------------------------------------------------
// builder.Configuration is pre-wired to appsettings.json, environment variables,
// command-line args, and appsettings.{Environment}.json.

// ---------------------------------------------------------------------------
// 2. SERVICE REGISTRATIONS
// ---------------------------------------------------------------------------

// AddControllersWithViews registers MVC controller + Razor View support.
// Global filters previously registered in FilterConfig.RegisterGlobalFilters()
// are moved here via options.Filters.Add(...).
//
// Original FilterConfig:
//   filters.Add(new HandleErrorAttribute());
//
// In ASP.NET Core the built-in exception-handling middleware replaces
// HandleErrorAttribute. UseExceptionHandler / UseDeveloperExceptionPage
// are used instead (see middleware pipeline section below).
// If a custom global exception-filter attribute is still required it can be
// added back with: options.Filters.Add<YourCustomExceptionFilter>()
builder.Services.AddControllersWithViews(options =>
{
    // HandleErrorAttribute is a System.Web.Mvc concern and does not exist in
    // ASP.NET Core. Exception handling is performed by middleware (see below).
    // Add any custom IActionFilter / IExceptionFilter implementations here:
    // options.Filters.Add<YourCustomExceptionFilter>();

    // Client-side validation support (was ClientValidationEnabled=true in Web.config)
    // is enabled by default in ASP.NET Core tag helpers / unobtrusive validation.
    // No explicit option is required.
});

// ---------------------------------------------------------------------------
// 3. BUILD THE APPLICATION
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// 4. MIDDLEWARE PIPELINE
//    Correct ASP.NET Core ordering:
//      Exception handling → HSTS/HTTPS → Static files → Routing →
//      Authentication → Authorization → Endpoints (MapControllerRoute)
// ---------------------------------------------------------------------------

// Exception handling
// Replaces FilterConfig's HandleErrorAttribute (global error filter).
if (app.Environment.IsDevelopment())
{
    // Shows detailed exception pages during development.
    app.UseDeveloperExceptionPage();
}
else
{
    // Mirrors HandleErrorAttribute behaviour in production:
    // redirects to /Home/Error on unhandled exceptions.
    app.UseExceptionHandler("/Home/Error");

    // Enforce HTTPS Strict Transport Security in production.
    app.UseHsts();
}

// Redirect HTTP → HTTPS
app.UseHttpsRedirection();

// ---------------------------------------------------------------------------
// Static files (wwwroot)
// Replaces BundleConfig.RegisterBundles() for serving CSS/JS/images.
//
// Original BundleConfig bundles:
//   ~/bundles/jquery        → Scripts/jquery-{version}.js
//   ~/bundles/jqueryval     → Scripts/jquery.validate*
//   ~/bundles/modernizr     → Scripts/modernizr-*
//   ~/bundles/bootstrap     → Scripts/bootstrap.js
//   ~/Content/css           → Content/bootstrap.css, Content/site.css
//
// These files are now served directly from the wwwroot folder (or Content /
// Scripts via UseStaticFiles with a FileProvider if you keep the legacy layout).
// For bundling & minification in .NET 8.0 consider:
//   - LibMan (client-side library manager)
//   - WebOptimizer (NuGet: LigerShark.WebOptimizer.Core)
//   - npm + a build tool (webpack, esbuild, etc.)
// ---------------------------------------------------------------------------
app.UseStaticFiles();

// ---------------------------------------------------------------------------
// Routing middleware
// Enables endpoint routing used by MapControllerRoute below.
// ---------------------------------------------------------------------------
app.UseRouting();

// Authentication & authorisation (add UseAuthentication() here if auth is needed)
// app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// 5. ENDPOINT MAPPING (replaces RouteConfig.RegisterRoutes)
//
// Original RouteConfig:
//   routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
//   routes.MapRoute(
//       name:     "Default",
//       url:      "{controller}/{action}/{id}",
//       defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
//   );
//
// Notes:
//   • IgnoreRoute for *.axd is unnecessary in ASP.NET Core — HTTP handlers
//     (.axd endpoints) do not exist in the Core pipeline.
//   • AreaRegistration.RegisterAllAreas() is replaced by app.MapAreaControllerRoute()
//     per area, or by placing [Area("AreaName")] on area controllers and adding
//     the area convention via AddControllersWithViews().
// ---------------------------------------------------------------------------

// Area route — uncomment and repeat for each MVC area in the project.
// app.MapAreaControllerRoute(
//     name:      "MyArea",
//     areaName:  "MyArea",
//     pattern:   "MyArea/{controller=Home}/{action=Index}/{id?}");

// Default conventional MVC route (mirrors RouteConfig "Default" route)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------------------------------------------------------------------------
// 6. START THE APPLICATION
// ---------------------------------------------------------------------------
app.Run();
