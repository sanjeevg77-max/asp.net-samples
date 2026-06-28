namespace WebApp5Identity.Migrations
{
    using Microsoft.EntityFrameworkCore;
    using WebApp5Identity.Models;

    // EF Core does not use a DbMigrationsConfiguration class.
    // Migrations are managed via the EF Core CLI tools:
    //   Add-Migration <Name>
    //   Update-Database
    // Seed data should be provided via ApplicationDbContext.OnModelCreating
    // using modelBuilder.Entity<T>().HasData(...), or via IHostedService /
    // WebApplication startup code in Program.cs.
    //
    // This file is retained for reference only and is excluded from
    // compilation by the <Compile Remove="Migrations\**" /> entry in the
    // project file. Remove that exclusion and delete this file once
    // EF Core migrations have been scaffolded with:
    //   dotnet ef migrations add InitialCreate
}
