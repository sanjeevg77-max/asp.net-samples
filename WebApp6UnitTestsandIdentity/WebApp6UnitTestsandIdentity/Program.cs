// Migrated from Global.asax / Global.asax.cs (MvcApplication.Application_Start) and
// OWIN Startup.cs / App_Start/Startup.Auth.cs to .NET 8.0 WebApplication minimal hosting model.
//
// Key migrations performed:
//   - AreaRegistration.RegisterAllAreas()          -> handled automatically by AddControllersWithViews()
//   - FilterConfig.RegisterGlobalFilters()         -> builder.Services.AddControllersWithViews(options => ...)
//   - RouteConfig.RegisterRoutes()                 -> app.MapControllerRoute()
//   - BundleConfig.RegisterBundles()               -> REMOVED (System.Web.Optimization not supported in .NET 8;
//                                                     reference static files directly via <link>/<script> tags)
//   - OWIN app.CreatePerOwinContext(ApplicationDbContext.Create)     -> builder.Services.AddDbContext<ApplicationDbContext>()
//   - OWIN app.CreatePerOwinContext<ApplicationUserManager>(...)     -> builder.Services.AddIdentity<>()
//   - OWIN app.CreatePerOwinContext<ApplicationSignInManager>(...)   -> builder.Services.AddIdentity<>()
//   - OWIN app.UseCookieAuthentication(CookieAuthenticationOptions)  -> builder.Services.AddAuthentication().AddCookie()
//   - OWIN app.UseExternalSignInCookie()           -> AddAuthentication().AddCookie() external scheme below
//   - OWIN app.UseTwoFactorSignInCookie()          -> built-in to ASP.NET Core Identity (no explicit call needed)
//   - OWIN app.UseTwoFactorRememberBrowserCookie() -> built-in to ASP.NET Core Identity (no explicit call needed)
//   - External OAuth providers (Microsoft, Twitter, Facebook, Google) -> commented placeholders below

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApp6UnitTestsandIdentity.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. DATABASE CONTEXT
//    Replaces: app.CreatePerOwinContext(ApplicationDbContext.Create)
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------------------
// 2. ASP.NET CORE IDENTITY
//    Replaces: app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create)
//              app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create)
// ---------------------------------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password settings migrated from PasswordValidator in IdentityConfig.cs
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;

        // Lockout settings migrated from ApplicationUserManager.Create in IdentityConfig.cs
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings migrated from UserValidator in IdentityConfig.cs
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ---------------------------------------------------------------------------
// 3. COOKIE AUTHENTICATION
//    Replaces: app.UseCookieAuthentication(new CookieAuthenticationOptions { ... })
//              app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)
//    Note: Two-factor sign-in and remember-browser cookies are handled internally
//          by ASP.NET Core Identity and do not require explicit registration.
// ---------------------------------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.SlidingExpiration = true;
});

// Security stamp validation interval (was OnValidateIdentity with 30-minute interval)
builder.Services.Configure<Microsoft.AspNetCore.Identity.SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});

// ---------------------------------------------------------------------------
// 4. EXTERNAL OAUTH PROVIDERS
//    Replaces the commented-out OWIN provider registrations in Startup.Auth.cs.
//    Uncomment and supply real credentials to enable each provider.
// ---------------------------------------------------------------------------
// builder.Services.AddAuthentication()
//     .AddMicrosoftAccount(options =>
//     {
//         options.ClientId = "<clientId>";
//         options.ClientSecret = "<clientSecret>";
//     })
//     .AddTwitter(options =>
//     {
//         options.ConsumerKey = "<consumerKey>";
//         options.ConsumerSecret = "<consumerSecret>";
//     })
//     .AddFacebook(options =>
//     {
//         options.AppId = "<appId>";
//         options.AppSecret = "<appSecret>";
//     })
//     .AddGoogle(options =>
//     {
//         options.ClientId = "<clientId>";
//         options.ClientSecret = "<clientSecret>";
//     });

// ---------------------------------------------------------------------------
// 5. MVC SERVICES + GLOBAL FILTERS
//    Replaces: AreaRegistration.RegisterAllAreas()  (automatic in ASP.NET Core)
//              FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
//                -> filters.Add(new HandleErrorAttribute()) is covered by
//                   app.UseExceptionHandler("/Home/Error") in the pipeline below.
// ---------------------------------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    // HandleErrorAttribute equivalent: errors are handled via UseExceptionHandler middleware.
    // Add any additional global action/result/exception filters here, e.g.:
    // options.Filters.Add<MyCustomFilter>();
});

// ---------------------------------------------------------------------------
// BUILD
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// 6. MIDDLEWARE PIPELINE
// ---------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    // Replaces: FilterConfig HandleErrorAttribute for production error handling
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Serves static files directly (replaces System.Web.Optimization BundleConfig -
// reference CSS/JS files via <link>/<script> tags pointing to wwwroot paths).
app.UseStaticFiles();

app.UseRouting();

// Replaces: OWIN app.UseCookieAuthentication / app.UseExternalSignInCookie
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// 7. ROUTING
//    Replaces: RouteConfig.RegisterRoutes(RouteTable.Routes)
//      routes.IgnoreRoute("{resource}.axd/{*pathInfo}") -> not needed in ASP.NET Core
//      routes.MapRoute("Default", "{controller}/{action}/{id}",
//                      defaults: { controller="Home", action="Index", id=Optional })
// ---------------------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
