/* Added by CTA: Please add the corresponding references.
   If certs are not provided for deployment, communication will be on http;
   please remove the https section of the kestrel config in appsettings.json
   and also remove middleware component app.UseHttpsRedirection().
   ASP.NET Core Identity replaces OWIN/Katana cookie authentication and
   ApplicationUserManager / ApplicationSignInManager from IdentityConfig.cs.
   BundleConfig is replaced by static file serving (wwwroot).
   FilterConfig's HandleErrorAttribute is replaced by UseExceptionHandler. */

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApp5Identity.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Service registrations
// -----------------------------------------------------------------------

// Register ApplicationDbContext with EF Core using the DefaultConnection
// connection string from appsettings.json.
// Replaces: ApplicationDbContext.Create() OWIN per-request factory and
//           the EntityFramework 6 / System.Data.Entity configuration in
//           Web.config <entityFramework> section.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register ASP.NET Core Identity.
// Replaces: ApplicationUserManager and ApplicationSignInManager created via
//           app.CreatePerOwinContext<T>() in App_Start/Startup.Auth.cs.
// Password rules mirror IdentityConfig.cs PasswordValidator settings:
//   RequiredLength=6, RequireNonLetterOrDigit, RequireDigit,
//   RequireLowercase, RequireUppercase.
// Lockout settings mirror IdentityConfig.cs UserLockout settings:
//   MaxFailedAccessAttempts=5, DefaultLockoutTimeSpan=5 min.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password policy - matches original PasswordValidator
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;

        // Lockout policy - matches original UserLockout config
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User policy - matches original UserValidator config
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure application cookie.
// Replaces: app.UseCookieAuthentication(new CookieAuthenticationOptions { ... })
//           and the SecurityStampValidator.OnValidateIdentity callback
//           from App_Start/Startup.Auth.cs.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.SlidingExpiration = true;
    // Security stamp validation every 30 minutes mirrors the original
    // validateInterval: TimeSpan.FromMinutes(30) in CookieAuthenticationProvider.
});

// Two-factor authentication support.
// Replaces: app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5))
//           and app.UseTwoFactorRememberBrowserCookie() in Startup.Auth.cs.
// ASP.NET Core Identity handles two-factor cookie management automatically
// through AddIdentity and the built-in TwoFactorUserIdScheme / TwoFactorRememberMeScheme.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});

// Add MVC controllers with views.
// Replaces: AreaRegistration.RegisterAllAreas() +
//           RouteConfig.RegisterRoutes(RouteTable.Routes) from Global.asax.
// Global error handling (HandleErrorAttribute from FilterConfig) is
// replaced by the UseExceptionHandler middleware registered below.
builder.Services.AddControllersWithViews();

// Expose IConfiguration globally for any remaining ConfigurationManager usages.
WebApp5Identity.ConfigurationManager.Configuration = builder.Configuration;

var app = builder.Build();

// -----------------------------------------------------------------------
// Middleware pipeline
// -----------------------------------------------------------------------

// Replaces: FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
//           which registered HandleErrorAttribute.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Redirects HTTP to HTTPS.
app.UseHttpsRedirection();

// Serves files from wwwroot.
// Replaces: BundleConfig.RegisterBundles() - static assets are now served
//           directly from wwwroot/Content and wwwroot/Scripts.
app.UseStaticFiles();

// Enables routing.
app.UseRouting();

// Enables ASP.NET Core Identity cookie authentication.
// Replaces: app.UseCookieAuthentication + app.UseExternalSignInCookie
//           from App_Start/Startup.Auth.cs.
app.UseAuthentication();

// Enables authorisation (must come after UseAuthentication).
app.UseAuthorization();

// Map conventional MVC route.
// Replaces: RouteConfig.RegisterRoutes() from App_Start/RouteConfig.cs
//   routes.IgnoreRoute("{resource}.axd/{*pathInfo}") - not needed in Core.
//   routes.MapRoute("Default", "{controller}/{action}/{id}",
//       defaults: { controller="Home", action="Index", id=Optional })
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// -----------------------------------------------------------------------
// Helpers retained for backward compatibility with existing code that
// references WebApp5Identity.ConfigurationManager.Configuration.
// -----------------------------------------------------------------------
namespace WebApp5Identity
{
    public class ConfigurationManager
    {
        public static IConfiguration Configuration { get; set; } = default!;
    }
}
