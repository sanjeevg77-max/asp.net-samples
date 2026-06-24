// Migrated from Global.asax / WebApiConfig to .NET 8.0 minimal hosting pattern.
// Original Global.asax.cs registered:
//   - GlobalConfiguration.Configure(WebApiConfig.Register)  -> builder.Services.AddControllers() + app.MapControllers()
//   - AreaRegistration.RegisterAllAreas()                   -> app.MapAreaControllerRoute()
// Original WebApiConfig.Register registered:
//   - config.MapHttpAttributeRoutes()                       -> app.MapControllers() (attribute routing on by default)
//   - config.Routes.MapHttpRoute "DefaultApi" (api/{controller}/{id}) -> app.MapControllerRoute("DefaultApi", ...)

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registrations
// Replaces: GlobalConfiguration.Configure(WebApiConfig.Register)
//           services.AddControllersWithViews() from legacy Startup.cs
// ---------------------------------------------------------------------------
builder.Services.AddControllers();          // Web API controllers (ApiController-style)
builder.Services.AddControllersWithViews(); // MVC controllers + Views (Areas support)

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// Route registration
// Replaces: config.MapHttpAttributeRoutes()  - attribute routing is enabled by
//           default when MapControllers() is called.
// Replaces: config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}")
// ---------------------------------------------------------------------------

// Web API default route: api/{controller}/{id?}
app.MapControllerRoute(
    name: "DefaultApi",
    pattern: "api/{controller}/{id?}");

// Replaces: AreaRegistration.RegisterAllAreas()
// Area route - matches area controllers placed under /Areas/{areaName}/Controllers/
app.MapAreaControllerRoute(
    name: "Areas",
    areaName: "{area:exists}",
    pattern: "{area}/{controller=Home}/{action=Index}/{id?}");

// Conventional MVC fallback route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Attribute-routed API controllers (MapHttpAttributeRoutes equivalent)
app.MapControllers();

app.Run();
