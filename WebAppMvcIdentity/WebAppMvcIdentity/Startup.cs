// This file has been superseded by Program.cs as part of the migration to
// the .NET 8.0 minimal hosting model (WebApplication.CreateBuilder pattern).
//
// All service registrations previously in ConfigureServices() and all
// middleware previously in Configure() have been consolidated into Program.cs.
//
// The ConfigurationManager helper class is retained below for any remaining
// code that references it directly; it now reads from the DI-provided
// IConfiguration instance populated in Program.cs.

using Microsoft.Extensions.Configuration;

namespace WebAppMvcIdentity
{
    /// <summary>
    /// Compatibility shim: exposes the application IConfiguration statically
    /// for legacy code paths that cannot yet receive it via dependency injection.
    /// Populated during application startup in Program.cs.
    /// </summary>
    public static class ConfigurationManager
    {
        public static IConfiguration Configuration { get; set; } = default!;
    }
}
