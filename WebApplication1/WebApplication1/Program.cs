
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.Common;
using WebApplication1.Components;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register Oracle DbProviderFactories
            DbProviderFactories.RegisterFactory(
                "Oracle.ManagedDataAccess.Client",
                Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance
            );

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
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

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