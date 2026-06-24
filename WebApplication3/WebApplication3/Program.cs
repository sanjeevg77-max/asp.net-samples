
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.Common;
using WebApplication3.Components;

namespace WebApplication3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Razor Components with Interactive Server Components
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Add Distributed Memory Cache (required before AddSession)
            builder.Services.AddDistributedMemoryCache();

            // Add Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

// Register Oracle connection using ConnectionStrings from appsettings.json
            builder.Services.AddTransient<OracleConnection>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("ConnectionString1");
                return new OracleConnection(connectionString);
            });

            // Register Oracle DbProviderFactory
            DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess.Client", OracleClientFactory.Instance);

            // Register Oracle ManagedDataAccess DbProviderFactory
            DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess.Client", OracleClientFactory.Instance);

            // Add built-in logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var app = builder.Build();

            // 1. Exception handling
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            // 2. HTTPS redirection
            app.UseHttpsRedirection();

            // 3. Static files
            app.UseStaticFiles();

            // 4. Routing
            app.UseRouting();

            // 7. Session
            app.UseSession();

            // 8. Antiforgery
            app.UseAntiforgery();

            // 9. Endpoint mapping
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
