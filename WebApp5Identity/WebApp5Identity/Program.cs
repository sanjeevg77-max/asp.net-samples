// Program.cs — .NET 8 minimal hosting model
// Replaces: Global.asax / Global.asax.cs (Application_Start),
//           Startup.cs (IAppBuilder-based),
//           App_Start/Startup.Auth.cs (OWIN cookie / two-factor auth),
//           App_Start/RouteConfig.cs   (conventional routing),
//           App_Start/FilterConfig.cs  (global HandleErrorAttribute),
//           App_Start/BundleConfig.cs  (bundling — not supported in .NET 8)

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp5Identity.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Database / Entity Framework Core
// Replaces: ApplicationDbContext.Create() per-request OWIN context factory
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---------------------------------------------------------------------------
// ASP.NET Core Identity
// Replaces: app.CreatePerOwinContext<ApplicationUserManager>(...)
//           app.CreatePerOwinContext<ApplicationSignInManager>(...)
//           PasswordValidator / UserValidator configured in IdentityConfig.cs
//           UserLockoutEnabledByDefault / DefaultAccountLockoutTimeSpan /
//           MaxFailedAccessAttemptsBeforeLockout from IdentityConfig.cs
// ---------------------------------------------------------------------------
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Sign-in requirements
    options.SignIn.RequireConfirmedAccount = false;

    // Password policy — migrated from PasswordValidator in IdentityConfig.cs
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Lockout policy — migrated from ApplicationUserManager.Create() in IdentityConfig.cs
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ---------------------------------------------------------------------------
// Cookie Authentication
// Replaces: app.UseCookieAuthentication(new CookieAuthenticationOptions { ... })
//           from App_Start/Startup.Auth.cs — OWIN IAppBuilder pipeline
//           Removes: app.UseExternalSignInCookie (ExternalCookie)
//                    app.UseTwoFactorSignInCookie (TwoFactorCookie, 5 min)
//                    app.UseTwoFactorRememberBrowserCookie
//           SecurityStampValidator.OnValidateIdentity interval of 30 minutes
//           is replaced by options.SecurityStampValidationInterval below.
// ---------------------------------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/LogOff";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

});

// Replaces SecurityStampValidator.OnValidateIdentity validateInterval
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});

// ---------------------------------------------------------------------------
// MVC — Controllers + Views
// Replaces: AreaRegistration.RegisterAllAreas() — areas are auto-discovered
//           FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters) with
//           HandleErrorAttribute — UseExceptionHandler covers this in .NET 8
//           BundleConfig.RegisterBundles() — not applicable in .NET 8
// ---------------------------------------------------------------------------
builder.Services.AddControllersWithViews();

// ---------------------------------------------------------------------------
// Build the application
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// Replaces: HttpApplication lifecycle in Global.asax.cs / MvcApplication
// ---------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    // Replaces: FilterConfig HandleErrorAttribute for unhandled exceptions
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Replaces: Static file serving previously handled by IIS / BundleConfig assets
app.UseStaticFiles();

app.UseRouting();

// Replaces: OWIN UseCookieAuthentication / UseExternalSignInCookie /
//           UseTwoFactorSignInCookie / UseTwoFactorRememberBrowserCookie
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// Routing
// Replaces: RouteConfig.RegisterRoutes(RouteTable.Routes)
//   Original: routes.MapRoute("Default", "{controller}/{action}/{id}",
//             defaults: { controller="Home", action="Index", id=Optional })
// ---------------------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
