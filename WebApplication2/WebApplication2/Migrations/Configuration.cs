namespace WebApplication2.Migrations
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using WebApplication2.Models;

    // NOTE: EF Core uses a different migration system. This seed configuration
    // has been retained for reference but is no longer used by DbMigrationsConfiguration.
    // To seed data in EF Core, override DbContext.OnModelCreating or use
    // IEntityTypeConfiguration, or call context.Database.EnsureCreated() with
    // HasData() in OnModelCreating.
    //
    // To re-create migrations for EF Core, run:
    //   dotnet ef migrations add InitialCreate
    //   dotnet ef database update

    internal sealed class Configuration
    {
        public static void Seed(ProductDBCintext context)
        {
            //  This method will be called after migrating to the latest version.
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product
                    {
                        Name = "Inductor 1uH THT",
                        ExpirationDate = DateTime.Parse("2014-01-01"),
                        QRCode = "29835612",
                        Price = 0.05M
                    },
                    new Product
                    {
                        Name = "Inductor 1uH SMD",
                        ExpirationDate = DateTime.Parse("2015-01-01"),
                        QRCode = "0348765",
                        Price = 0.1M
                    },
                    new Product
                    {
                        Name = "Capacitor 1uH SMD",
                        ExpirationDate = DateTime.Parse("2016-11-01"),
                        QRCode = "09374653",
                        Price = 0.08M
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
