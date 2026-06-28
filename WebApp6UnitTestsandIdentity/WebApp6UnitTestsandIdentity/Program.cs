// -----------------------------------------------------------------------
// Program.cs — .NET 8 Minimal Hosting Entry Point
//
// Migrated from:
//   • Global.asax / Global.asax.cs  (MvcApplication : HttpApplication)
//   • Startup.cs (OWIN)             (OwinStartupAttribute / ConfigureAuth)
//   • App_Start/Startup.Auth.cs     (OWIN cookie & external-login setup)
//   • App_Start/RouteConfig.cs      (RouteTable / MapRoute)
//   • App_Start/FilterConfig.cs     (GlobalFilters / HandleErrorAttribute)
//   • App_Start/BundleConfig.cs     (System.Web.Optimization bundles — NOT
//                                    supported natively in .NET 8; static
//                                    assets are served from wwwroot via
//                                    UseStaticFiles. Replace bundling with
//                                    LibMan / npm / WebPack as needed.)
// -----------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApp6UnitTestsandIdentity.Models;

// ---------------------------------------------------------------------------
// 1. Builder — equivalent to the old Application_Start + ConfigureServices
// ---------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 2. MVC controllers with views
//    Replaces: FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
//    The legacy HandleErrorAttribute is superseded by the built-in exception-
//    handling middleware; we add it as a global filter for backward-compat.
// ---------------------------------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    // Migrated from FilterConfig.RegisterGlobalFilters:
    //   filters.Add(new HandleErrorAttribute());
    // In ASP.NET Core the error-handler middleware (UseExceptionHandler /
    // UseDeveloperExceptionPage) is the idiomatic replacement.
    // ExceptionFilterAttribute is abstract and cannot be instantiated directly;
    // exception handling is handled by UseExceptionHandler / UseDeveloperExceptionPage
    // middleware configured below in the pipeline.
});

// ---------------------------------------------------------------------------
// 3. Entity Framework Core — replaces EF6 ApplicationDbContext
//    Connection string migrated from Web.config <connectionStrings> into
//    appsettings.json["ConnectionStrings:DefaultConnection"]
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------------------
// 4. ASP.NET Core Identity
//    Replaces OWIN pipeline registrations from Startup.Auth.cs:
//      app.CreatePerOwinContext(ApplicationDbContext.Create)
//      app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create)
//      app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create)
//      app.UseCookieAuthentication(new CookieAuthenticationOptions { ... })
//      app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)
//      app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, ...)
//      app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie)
//
//    ApplicationUserManager<ApplicationUser> and
//    SignInManager<ApplicationUser> are resolved from DI automatically;
//    controllers should inject them via constructor injection.
// ---------------------------------------------------------------------------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // --- Password policy (migrated from ApplicationUserManager.Create) ---
        options.Password.RequiredLength         = 6;
        options.Password.RequireNonAlphanumeric = true;   // RequireNonLetterOrDigit
        options.Password.RequireDigit           = true;
        options.Password.RequireLowercase       = true;
        options.Password.RequireUppercase       = true;

        // --- Lockout policy (migrated from ApplicationUserManager.Create) ----
        options.Lockout.AllowedForNewUsers      = true;   // UserLockoutEnabledByDefault
        options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;

        // --- User validation (migrated from UserValidator settings) ----------
        options.User.RequireUniqueEmail          = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ---------------------------------------------------------------------------
// 5. Cookie authentication options
//    Migrated from CookieAuthenticationOptions in Startup.Auth.cs:
//      LoginPath    = "/Account/Login"
//      SecurityStampValidator.OnValidateIdentity (validateInterval: 30 min)
//    Two-factor cookies are handled by Identity's built-in infrastructure.
// ---------------------------------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath         = new PathString("/Account/Login");
    options.SlidingExpiration = true;
});

// Equivalent to SecurityStampValidator.OnValidateIdentity validateInterval
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});

// ---------------------------------------------------------------------------
// NOTE — External login providers (Google, Facebook, Twitter, Microsoft)
// were commented out in the original Startup.Auth.cs. To re-enable them
// add the appropriate packages and configure below, e.g.:
//
//   builder.Services.AddAuthentication()
//       .AddGoogle(o => { o.ClientId = "..."; o.ClientSecret = "..."; })
//       .AddFacebook(o => { o.AppId = "..."; o.AppSecret = "..."; })
//       .AddTwitter(o => { o.ConsumerKey = "..."; o.ConsumerSecret = "..."; })
//       .AddMicrosoftAccount(o => { o.ClientId = "..."; o.ClientSecret = "..."; });
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// NOTE — BundleConfig.RegisterBundles (System.Web.Optimization)
// Bundling is NOT natively supported in .NET 8. The wwwroot/Scripts and
// wwwroot/Content folders are served as static files via UseStaticFiles().
// Use LibMan, npm/Webpack, or the WebOptimizer NuGet package as a replacement.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// 6. Build the application
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// 7. Middleware pipeline
//    Ordering follows ASP.NET Core conventions and mirrors the intent of the
//    original Global.asax Application_Start + OWIN Startup pipeline.
// ---------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    // Replaces the old customErrors / compilation debug="true" in Web.config
    app.UseDeveloperExceptionPage();
}
else
{
    // Replaces <customErrors mode="On" defaultRedirect="~/Views/Shared/Error"/>
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS redirection — .NET 8 best-practice; remove if certs are not available
// (see note in Startup.cs CTA comment re Kestrel config in appsettings.json).
app.UseHttpsRedirection();

// Serves files from wwwroot — replaces BundleConfig static-file handling
// and the old ~/Content / ~/Scripts virtual paths.
app.UseStaticFiles();

// Routing must come before authentication/authorisation
app.UseRouting();

// ---------------------------------------------------------------------------
// 8. Authentication & Authorisation middleware
//    Replaces OWIN UseCookieAuthentication / UseExternalSignInCookie /
//    UseTwoFactorSignInCookie / UseTwoFactorRememberBrowserCookie
//    order: Authentication → Authorisation
// ---------------------------------------------------------------------------
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// 9. Endpoint routing
//    Replaces: RouteConfig.RegisterRoutes(RouteTable.Routes)
//              routes.MapRoute(name: "Default", url: "{controller}/{action}/{id}",
//                              defaults: new { controller="Home", action="Index",
//                                             id=UrlParameter.Optional })
//
//    Also replaces: AreaRegistration.RegisterAllAreas()
//    Areas are discovered automatically in ASP.NET Core; use
//    MapAreaControllerRoute for explicit area routes if required.
// ---------------------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------------------------------------------------------------------------
// 10. Run the application
// ---------------------------------------------------------------------------
app.Run();
