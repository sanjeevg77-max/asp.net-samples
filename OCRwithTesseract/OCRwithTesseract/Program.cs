var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registrations
// Replaces: AreaRegistration.RegisterAllAreas()  (areas discovered via convention)
//           FilterConfig.RegisterGlobalFilters() (HandleErrorAttribute -> UseExceptionHandler)
//           RouteConfig.RegisterRoutes()         (see app.MapControllerRoute below)
//           BundleConfig.RegisterBundles()       (replaced by app.UseStaticFiles)
// ---------------------------------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    // Global exception-handling filter equivalent (HandleErrorAttribute)
    // Errors are handled via the middleware pipeline below
});

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
    // Replaces HandleErrorAttribute / FilterConfig.RegisterGlobalFilters
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days.
    // Adjust for production scenarios: https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}

app.UseHttpsRedirection();

// Replaces BundleConfig.RegisterBundles — serves CSS, JS and other static assets
// from the wwwroot folder without any bundling/minification pipeline
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ---------------------------------------------------------------------------
// Routing — replaces RouteConfig.RegisterRoutes
// Default route: {controller=Home}/{action=Index}/{id?}
// Areas are discovered automatically via the convention registered above
// ---------------------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
