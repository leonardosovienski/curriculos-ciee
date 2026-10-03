using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Curriculos.Api.Data;

public static class DatabaseMigrator
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Aplica as migrations pendentes na inicialização.
    /// O retry cobre o caso de o SQL Server ainda estar subindo no container.
    /// </summary>
    public static async Task MigrateWithRetryAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigrator");

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                logger.LogInformation("Banco de dados pronto (migrations aplicadas).");
                return;
            }
            catch (SqlException ex) when (attempt < MaxAttempts)
            {
                logger.LogWarning(
                    "SQL Server indisponível (tentativa {Attempt}/{Max}): {Message}. Nova tentativa em {Delay}s...",
                    attempt, MaxAttempts, ex.Message, Delay.TotalSeconds);
                await Task.Delay(Delay);
            }
        }
    }
}
