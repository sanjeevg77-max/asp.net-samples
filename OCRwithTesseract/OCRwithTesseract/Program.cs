var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Service registrations
// Replaces: FilterConfig.RegisterGlobalFilters  — HandleErrorAttribute is
//   superseded by the UseExceptionHandler middleware below.
// Replaces: BundleConfig.RegisterBundles        — System.Web.Optimization
//   bundles are not available in .NET 8; static assets are served directly
//   via UseStaticFiles().
// Replaces: AreaRegistration.RegisterAllAreas() — no areas in this project.
// -----------------------------------------------------------------------
builder.Services.AddControllersWithViews();

var app = builder.Build();

// -----------------------------------------------------------------------
// Middleware pipeline
// Order matches the legacy ASP.NET MVC pipeline behaviour.
// -----------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    // Replaces: customErrors / HandleErrorAttribute from FilterConfig
    app.UseExceptionHandler("/Home/Error");
    // HSTS — recommended for production HTTPS deployments
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Serves files from wwwroot/; replaces BundleConfig static-asset bundles.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// -----------------------------------------------------------------------
// Routing
// Replaces: RouteConfig.RegisterRoutes — legacy default route:
//   url: "{controller}/{action}/{id}"
//   defaults: controller=Home, action=Index, id=UrlParameter.Optional
// -----------------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
