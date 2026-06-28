// ----------------------------------------------------------------------------------
// Program.cs – ASP.NET Core 8 entry point
//
// Migrated from:
//   • Global.asax / Global.asax.cs  (Application_Start)
//   • Startup.cs (OWIN OwinStartupAttribute + Configuration(IAppBuilder))
//   • App_Start/Startup.Auth.cs     (ConfigureAuth – cookie & external-login OWIN middleware)
//   • App_Start/RouteConfig.cs      (conventional MVC route "Default")
//   • App_Start/FilterConfig.cs     (HandleErrorAttribute global filter)
//   • App_Start/BundleConfig.cs     (removed – bundling/minification handled by build tools
//                                    in .NET 8; static assets are served from wwwroot/)
// ----------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebAppMvcIdentity.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. Service registrations
//    (replaces Global.asax Application_Start + OWIN app.CreatePerOwinContext
//     calls for ApplicationDbContext, ApplicationUserManager, ApplicationSignInManager)
// ---------------------------------------------------------------------------

// -- Database context -------------------------------------------------------
// Replaces: app.CreatePerOwinContext(ApplicationDbContext.Create)
// The connection string is read from appsettings.json "DefaultConnection" key,
// which mirrors the <connectionStrings> entry in Web.config.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty));

// -- ASP.NET Core Identity --------------------------------------------------
// Replaces:
//   app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create)
//   app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create)
//   app.UseCookieAuthentication(new CookieAuthenticationOptions { ... })
//   app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)
//   app.UseTwoFactorSignInCookie(...)  ← built-in with AddIdentity
//   app.UseTwoFactorRememberBrowserCookie(...)  ← built-in with AddIdentity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password policy from IdentityConfig.cs PasswordValidator settings
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;

        // Lockout policy from IdentityConfig.cs UserLockoutEnabledByDefault / DefaultAccountLockoutTimeSpan
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // Require unique email (UserValidator.RequireUniqueEmail = true)
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// -- Cookie authentication options ------------------------------------------
// Replaces: app.UseCookieAuthentication(new CookieAuthenticationOptions
//           {
//               AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
//               LoginPath = new PathString("/Account/Login"),
//               Provider = new CookieAuthenticationProvider
//               {
//                   OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<...>(validateInterval: 30 min)
//               }
//           });
// Note: AddIdentity already registers cookie authentication internally.
//       ConfigureApplicationCookie is the correct way to customise it in ASP.NET Core.
builder.Services.ConfigureApplicationCookie(options =>
{
    // Map DefaultAuthenticationTypes.ApplicationCookie → CookieAuthenticationDefaults.AuthenticationScheme
    options.LoginPath = "/Account/Login";
    options.SlidingExpiration = true;
    // SecurityStamp validation interval (was 30 minutes in OnValidateIdentity)

});

// -- MVC / Controllers with Views -------------------------------------------
// Replaces: AreaRegistration.RegisterAllAreas() + RouteConfig / FilterConfig bootstrapping.
// HandleErrorAttribute (FilterConfig) is the equivalent of the built-in exception handler;
// UseExceptionHandler middleware covers that at the pipeline level (see below).
builder.Services.AddControllersWithViews(options =>
{
    // Replaces: filters.Add(new HandleErrorAttribute()) in FilterConfig.RegisterGlobalFilters
    // ASP.NET Core's exception-handling middleware (UseExceptionHandler) is the
    // preferred replacement; no explicit HandleErrorAttribute needed.
});

// ---------------------------------------------------------------------------
// 2. Build the application
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// 3. Middleware pipeline
//    Order matters – mirrors the conventional OWIN/MVC pipeline order.
// ---------------------------------------------------------------------------

// -- Exception handling -----------------------------------------------------
// Replaces: HandleErrorAttribute global filter (FilterConfig.cs)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // /Home/Error is the standard MVC error view (Views/Shared/Error.cshtml)
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// -- HTTPS redirection ------------------------------------------------------
app.UseHttpsRedirection();

// -- Static files -----------------------------------------------------------
// Replaces: BundleConfig.RegisterBundles() – static assets (JS/CSS) are now
// served directly from wwwroot/ without bundling middleware.
// Script and style bundles (jquery, bootstrap, modernizr, site.css) have already
// been copied to wwwroot/Scripts/ and wwwroot/Content/ during migration.
app.UseStaticFiles();

// -- Routing ----------------------------------------------------------------
app.UseRouting();

// -- Authentication & Authorization -----------------------------------------
// Replaces: app.UseCookieAuthentication / app.UseExternalSignInCookie /
//           app.UseTwoFactorSignInCookie / app.UseTwoFactorRememberBrowserCookie
// UseAuthentication must come BEFORE UseAuthorization.
app.UseAuthentication();
app.UseAuthorization();

// -- Endpoint routing -------------------------------------------------------
// Replaces: RouteConfig.RegisterRoutes(RouteTable.Routes)
//   Original route:
//     name: "Default"
//     url:  "{controller}/{action}/{id}"
//     defaults: controller = "Home", action = "Index", id = UrlParameter.Optional
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------------------------------------------------------------------------
// 4. Run the application
// ---------------------------------------------------------------------------
app.Run();
