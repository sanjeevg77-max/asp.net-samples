
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oracle.ManagedDataAccess.Client;
using WebFormSiteOracleDB.Components;

namespace WebFormSiteOracleDB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Razor Components with Interactive Server Components
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Add HttpContext accessor
            builder.Services.AddHttpContextAccessor();

            // Add Distributed Memory Cache (required before AddSession)
            builder.Services.AddDistributedMemoryCache();

            // Add Session
            builder.Services.AddSession();

// Register Oracle connection using OracleDbContext connection string from appsettings.json
            builder.Services.AddTransient<OracleConnection>(sp =>
            {
                var configuration = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                var connectionString = configuration.GetConnectionString("OracleDbContext");
                return new OracleConnection(connectionString);
            });

            var app = builder.Build();

            // 1. Exception handling
            app.UseExceptionHandler("/Error");

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
