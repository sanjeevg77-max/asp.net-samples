using Microsoft.Extensions.DependencyInjection;
using MvcUnit.Models;

namespace MvcUnit
{
    /// <summary>
    /// Configures the ASP.NET Core DI container for the application.
    /// Migrated from .NET Framework (Unity 5.11.1 + Unity.Mvc5) to
/// .NET 8.0 using ASP.NET Core's built-in IServiceCollection.
    ///
    /// Unity.Mvc5 relied on System.Web and DependencyResolver.SetResolver() which
    /// are not available in .NET 8.0.  The equivalent integration point for
    /// ASP.NET Core is the native IServiceCollection / IServiceProvider.
    ///
    /// Call UnityConfig.RegisterComponents(services) from Program.cs after
    /// AddControllersWithViews() to register application-specific services.
    /// </summary>
    public static class UnityConfig
    {
        /// <summary>
        /// Registers all application components with the ASP.NET Core
        /// <see cref="IServiceCollection"/>.
        /// Call this from Program.cs before builder.Build().
        /// </summary>
        /// <param name="services">The ASP.NET Core service collection built up
        /// by WebApplicationBuilder (e.g. after AddControllersWithViews).</param>
        public static void RegisterComponents(IServiceCollection services)
        {
            // Original: container.RegisterType<IProductRepository, ProductRepository>()
            // Default Unity lifetime was Transient, preserved here as AddTransient.
            services.AddTransient<IProductRepository, ProductRepository>();
        }
    }
}
