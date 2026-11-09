using Microsoft.EntityFrameworkCore.Design;
using NorthIsland.Server.Data.Sql;

namespace NorthIsland.Server.Data;

/// <summary>Creates the database context for the EF Core design-time tools (migrations), against a PostgreSQL placeholder connection.</summary>
public sealed class ServerDbContextDesignFactory : IDesignTimeDbContextFactory<ServerDbContext>
{
    public ServerDbContext CreateDbContext(string[] args) => new(StorageRegistration.Postgres("Host=localhost;Database=northisland"));
}
