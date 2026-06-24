using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebAppMvcIdentity.Models;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------
// Configuration – connection string migrated from Web.config <connectionStrings>
// -------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// -------------------------------------------------------------------------
// Service registrations
// -------------------------------------------------------------------------

// Entity Framework Core (replaces EF 6 / ApplicationDbContext constructor-string approach)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ASP.NET Core Identity (replaces OWIN-based Microsoft.AspNet.Identity.*)
// Migrated from: App_Start/IdentityConfig.cs ApplicationUserManager / ApplicationSignInManager
//                App_Start/Startup.Auth.cs  app.CreatePerOwinContext<>()
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password policy – migrated from IdentityConfig.cs PasswordValidator
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;  // was RequireNonLetterOrDigit
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;

        // Lockout policy – migrated from IdentityConfig.cs lockout defaults
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;

        // User policy – migrated from IdentityConfig.cs UserValidator
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = string.Empty; // allow non-alphanumeric (AllowOnlyAlphanumericUserNames = false)
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();  // replaces DataProtectorTokenProvider registration

// Authentication / Cookie configuration
// Migrated from: App_Start/Startup.Auth.cs app.UseCookieAuthentication / app.UseExternalSignInCookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.SlidingExpiration = true;
    // Security-stamp re-validation every 30 minutes
    // (was: SecurityStampValidator.OnValidateIdentity validateInterval: TimeSpan.FromMinutes(30))
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

// External cookie for third-party logins
// Migrated from: app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)
builder.Services.AddAuthentication()
    .AddCookie(IdentityConstants.ExternalScheme);
    // Uncomment and configure client-id / secret to re-enable external providers:
    // .AddGoogle(options => { options.ClientId = ""; options.ClientSecret = ""; })
    // .AddMicrosoftAccount(options => { options.ClientId = ""; options.ClientSecret = ""; })
    // .AddTwitter(options => { options.ConsumerKey = ""; options.ConsumerSecret = ""; })
    // .AddFacebook(options => { options.AppId = ""; options.AppSecret = ""; })

// MVC with global filters
// Migrated from: App_Start/FilterConfig.cs  filters.Add(new HandleErrorAttribute())
//                Global.asax.cs             FilterConfig.RegisterGlobalFilters()
builder.Services.AddControllersWithViews(options =>
{
    // HandleErrorAttribute equivalent: the built-in exception handler middleware below
    // (app.UseExceptionHandler) covers unhandled exceptions application-wide.
    // No additional filter registration required in ASP.NET Core.
});

// -------------------------------------------------------------------------
// Build the application
// -------------------------------------------------------------------------
var app = builder.Build();

// -------------------------------------------------------------------------
// Middleware pipeline
// Ordering mirrors the legacy System.Web + OWIN pipeline:
//   Error handling -> HTTPS -> Static files -> Routing -> Auth -> Endpoints
// -------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Replaces: FilterConfig HandleErrorAttribute for unhandled exceptions
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Replaces: IIS / system.webServer HTTPS enforcement
app.UseHttpsRedirection();

// Replaces: BundleConfig / System.Web.Optimization static asset serving
// Static files are served directly from wwwroot (Bootstrap, jQuery, etc. already copied there)
app.UseStaticFiles();

// Replaces: RouteConfig.RegisterRoutes() IgnoreRoute / MapRoute pipeline
app.UseRouting();

// Replaces: OWIN app.UseCookieAuthentication / app.UseExternalSignInCookie
app.UseAuthentication();

// Replaces: [Authorize] filter processing in System.Web MVC pipeline
app.UseAuthorization();

// Conventional MVC route – migrated from App_Start/RouteConfig.cs:
//   routes.MapRoute(name: "Default", url: "{controller}/{action}/{id}",
//                   defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional })
// Area registration (AreaRegistration.RegisterAllAreas()) is handled automatically
// by MapControllerRoute in ASP.NET Core.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
