using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Taskify.Data;

/// <summary>
/// Permite comandos <c>dotnet ef</c> sin ejecutar el host completo.
/// Usa user-secrets / variables de entorno cuando existen.
/// </summary>
public sealed class TaskifyDbContextFactory : IDesignTimeDbContextFactory<TaskifyDbContext>
{
    public TaskifyDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets(typeof(TaskifyDbContext).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Host=127.0.0.1;Database=taskify;Username=postgres;Password=postgres";
        }

        var options = new DbContextOptionsBuilder<TaskifyDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TaskifyDbContext(options);
    }
}
