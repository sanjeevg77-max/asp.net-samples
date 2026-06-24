using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApp5Identity.Models;

namespace WebApp5Identity.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("InitialCreate")]
    internal sealed class Configuration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Initial schema is created by EF Core scaffolded Identity migrations.
            // Run: dotnet ef migrations add InitialCreate
            //      dotnet ef database update
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
