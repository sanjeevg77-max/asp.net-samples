// ----------------------------------------------------------------------------------
// Startup.cs – retained as an empty placeholder after migration to Program.cs.
//
// All startup logic previously in this file (OWIN Startup / CTA-generated
// Startup class) has been migrated to Program.cs using the ASP.NET Core 8
// WebApplication.CreateBuilder() minimal hosting model.
//
// Migration notes:
//   • OwinStartupAttribute + Configuration(IAppBuilder) → Program.cs
//   • ConfigureServices(IServiceCollection)            → Program.cs builder.Services.*
//   • Configure(IApplicationBuilder, IWebHostEnvironment) → Program.cs app.Use*()
//   • ConfigurationManager helper class                → use IConfiguration directly
//                                                         via DI (already available
//                                                         through builder.Configuration)
// ----------------------------------------------------------------------------------
// This file is intentionally left empty.
// The Generic Host / WebApplication entry point is in Program.cs.
