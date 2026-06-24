// ----------------------------------------------------------------------------
// Program.cs – .NET 8 minimal hosting model
// Migrated from Global.asax / Startup.cs for WebApplication5DemoDTjs
//
// Migration notes:
//   • AreaRegistration.RegisterAllAreas()         → AddControllersWithViews() (areas
//                                                    discovered automatically via
//                                                    MapAreaControllerRoute or
//                                                    [Area] attribute routing)
//   • FilterConfig (HandleErrorAttribute)         → app.UseExceptionHandler("/Home/Error")
//                                                    registered via AddControllersWithViews()
//   • RouteConfig.RegisterRoutes()                → app.MapControllerRoute() default route
//   • BundleConfig (System.Web.Optimization)      → NOT available in .NET 8;
//                                                    static assets served from wwwroot via
//                                                    app.UseStaticFiles()
//   • Connection strings                          → appsettings.json ConnectionStrings section
//   • webpages:Version / webpages:Enabled         → removed (WebPages not used in .NET 8)
//   • ClientValidationEnabled / UnobtrusiveJS     → retained in appsettings.json as AppSettings
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApplication5DemoDTjs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ----------------------------------------------------------------
            // 1. Create the WebApplication builder
            // ----------------------------------------------------------------
            var builder = WebApplication.CreateBuilder(args);

            // ----------------------------------------------------------------
            // 2. Register services
            //
            //    AddControllersWithViews() covers:
            //      • MVC controllers + Razor views
            //      • Area support (areas are discovered automatically; use
            //        MapAreaControllerRoute or [Area] attributes in controllers)
            //      • Global exception-handling filter equivalent:
            //        HandleErrorAttribute is replaced by the exception-handler
            //        middleware configured below (app.UseExceptionHandler)
            // ----------------------------------------------------------------
            builder.Services.AddControllersWithViews(options =>
            {
                // HandleErrorAttribute equivalent: production error handling is
                // handled by the UseExceptionHandler middleware below.
                // Add any additional global action filters here if required.
            });

            // ----------------------------------------------------------------
            // 3. Build the application
            // ----------------------------------------------------------------
            var app = builder.Build();

            // ----------------------------------------------------------------
            // 4. Configure the middleware pipeline
            //    Order matters – follow the recommended ASP.NET Core ordering.
            // ----------------------------------------------------------------

            // 4a. Exception handling / HSTS
            //     Replaces HandleErrorAttribute from FilterConfig.RegisterGlobalFilters()
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Mirrors HandleErrorAttribute: redirects unhandled exceptions to /Home/Error
                app.UseExceptionHandler("/Home/Error");
                // Enforce HTTPS Strict Transport Security in production
                app.UseHsts();
            }

            // 4b. Redirect HTTP → HTTPS
            app.UseHttpsRedirection();

            // 4c. Serve static files from wwwroot
            //     Replaces BundleConfig / System.Web.Optimization:
            //     scripts and styles are referenced directly from wwwroot/Scripts
            //     and wwwroot/Content without bundling.
            app.UseStaticFiles();

            // 4d. Routing
            app.UseRouting();

            // 4e. Authorization
            app.UseAuthorization();

            // ----------------------------------------------------------------
            // 5. Endpoint mapping
            //
            //    Replaces RouteConfig.RegisterRoutes() / RouteTable.Routes:
            //      • Default conventional route mirrors the legacy
            //        "{controller}/{action}/{id}" pattern with optional id.
            //      • IgnoreRoute("{resource}.axd/{*pathInfo}") is not needed
            //        in ASP.NET Core (.axd HTTP handlers do not exist).
            //      • Area routes: add MapAreaControllerRoute calls before the
            //        default route for any named areas, or rely on [Area]
            //        attribute routing on area controllers.
            // ----------------------------------------------------------------
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // ----------------------------------------------------------------
            // 6. Run the application
            // ----------------------------------------------------------------
            app.Run();
        }
    }
}
