using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Server.Data.Sql;

namespace NorthIsland.Server.Data;

/// <summary>Registers the account and document stores.</summary>
public static class StorageRegistration
{
    /// <summary>The name of the connection string (<c>ConnectionStrings:Accounts</c>) used by the Postgres provider.</summary>
    public const string ConnectionStringName = "Accounts";

    /// <summary>
    /// Registers the account store and the document store: JSON files, or Postgres when <c>Database:Provider</c> is <c>Postgres</c>.
    /// </summary>
    /// <param name="services">The service collection to add the stores to.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddStorage(this IServiceCollection services)
    {
        services.AddSingleton<IAccountStore>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var dataDirectory = ServerPaths.Resolve(sp.GetRequiredService<IOptions<ServerOptions>>().Value.DataDirectory);
            var provider = config["Database:Provider"] ?? "Json";
            if (provider.Equals("Json", StringComparison.OrdinalIgnoreCase))
                return new JsonAccountStore(dataDirectory, sp.GetRequiredService<ILogger<JsonAccountStore>>());
            if (!provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Database:Provider is '{provider}'; use Json or Postgres.");

            var connection = config.GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException($"Database:Provider is Postgres but ConnectionStrings:{ConnectionStringName} is not set.");
            var import = config.GetValue("Database:ImportJson", true) ? dataDirectory : null;
            return new SqlAccountStore(Postgres(connection), sp.GetRequiredService<ILogger<SqlAccountStore>>(), migrate: true, import);
        });
        services.AddSingleton<IDocumentStore>(sp =>
        {
            var dataDirectory = ServerPaths.Resolve(sp.GetRequiredService<IOptions<ServerOptions>>().Value.DataDirectory);
            if (sp.GetRequiredService<IAccountStore>() is not SqlAccountStore sql)
                return new FileDocumentStore(dataDirectory);
            var import = sp.GetRequiredService<IConfiguration>().GetValue("Database:ImportJson", true) ? dataDirectory : null;
            return new SqlDocumentStore(sql.Options, import, sp.GetRequiredService<ILogger<SqlDocumentStore>>());
        });
        return services;
    }

    /// <summary>Builds the Entity Framework options for a Postgres database.</summary>
    /// <param name="connectionString">The Npgsql connection string.</param>
    /// <returns>The database context options.</returns>
    public static DbContextOptions<ServerDbContext> Postgres(string connectionString) =>
        new DbContextOptionsBuilder<ServerDbContext>().UseNpgsql(connectionString).Options;
}
