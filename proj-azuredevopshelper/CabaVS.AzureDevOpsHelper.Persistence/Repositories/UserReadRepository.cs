using System.Data.Common;
using CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence.Repositories;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace CabaVS.AzureDevOpsHelper.Persistence.Repositories;

internal sealed class UserReadRepository(ApplicationDbContext dbContext) : IUserReadRepository
{
    private DbConnection GetDbConnection() => dbContext.Database.GetDbConnection();

    public async Task<bool> ExistsByExternalId(Guid externalId, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1
                FROM azdh.users
                WHERE external_id = @ExternalId
            );
            """;

        DbConnection connection = GetDbConnection();
        return await connection.ExecuteScalarAsync<bool>(
            sql,
            new { ExternalId = externalId });
    }
}
